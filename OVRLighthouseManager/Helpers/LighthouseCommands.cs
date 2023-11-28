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
