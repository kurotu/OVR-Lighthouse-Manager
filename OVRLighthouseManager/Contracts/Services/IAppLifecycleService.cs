using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVRLighthouseManager.Contracts.Services;

public interface IAppLifecycleService
{
    public void Initialize();
    public Task OnBeforeAppExit();
}
