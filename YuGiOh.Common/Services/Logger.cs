using System;
using System.Reflection;
using System.Text;

namespace YuGiOh.Common.Services;

public static class Logger
{

    private static string _defaultCategory;

    private static string DefaultCategory => _defaultCategory ??= Assembly.GetEntryAssembly()!.GetName().Name; //GetEntryAssembly shouldnt ever return null in our usage..... hopefully

    static Logger()
    {
        Console.OutputEncoding = Encoding.UTF8;
    }

    public static void Log(LogLevel logLevel, string message)
        => Log(logLevel, DefaultCategory, message);

    public static void Log(LogLevel logLevel, string category, string message)
    {

        lock (Console.Out)
        {

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{DateTime.Now} ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[{logLevel}]");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[{category}]");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($" {message}");

        }

    }

    public static void InlineLog(LogLevel logLevel, string message)
        => InlineLog(logLevel, DefaultCategory, message);

    public static void InlineLog(LogLevel logLevel, string category, string message)
    {

        lock (Console.Out)
        {

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{DateTime.Now} ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[{logLevel}]");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[{category}]");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($" {message}\r");

        }

    }

    public static void Info(string message)
        => Log(LogLevel.Info, message);

    public static void Info(string category, string message)
        => Log(LogLevel.Info, category, message);

    public static void Error(string message)
        => Log(LogLevel.Error, message);

    public static void Error(string category, string message)
        => Log(LogLevel.Error, category, message);

}

public enum LogLevel
{

    Info,
    Error

}