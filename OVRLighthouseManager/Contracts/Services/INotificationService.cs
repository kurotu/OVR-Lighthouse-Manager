using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.UI.Xaml.Controls;

namespace OVRLighthouseManager.Contracts.Services;

public interface INotificationService
{
    public void SetNotificationQueue(StackedNotificationsBehavior notificationQueue);
    public void Show(Notification notification);
    public void Information(string message);
    public void Success(string message);
    public void Warning(string message);
    public void Error(string message);
}
