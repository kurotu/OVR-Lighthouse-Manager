using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVRLighthouseManager.Contracts.Services;
public interface IMiscSettingsService
{
    public bool OutputDebug
    {
        get;
    }

    public Task InitializeAsync();
    public Task SetOutputDebugAsync(bool outputDebug);
}
