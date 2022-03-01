using Serilog.Core;
using Serilog.Events;

namespace YuGiOh.Bot.Logger;

public class ColorEnricher : ILogEventEnricher
{

    #region Colors
    
    //ANSI Color Formatting
    //For foreground 24bit color: \x1b[38;5;{value}m
    //For foreground 24bit color with rgb: \x1b[38;2;{r};{g};{b}m

    private const string Reset = "\x1b[0m";
    private const string Test = "\x1b[38;5;255m";
    private const string Yellow = "\x1b[38;5;220m";
    private const string Red = "\x1b[38;5;9m";
    private const string Green = "\x1b[38;5;82m";
    private const string White = "\x1b[37m";
    private const string LightGray = "\x1b[38;5;246m";
    private const string Turqoise = "\x1b[38;5;31m";
    private const string Color1 = "\x1b[38;2;115;18;54m";
    private const string Color2 = "\x1b[38;2;122;2;141m";
    private const string Color3 = "\x1b[38;2;65;63;151m";
    private const string Color4 = "\x1b[38;2;241;242;240m";

    #endregion Colors

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {

        logEvent.AddOrUpdateProperty(new LogEventProperty("Reset", new ScalarValue(Reset)));
        logEvent.AddOrUpdateProperty(new LogEventProperty("Test", new ScalarValue(Test)));
        logEvent.AddOrUpdateProperty(new LogEventProperty("Color1", new ScalarValue(Color1)));
        logEvent.AddOrUpdateProperty(new LogEventProperty("Color2", new ScalarValue(Color2)));
        logEvent.AddOrUpdateProperty(new LogEventProperty("Color3", new ScalarValue(Color3)));
        logEvent.AddOrUpdateProperty(new LogEventProperty("Color4", new ScalarValue(Color4)));

    }

}