using Serilog.Core;
using Serilog.Events;

namespace YuGiOh.Bot.Logger;

public class LogLevelEnricher : ILogEventEnricher
{

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {

        var logLevelOutput = logEvent.Level switch
        {
            LogEventLevel.Debug => "Debug",
            LogEventLevel.Error => "Error",
            LogEventLevel.Fatal => "Fatal",
            LogEventLevel.Information => "Info",
            LogEventLevel.Verbose => "Verbose",
            LogEventLevel.Warning => "Warning",
            _ => "Info"
        };

        logEvent.AddOrUpdateProperty(new LogEventProperty("yLevel", new ScalarValue(logLevelOutput)));

    }

}