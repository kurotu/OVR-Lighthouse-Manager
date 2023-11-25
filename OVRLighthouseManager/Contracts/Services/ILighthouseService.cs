using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OVRLighthouseManager.Models;

namespace OVRLighthouseManager.Contracts.Services;
public interface ILighthouseService
{
    public event EventHandler<LighthouseDevice> OnFound;

    public List<LighthouseDevice> KnownDevices { get; }

    public void StartScan();
    public void StopScan();
    public Task StopScanAsync();

    public void AddDevice(LighthouseDevice device);

    public Task<LighthouseDevice> GetDeviceAsync(ulong bluetoothAddress);
    public Task<LighthouseDevice> GetDeviceAsync(string bluetoothAddress);
}
