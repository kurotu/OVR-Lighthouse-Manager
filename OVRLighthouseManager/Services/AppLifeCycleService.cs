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
    private readonly ILighthouseService _lighthouseService;
    private readonly IOverlayAppService _overlayAppService;
    private readonly ScanCommand _scanCommand;

    public AppLifeCycleService(ILighthouseSettingsService lighthouseSettingsService, ILighthouseService lighthouseService, IOverlayAppService overlayAppService, ScanCommand scanCommand)
    {
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        _lighthouseSettingsService = lighthouseSettingsService;
        _lighthouseService = lighthouseService;
        _overlayAppService = overlayAppService;
        _scanCommand = scanCommand;
    }

    public void Initialize()
    {
        _overlayAppService.OnVRMonitorConnected += async (_, __) =>
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
        _overlayAppService.OnVRSystemQuit += async (_, __) =>
        {
            try
            {
                await OnVRSystemQuit();
                await OnBeforeAppExit();

                dispatcherQueue.TryEnqueue(() =>
                {
                    App.Current.Exit();
                });
            }
            catch (Exception e)
            {
                Log.Error(e, "OnVRSystemQuit Failed");
            }
        };
        if (_overlayAppService.IsVRMonitorConnected)
        {
            try
            {
                OnVRMonitorConnected().Wait();
            }
            catch (Exception e)
            {
                Log.Error(e, "OnVRMonitorConnected Failed");
            }
        }
    }

    public async Task OnBeforeAppExit()
    {
        _overlayAppService.Shutdown();
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
            const int retry = 20;
            LighthouseDevice? device = null;
            for (var i = 0; i < retry; i++)
            {
                device = _lighthouseService.GetLighthouse(d.BluetoothAddress);
                if (device != null && device.IsInitialized)
                {
                    break;
                }
                await Task.Delay(1000);
            }
            if (device == null || !device.IsInitialized)
            {
                Log.Information($"Failed to get device {d.Name}");
                return;
            }
            Log.Information($"Power On {d.Name}");
            var result = await device.PowerOnAsync();
            Log.Information($"Done {d.Name}: {result}");
        }).ToArray());

        await _scanCommand.StopScan();
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
            await Task.Run(async () =>
            {
                const int retry = 20;
                LighthouseDevice? device = null;
                for (var i = 0; i < retry; i++)
                {

                    device = _lighthouseService.GetLighthouse(d.BluetoothAddress);
                    if (device != null && device.IsInitialized)
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }
                if (device == null || !device.IsInitialized)
                {
                    Log.Information($"Failed to get device {d.Name}");
                    return;
                }
                Log.Information($"Sleep {d.Name}");
                var result = await device.SleepAsync();
                Log.Information($"Done {d.Name}: {result}");
            });
        }).ToArray());

        await _scanCommand.StopScan();
        Log.Information("OnVRSystemQuit Done");
    }
}
