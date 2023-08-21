using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YuGiOh.Common.Test;

public record Config
{

    [JsonInclude]
    [JsonPropertyName("Db Connection Strings")]
    public Dictionary<string, DbConnectionStrings> DbConnectionStrings { get; private set; } = null!;

    private static Config? _instance;

    public static Config Instance
    {
        get
        {

            if (_instance is not null)
                return _instance;

            _instance = JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json")) ?? throw new NullReferenceException(); //most likely never null

            return _instance;

        }
    }

    public DbConnectionStrings GetDbConnectionStrings()
        => DbConnectionStrings[Constants.YuGiOhEnv];

    // public NpgsqlConnection GetYuGiOhDbConnection()
    //     => new(DbConnectionString);

}

public record DbConnectionStrings
{

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? YuGiOh { get; init; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? Guilds { get; init; }

}