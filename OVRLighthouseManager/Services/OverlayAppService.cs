using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OVRLighthouseManager.Contracts.Services;
using OVRSharp;
using Valve.VR;

namespace OVRLighthouseManager.Services;
public class OverlayAppService : IOverlayAppService
{
    public bool IsVRMonitorConnected { get; set; }

    private readonly Application _application;

    private const int PollingRate = 20;

    private Thread? _pollThread = null;

    public event EventHandler<VREvent_t> OnVRMonitorConnected = delegate { };
    public event EventHandler<VREvent_t> OnVRSystemQuit = delegate { };

    public OverlayAppService()
    {
        _application = new Application(Application.ApplicationType.Utility);
        if (Process.GetProcessesByName("vrmonitor").Any())
        {
            IsVRMonitorConnected = true;
        }
        StartPolling();
    }

    public void Shutdown()
    {
        StopPolling();
        _application.Shutdown();
    }

    public void StartPolling()
    {
        _pollThread = new Thread(Poll);
        _pollThread.Start();
    }

    public void StopPolling()
    {
        _pollThread = null;
    }


    private void Poll()
    {
        while (_pollThread != null)
        {
            var pEvent = default(VREvent_t);
            var uncbVREvent = (uint)Marshal.SizeOf(typeof(VREvent_t));
            if (!_application.OVRSystem.PollNextEvent(ref pEvent, uncbVREvent))
            {
                Thread.Sleep(PollingRate);
                continue;
            }

            // System.Diagnostics.Debug.WriteLine($"OverlayAppService: {(EVREventType)pEvent.eventType}");

            switch ((EVREventType)pEvent.eventType)
            {
                case EVREventType.VREvent_ProcessConnected:
                    {
                        var pid = pEvent.data.process.pid;
                        var process = Process.GetProcessById((int)pid);
                        if (process.ProcessName == "vrmonitor")
                        {
                            IsVRMonitorConnected = true;
                            OnVRMonitorConnected(this, pEvent);
                        }
                    }
                    break;
                case EVREventType.VREvent_Quit:
                    IsVRMonitorConnected = false;
                    OnVRSystemQuit(this, pEvent);
                    break;
            }
        }
    }

    private void OnOverlayUnknown(object? sender, VREvent_t e)
    {
        var type = (EVREventType)e.eventType;
        switch (type)
        {
            case EVREventType.VREvent_Quit:
                OnVRSystemQuit(sender, e);
                break;
        }
    }
}
