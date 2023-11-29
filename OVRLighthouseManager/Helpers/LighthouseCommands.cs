using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Services;
using OVRLighthouseManager.ViewModels;
using Serilog;

namespace OVRLighthouseManager.Helpers;

public class ScanCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    private readonly ILighthouseService _lighthouseService;
    private readonly INotificationService _notificationService;
    private readonly ILogger _log = LogHelper.ForContext<ScanCommand>();
    private bool _shouldStop = false;

    public ScanCommand(ILighthouseService lighthouseService, INotificationService notificationService)
    {
        _lighthouseService = lighthouseService;
        _notificationService = notificationService;
    }

    public bool CanExecute(object? parameter)
    {
        return !_lighthouseService.IsScanning && LighthouseService.HasBluetoothAdapter();
    }

    public async void Execute(object? parameter)
    {
        _log.Debug("Execute");
        _notificationService.Information("Scanning for lighthouses");
        _shouldStop = false;
        _lighthouseService.StartScan();
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        for (var i = 0; i < 100; i++)
        {
            await Task.Delay(100);
            if (_shouldStop)
            {
                break;
            }
        }
        _lighthouseService.StopScan();
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        _log.Debug("Execute done");
    }

    public async Task StopScan()
    {
        _log.Debug("Stop scan");
        _shouldStop = true;
        while (_lighthouseService.IsScanning)
        {
            await Task.Delay(100);
        }
        _log.Debug("Stop scan done");
    }
}

public class PowerOnCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    private Task<bool>? _task;

    public bool CanExecute(object? parameter)
    {
        return _task == null || _task.IsCompleted;
    }

    public async void Execute(object? parameter)
    {
        if (parameter is LighthouseObject lighthouse)
        {
            Log.Information("Powering on {@lighthouse}", lighthouse);
            var device = App.GetService<ILighthouseService>().GetLighthouse(lighthouse.BluetoothAddress);
            if (device == null)
            {
                throw new Exception("Device not found");
            }
            _task = device.PowerOnAsync();
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            var result = await _task;
            Log.Information("PowerOn result: {result}", result);
            _task = null;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            throw new Exception("Invalid parameter");
        }
    }
}

public class SleepCommand : ICommand
{

    public event EventHandler? CanExecuteChanged;
    private Task<bool>? _task;

    public bool CanExecute(object? parameter)
    {
        return _task == null || _task.IsCompleted;
    }

    public async void Execute(object? parameter)
    {
        if (parameter is LighthouseObject lighthouse)
        {
            Log.Information("Sleeping {@lighthouse}", lighthouse);
            var device = App.GetService<ILighthouseService>().GetLighthouse(lighthouse.BluetoothAddress);
            if (device == null)
            {
                           throw new Exception("Device not found");
                       }
            _task = device.SleepAsync();
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            var reulst = await _task;
            Log.Information("Sleep result: {result}", reulst);
            _task = null;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            throw new Exception("Invalid parameter");
        }
    }
}

public class StandbyCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    private Task<bool>? _task;

    public bool CanExecute(object? parameter)
    {
        return _task == null || _task.IsCompleted;
    }

    public async void Execute(object? parameter)
    {

        if (parameter is LighthouseObject lighthouse)
        {
            Log.Information("Standby {@lighthouse}", lighthouse);
            var device = App.GetService<ILighthouseService>().GetLighthouse(lighthouse.BluetoothAddress);
            if (device == null)
            {
                throw new Exception("Device not found");
            }
            _task = device.StandbyAsync();
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            var result = await _task;
            Log.Information("Standby result: {result}", result);
            _task = null;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            throw new Exception("Invalid parameter");
        }
    }
}
