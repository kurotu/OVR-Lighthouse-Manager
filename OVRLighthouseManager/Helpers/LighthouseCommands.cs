using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Services;
using OVRLighthouseManager.ViewModels;

namespace OVRLighthouseManager.Helpers;

public class PowerOnCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    private Task? _task;

    public bool CanExecute(object? parameter)
    {
        return _task == null || _task.IsCompleted;
    }

    public async void Execute(object? parameter)
    {
        if (parameter is LighthouseObject lighthouse)
        {
            var device = await App.GetService<ILighthouseService>().GetDeviceAsync(lighthouse.BluetoothAddress);
            _task = device.PowerOnAsync();
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            await _task;
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
    private Task? _task;

    public bool CanExecute(object? parameter)
    {
        return _task == null || _task.IsCompleted;
    }

    public async void Execute(object? parameter)
    {
        if (parameter is LighthouseObject lighthouse)
        {

            var device = await App.GetService<ILighthouseService>().GetDeviceAsync(lighthouse.BluetoothAddress);
            _task = device.SleepAsync();
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            await _task;
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
    private Task? _task;

    public bool CanExecute(object? parameter)
    {
        return _task == null || _task.IsCompleted;
    }

    public async void Execute(object? parameter)
    {

        if (parameter is LighthouseObject lighthouse)
        {
            var device = await App.GetService<ILighthouseService>().GetDeviceAsync(lighthouse.BluetoothAddress);
            _task = device.StandbyAsync();
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            await _task;
            _task = null;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            throw new Exception("Invalid parameter");
        }
    }
}