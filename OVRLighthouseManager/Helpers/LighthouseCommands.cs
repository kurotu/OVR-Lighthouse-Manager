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

public class ScanCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    private readonly ILighthouseDiscoveryService _lighthouseService;
    private readonly INotificationService _notificationService;
    private readonly ILogger _log = LogHelper.ForContext<ScanCommand>();
    private bool _shouldStop = false;

    public ScanCommand(ILighthouseDiscoveryService lighthouseService, INotificationService notificationService)
    {
        _lighthouseService = lighthouseService;
        _notificationService = notificationService;
    }

    public bool CanExecute(object? parameter)
    {
        return !_lighthouseService.IsDiscovering && BluetoothLEHelper.HasBluetoothLEAdapter();
    }

    public async void Execute(object? parameter)
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

public class PowerOnCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    private Task? _task;
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
                var l = new Lighthouse { Name = lighthouse.Name, BluetoothAddress = AddressToStringConverter.StringToAddress(lighthouse.BluetoothAddress), Id = lighthouse.Id };
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

public class SleepCommand : ICommand
{

    public event EventHandler? CanExecuteChanged;
    private Task? _task;
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
                var l = new Lighthouse { Name = lighthouse.Name, BluetoothAddress = AddressToStringConverter.StringToAddress(lighthouse.BluetoothAddress), Id = lighthouse.Id };
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

public class StandbyCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    private Task? _task;
    private readonly ILogger _log = LogHelper.ForContext<StandbyCommand>();

    public bool CanExecute(object? parameter)
    {
        var isV1 = parameter is LighthouseObject lighthouse && new Lighthouse { Name = lighthouse.Name }.Version == LighthouseVersion.V1;
        if (isV1)
        {
            return false;
        }
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
                var l = new Lighthouse { Name = lighthouse.Name, BluetoothAddress = AddressToStringConverter.StringToAddress(lighthouse.BluetoothAddress), Id = lighthouse.Id };
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
