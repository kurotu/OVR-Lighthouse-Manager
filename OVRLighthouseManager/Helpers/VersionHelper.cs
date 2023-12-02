using System.Reflection;

namespace OVRLighthouseManager.Helpers;
internal static class VersionHelper
{
    public static string GetInformationalVersion()
    {
        var attribute = Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
        return attribute?.InformationalVersion ?? "Unknown";
    }

    public static string GetVersion()
    {
        var version = GetInformationalVersion();
        return version.Split('+').First();
    }

    public static string? GetCommit()
    {
        var version = GetInformationalVersion();
        var split = version.Split('+');
        if (split.Length > 1)
        {
            return split[1];
        }
        return null;
    }
}
