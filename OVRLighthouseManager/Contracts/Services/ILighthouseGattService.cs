using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OVRLighthouseManager.Models;

namespace OVRLighthouseManager.Contracts.Services;
internal interface ILighthouseGattService
{
    public Task PowerOnAsync(Lighthouse lighthouse);

    public Task SleepAsync(Lighthouse lighthouse);

    public Task StandbyAsync(Lighthouse lighthouse);
}
