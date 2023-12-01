using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using OVRLighthouseManager.Contracts.Services;
using OVRSharp;
using Serilog;
using Valve.VR;

namespace OVRLighthouseManager.Services;
public class OpenVRService : IOpenVRService
{
    public bool IsVRMonitorConnected
    {
        get; set;
    }

    private Application? _application;

    private const int PollingRate = 20;

    private Thread? _pollThread = null;

    private readonly ILogger _log = Helpers.LogHelper.ForContext<OpenVRService>();

    public event EventHandler<VREvent_t> OnVRMonitorConnected = delegate { };
    public event EventHandler<VREvent_t> OnVRSystemQuit = delegate { };

    public OpenVRService()
    {
        _application = new Application(Application.ApplicationType.Utility);
        if (Process.GetProcessesByName("vrmonitor").Any())
        {
            _log.Information("VRMonitor already running");
            IsVRMonitorConnected = true;
        }
        StartPolling();
    }

    public void Shutdown()
    {
        StopPolling();
        _application?.Shutdown();
        _application = null;
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
        while (_pollThread != null && _application != null)
        {
            var pEvent = default(VREvent_t);
            var uncbVREvent = (uint)Marshal.SizeOf(typeof(VREvent_t));
            if (!_application.OVRSystem.PollNextEvent(ref pEvent, uncbVREvent))
            {
                Thread.Sleep(PollingRate);
                continue;
            }

            // _log.Debug("Polling event {eventType}", (EVREventType)pEvent.eventType);

            switch ((EVREventType)pEvent.eventType)
            {
                case EVREventType.VREvent_ProcessConnected:
                    {
                        var pid = pEvent.data.process.pid;
                        var process = Process.GetProcessById((int)pid);
                        if (process.ProcessName == "vrmonitor")
                        {
                            _log.Information("VRMonitor connected");
                            IsVRMonitorConnected = true;
                            OnVRMonitorConnected(this, pEvent);
                        }
                    }
                    break;
                case EVREventType.VREvent_Quit:
                    _log.Information("VRSystem quit");
                    IsVRMonitorConnected = false;
                    OnVRSystemQuit(this, pEvent);
                    Shutdown();
                    break;
            }
        }
    }

    public void AddApplicationManifest(string applicationManifestPath)
    {
        if (_application == null)
        {
            throw new Exception("OpenVRService not initialized");
        }
        _log.Information($"Adding application manifest {applicationManifestPath}");
        OpenVR.Applications.AddApplicationManifest(applicationManifestPath, false);
    }

    public void RemoveApplicationManifest(string applicationManifestPath)
    {
        if (_application == null)
        {
            throw new Exception("OpenVRService not initialized");
        }
        _log.Information($"Removing application manifest {applicationManifestPath}");
        OpenVR.Applications.RemoveApplicationManifest(applicationManifestPath);
    }

    public void SetApplicationAutoLaunch(string applicationKey, bool autoLaunch)
    {
        if (_application == null)
        {
            throw new Exception("OpenVRService not initialized");
        }
        _log.Information($"Setting application {applicationKey} auto launch to {autoLaunch}");
        OpenVR.Applications.SetApplicationAutoLaunch(applicationKey, autoLaunch);
    }
}
