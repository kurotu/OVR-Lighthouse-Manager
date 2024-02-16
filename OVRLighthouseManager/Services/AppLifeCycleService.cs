using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;
using OVRLighthouseManager.Models;
using Serilog;
using Valve.VR;

namespace OVRLighthouseManager.Services;

class AppLifeCycleService : IAppLifecycleService
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private readonly ILighthouseSettingsService _lighthouseSettingsService;
    private readonly ILighthouseGattService _lighthouseGattService;
    private readonly IOpenVRService _openVRService;
    private readonly ScanCommand _scanCommand;

    public AppLifeCycleService(ILighthouseSettingsService lighthouseSettingsService, ILighthouseGattService lighthouseGattService, IOpenVRService openVRService, ScanCommand scanCommand)
    {
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        _lighthouseSettingsService = lighthouseSettingsService;
        _lighthouseGattService = lighthouseGattService;
        _openVRService = openVRService;
        _scanCommand = scanCommand;
    }

    public async Task OnLaunch()
    {
        _openVRService.OnVRMonitorConnected += async (_, __) =>
        {
            try
            {
                await OnVRMonitorConnected();
            }
            catch (Exception e)
            {
                Log.Error(e, "OnVRMonitorConnected Failed");
            }
        };
        _openVRService.OnVRSystemQuit += async (_, __) =>
        {
            try
            {
                await OnVRSystemQuit();
            }
            catch (Exception e)
            {
                Log.Error(e, "OnVRSystemQuit Failed");
            }
        };
        if (_openVRService.IsVRMonitorConnected)
        {
            try
            {
                await OnVRMonitorConnected();
            }
            catch (Exception e)
            {
                Log.Error(e, "OnVRMonitorConnected Failed");
            }
        }
    }

    public async Task OnBeforeAppExit()
    {
        _openVRService.Shutdown();
        await _scanCommand.StopScan();
    }

    private async Task OnVRMonitorConnected()
    {
        Log.Information("OnVRMonitorConnected");
        if (!_lighthouseSettingsService.PowerManagement)
        {
            return;
        }

        if (_scanCommand.CanExecute(null))
        {
            _scanCommand.Execute(null);
        }

        var managedDevices = _lighthouseSettingsService.Devices.Where(d => d.IsManaged).ToArray();
        await Task.WhenAll(managedDevices.Select(async d =>
        {
            Lighthouse lighthouse = new Lighthouse()
            {
                Name = d.Name,
                BluetoothAddress = AddressToStringConverter.StringToAddress(d.BluetoothAddress),
            };
            Log.Information($"Power On {d.Name}");
            try
            {
                await _lighthouseGattService.PowerOnAsync(lighthouse);
                Log.Information($"Done {d.Name}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to power on {d.Name}");
            }
        }).ToArray());

        Log.Information("OnVRMonitorConnected Done");
    }


    private async Task OnVRSystemQuit()
    {
        Log.Information("OnVRSystemQuit");
        if (!_lighthouseSettingsService.PowerManagement)
        {
            return;
        }

        if (_scanCommand.CanExecute(null))
        {
            _scanCommand.Execute(null);
        }

        var managedDevices = _lighthouseSettingsService.Devices.Where(d => d.IsManaged).ToArray();
        await Task.WhenAll(managedDevices.Select(async d =>
        {
            Lighthouse lighthouse = new Lighthouse()
            {
                Name = d.Name,
                BluetoothAddress = AddressToStringConverter.StringToAddress(d.BluetoothAddress),
            };
            Log.Information($"Sleep {d.Name}");
            try
            {
                await _lighthouseGattService.SleepAsync(lighthouse);
                Log.Information($"Done {d.Name}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to sleep {d.Name}");
            }
        }).ToArray());

        await _scanCommand.StopScan();

        dispatcherQueue.TryEnqueue(() =>
        {
            App.Current.Exit();
        });

        Log.Information("OnVRSystemQuit Done");
    }
}
