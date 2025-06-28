using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OVRLighthouseManager.Helpers;

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync(object parameter);
}

public abstract class AsyncCommandBase : IAsyncCommand
{
    public abstract event EventHandler? CanExecuteChanged;

    public abstract bool CanExecute(object? parameter);
    public abstract Task ExecuteAsync(object? parameter);
    public async void Execute(object? parameter)
    {
        await ExecuteAsync(parameter);
    }
}
