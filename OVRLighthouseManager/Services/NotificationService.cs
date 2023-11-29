using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.UI.Xaml.Controls;
using OVRLighthouseManager.Contracts.Services;

namespace OVRLighthouseManager.Services;
public class NotificationService : INotificationService
{
    private StackedNotificationsBehavior? _notificationQueue;
    private Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;

    public void SetNotificationQueue(StackedNotificationsBehavior notificationQueue)
    {
        _notificationQueue = notificationQueue;
        _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    }

    public void Show(Notification notification)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            _notificationQueue?.Show(notification);
        });
    }

    public void Information(string message)
    {
        var notification = new Notification()
        {
            Message = message,
            Duration = TimeSpan.FromSeconds(5),
            Severity = InfoBarSeverity.Informational,
            IsIconVisible = true,
        };
        Show(notification);
    }

    public void Warning(string message)
    {
        var notification = new Notification()
        {

            Message = message,
            Duration = TimeSpan.FromSeconds(5),
            Severity = InfoBarSeverity.Warning,
            IsIconVisible = true,
        };
        Show(notification);
    }

    public void Error(string message)
    {
        var notification = new Notification()
        {
            Message = message,
            Duration = TimeSpan.FromSeconds(5),
            Severity = InfoBarSeverity.Error,
            IsIconVisible = true,
        };
        Show(notification);
    }
}
