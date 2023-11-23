using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVRLighthouseManager.Contracts.Services;
public interface IOpenVRService
{
    public void Initialize();
    public void Shutdown();
}
