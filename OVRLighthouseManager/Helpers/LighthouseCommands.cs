using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Models;
using OVRLighthouseManager.Services;
using OVRLighthouseManager.ViewModels;
using Serilog;

namespace OVRLighthouseManager.Helpers;

public class ScanCommand : AsyncCommandBase
{
    public override event EventHandler? CanExecuteChanged;

    private readonly ILighthouseDiscoveryService _lighthouseService;
    private readonly INotificationService _notificationService;
    private readonly ILogger _log = LogHelper.ForContext<ScanCommand>();
    private bool _shouldStop = false;

    public ScanCommand(ILighthouseDiscoveryService lighthouseService, INotificationService notificationService)
    {
        _lighthouseService = lighthouseService;
        _notificationService = notificationService;
    }

    public override bool CanExecute(object? parameter)
    {
        return !_lighthouseService.IsDiscovering && BluetoothLEHelper.HasBluetoothLEAdapter();
    }

    public async override Task ExecuteAsync(object? parameter)
    {
        _log.Debug("Execute");
        _notificationService.Information("Notification_Scanning".GetLocalized());
        _shouldStop = false;
        _lighthouseService.StartDiscovery();
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        for (var i = 0; i < 100; i++)
        {
            await Task.Delay(100);
            if (_shouldStop)
            {
                break;
            }
        }
        _lighthouseService.StopDiscovery();
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        _log.Debug("Execute done");
    }

    public async Task StopScan()
    {
        _log.Debug("Stop scan");
        _shouldStop = true;
        while (_lighthouseService.IsDiscovering)
        {
            await Task.Delay(100);
        }
        _log.Debug("Stop scan done");
    }
}

public class PowerOnCommand : AsyncCommandBase
{
    public override event EventHandler? CanExecuteChanged;
    private Task? _task;
    private readonly ILogger _log = LogHelper.ForContext<PowerOnCommand>();

    public override bool CanExecute(object? parameter)
    {
        return _task == null || _task.IsCompleted;
    }

    public async override Task ExecuteAsync(object? parameter)
    {
        if (parameter is LighthouseObject lighthouse)
        {
            var notification = App.GetService<INotificationService>();
            try
            {
                _log.Information($"{lighthouse.Name} Powering on");
                var l = new Lighthouse { Name = lighthouse.Name, BluetoothAddress = lighthouse.BluetoothAddress, Id = lighthouse.Id };
                _task = App.GetService<ILighthouseGattService>().PowerOnAsync(l);
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                await _task;
                _task = null;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                notification.Success(string.Format("Notification_PowerOn".GetLocalized(), lighthouse.Name));
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"{lighthouse.Name} Failed to power on");
                notification.Error(string.Format("Notification_CommunicationError".GetLocalized(), lighthouse.Name) + "\n" + ex.Message);
            }
        }
        else
        {
            throw new ArgumentException("Invalid parameter");
        }
    }
}

public class SleepCommand : AsyncCommandBase
{

    public override event EventHandler? CanExecuteChanged;
    private Task? _task;
    private readonly ILogger _log = LogHelper.ForContext<SleepCommand>();

    public override bool CanExecute(object? parameter)
    {
        return _task == null || _task.IsCompleted;
    }

    public async override Task ExecuteAsync(object? parameter)
    {
        if (parameter is LighthouseObject lighthouse)
        {
            var notification = App.GetService<INotificationService>();
            try
            {
                _log.Information($"{lighthouse.Name} Sleeping");
                var l = new Lighthouse { Name = lighthouse.Name, BluetoothAddress = lighthouse.BluetoothAddress, Id = lighthouse.Id };
                _task = App.GetService<ILighthouseGattService>().SleepAsync(l);
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                await _task;
                _task = null;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                notification.Success(string.Format("Notification_Sleep".GetLocalized(), lighthouse.Name));
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"{lighthouse.Name} Failed to sleep");
                notification.Error(string.Format("Notification_CommunicationError".GetLocalized(), lighthouse.Name) + "\n" + ex.Message);
            }
        }
        else
        {
            throw new ArgumentException("Invalid parameter");
        }
    }
}

public class StandbyCommand : AsyncCommandBase
{
    public override event EventHandler? CanExecuteChanged;
    private Task? _task;
    private readonly ILogger _log = LogHelper.ForContext<StandbyCommand>();

    public override bool CanExecute(object? parameter)
    {
        var isV1 = parameter is LighthouseObject lighthouse && new Lighthouse { Name = lighthouse.Name }.Version == LighthouseVersion.V1;
        if (isV1)
        {
            return false;
        }
        return _task == null || _task.IsCompleted;
    }

    public async override Task ExecuteAsync(object? parameter)
    {
        if (parameter is LighthouseObject lighthouse)
        {
            var notification = App.GetService<INotificationService>();
            try
            {
                _log.Information($"{lighthouse.Name} Standby");
                var l = new Lighthouse { Name = lighthouse.Name, BluetoothAddress = lighthouse.BluetoothAddress, Id = lighthouse.Id };
                _task = App.GetService<ILighthouseGattService>().StandbyAsync(l);
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                await _task;
                _task = null;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                notification.Success(string.Format("Notification_Standby".GetLocalized(), lighthouse.Name));
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"{lighthouse.Name} Failed to standby");
                notification.Error(string.Format("Notification_CommunicationError".GetLocalized(), lighthouse.Name) + "\n" + ex.Message);
            }
        }
        else
        {
            throw new ArgumentException("Invalid parameter");
        }
    }
}

public class PowerAllCommand : AsyncCommandBase
{
    public override event EventHandler? CanExecuteChanged;

    public PowerAllCommandOperation Operation
    {
        get; private set;
    }

    public override bool CanExecute(object? parameter)
    {
        var param = parameter as PowerAllCommandParameter;
        return Operation == PowerAllCommandOperation.None && param.Lighthouses.Any(l => l.IsManaged);
    }

    public async override Task ExecuteAsync(object? parameter)
    {
        try
        {
            var param = parameter as PowerAllCommandParameter;

            Operation = param.Command switch
            {
                "powerOn" => PowerAllCommandOperation.PowerOn,
                "powerDown" => PowerAllCommandOperation.PowerDown,
                _ => PowerAllCommandOperation.None
            };
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

            var settings = App.GetService<ILighthouseSettingsService>();
            foreach (var lighthouse in param.Lighthouses.Where(l => l.IsManaged))
            {
                switch (Operation)
                {
                    case PowerAllCommandOperation.PowerOn:
                        var powerOn = new PowerOnCommand();
                        await powerOn.ExecuteAsync(lighthouse);
                        break;
                    case PowerAllCommandOperation.PowerDown:
                        if (settings.PowerDownMode == PowerDownMode.Sleep || lighthouse.Lighthouse.Version == LighthouseVersion.V1)
                        {
                            var sleep = new SleepCommand();
                            await sleep.ExecuteAsync(lighthouse);
                        }
                        else
                        {
                            var standby = new StandbyCommand();
                            await standby.ExecuteAsync(lighthouse);
                        }
                        break;
                    default:
                        continue;
                }
            }
        }
        finally
        {
            Operation = PowerAllCommandOperation.None;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

public class PowerAllCommandParameter
{
    public string Command { get; set; } = string.Empty;
    public IEnumerable<LighthouseObject> Lighthouses { get; set; } = Enumerable.Empty<LighthouseObject>();
}
