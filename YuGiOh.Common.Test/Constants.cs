using System;

namespace YuGiOh.Common.Test;

public static class Constants
{

    public const string TestDataDirectory = "Test Data";
    public const string DefaultEnv = "Docker";

    public static string YuGiOhEnv => Environment.GetEnvironmentVariable("YUGIOH_ENV") ?? DefaultEnv;

}