using System.Diagnostics;
using System.IO;

namespace OVRLighthouseManagerSetupActions
{
    [System.ComponentModel.RunInstaller(true)]
    public class CustomActions : System.Configuration.Install.Installer
    {
        private const string exeFileName = "OVRLighthouseManager.exe";

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            var dir = Context.Parameters["targetdir"];
            var exe = Path.Combine(dir, exeFileName);
            var info = new ProcessStartInfo(exe, "install")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var process = Process.Start(info);
            process.WaitForExit();
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);

            var dir = Context.Parameters["targetdir"];
            var exe = Path.Combine(dir, exeFileName);
            var info = new ProcessStartInfo(exe, "uninstall")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var process = Process.Start(info);
            process.WaitForExit();
        }
    }
}
