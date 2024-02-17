using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVRLighthouseManager.Contracts.Services;
public interface IMiscSettingsService
{
    public bool MinimizeOnLaunchedByOpenVR
    {
        get;
    }

    public bool MinimizeToTray
    {
        get;
    }

    public bool OutputDebug
    {
        get;
    }

    public Task InitializeAsync();
    public Task SetMinimizeOnLaunchedByOpenVRAsync(bool minimizeOnLaunchedByOpenVR);
    public Task SetMinimizeToTray(bool minimizeToTray);
    public Task SetOutputDebugAsync(bool outputDebug);
}
