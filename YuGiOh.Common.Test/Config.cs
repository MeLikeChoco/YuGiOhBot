using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YuGiOh.Common.Test;

public record Config
{

    [JsonPropertyName("Db Connection Strings")]
    public DbConnectionStrings? DbConnectionString { get; init; }

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

    // public NpgsqlConnection GetYuGiOhDbConnection()
    //     => new(DbConnectionString);

}

// ReSharper disable once ClassNeverInstantiated.Global
public record DbConnectionStrings
{

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? YuGiOh { get; init; }
    public string? Guilds { get; init; }
    
}