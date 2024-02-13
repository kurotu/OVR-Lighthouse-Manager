using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace OVRLighthouseManager.Helpers;
internal static class LogHelper
{
    public static void InitializeLogger(bool enableDebugOutput)
    {
        var logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OVRLighthouseManager", "log-.txt");
        var messageTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
        var conf = new LoggerConfiguration()
            .Enrich.WithProperty("SourceContext", "OVRLighthouseManager")
            .WriteTo.File(logFile,
                rollingInterval: RollingInterval.Day,
                outputTemplate: messageTemplate)
#if DEBUG
            .WriteTo.Debug(outputTemplate: messageTemplate)
            .MinimumLevel.Debug()
#endif
            ;

        if (enableDebugOutput)
        {
            conf = conf.MinimumLevel.Debug();
        }

        Log.Logger = conf.CreateLogger();
    }

    public static ILogger ForContext<T>()
    {
        return Log.Logger.ForContext("SourceContext", typeof(T).Name);
    }
}
