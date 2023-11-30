namespace OVRLighthouseManagerSetupActions
{
    [System.ComponentModel.RunInstaller(true)]
    public class CustomActions : System.Configuration.Install.Installer
    {
        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);
        }
    }
}
