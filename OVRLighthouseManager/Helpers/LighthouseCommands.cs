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
    private readonly ILogger _log = LogHelper.ForContext<PowerOnCommand>();

    public bool CanExecute(object? parameter)
    {
        return _task == null || _task.IsCompleted;
    }

    public async void Execute(object? parameter)
    {
        if (parameter is LighthouseObject lighthouse)
        {
            var notification = App.GetService<INotificationService>();
            try
            {
                _log.Information($"{lighthouse.Name} Powering on");
                var device = App.GetService<ILighthouseService>().GetLighthouse(lighthouse.BluetoothAddress);
                if (device == null)
                {
                    throw new Exception("Device not found");
                }
                _task = device.PowerOnAsync();
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                var result = await _task;
                _log.Information($"{lighthouse.Name} Power on result: {result}");
                _task = null;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);

                if (result)
                {
                    notification.Information($"{lighthouse.Name} powered on.");
                }
                else
                {
                    notification.Error($"{lighthouse.Name} failed to power on.");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"{lighthouse.Name} Failed to power on");
                notification.Error($"{lighthouse.Name} failed to power on.");
            }
        }
        else
        {
            throw new ArgumentException("Invalid parameter");
        }
    }
}

public class SleepCommand : ICommand
{

    public event EventHandler? CanExecuteChanged;
    private Task<bool>? _task;
    private readonly ILogger _log = LogHelper.ForContext<SleepCommand>();

    public bool CanExecute(object? parameter)
    {
        return _task == null || _task.IsCompleted;
    }

    public async void Execute(object? parameter)
    {
        if (parameter is LighthouseObject lighthouse)
        {
            var notification = App.GetService<INotificationService>();
            try
            {
                _log.Information($"{lighthouse.Name} Sleeping");
                var device = App.GetService<ILighthouseService>().GetLighthouse(lighthouse.BluetoothAddress);
                if (device == null)
                {
                    throw new Exception("Device not found");
                }
                _task = device.SleepAsync();
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                var reulst = await _task;
                _log.Information($"{lighthouse.Name} Sleep result: {reulst}");
                _task = null;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);

                if (reulst)
                {
                    notification.Information($"{lighthouse.Name} slept.");
                }
                else
                {
                    notification.Error($"{lighthouse.Name} failed to sleep.");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"{lighthouse.Name} Failed to sleep");
                notification.Error($"{lighthouse.Name} failed to sleep.");
            }
        }
        else
        {
            throw new ArgumentException("Invalid parameter");
        }
    }
}

public class StandbyCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    private Task<bool>? _task;
    private readonly ILogger _log = LogHelper.ForContext<StandbyCommand>();

    public bool CanExecute(object? parameter)
    {
        return _task == null || _task.IsCompleted;
    }

    public async void Execute(object? parameter)
    {
        if (parameter is LighthouseObject lighthouse)
        {
            var notification = App.GetService<INotificationService>();
            try
            {
                _log.Information($"{lighthouse.Name} Standby");
                var device = App.GetService<ILighthouseService>().GetLighthouse(lighthouse.BluetoothAddress);
                if (device == null)
                {
                    throw new Exception("Device not found");
                }
                _task = device.StandbyAsync();
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                var result = await _task;
                _log.Information($"{lighthouse.Name} Standby result: {result}");
                _task = null;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);

                if (result)
                {
                    notification.Information($"{lighthouse.Name} standby.");
                }
                else
                {
                    notification.Error($"{lighthouse.Name} failed to standby.");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"{lighthouse.Name} Failed to standby");
                notification.Error($"{lighthouse.Name} failed to standby.");
            }
        }
        else
        {
            throw new ArgumentException("Invalid parameter");
        }
    }
}
