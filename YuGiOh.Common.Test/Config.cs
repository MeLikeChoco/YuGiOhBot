using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YuGiOh.Common.Test;

public record Config
{

    [JsonPropertyName("Db Connection Strings")]
    public DbConnectionStrings? DbConnectionStrings { get; init; }

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
public class DbConnectionStrings
{
    
    public DbConnectionString? Dev { get; init; }
    public DbConnectionString? Docker { get; init; }

    /// <summary>
    /// Gets connection string based on environment variables
    /// </summary>
    public DbConnectionString? GetConnectionString()
    {
        return Environment.GetEnvironmentVariable("YUGIOH_ENV") switch
        {
            "Dev" => Dev,
            "Docker" => Docker,
            _ => Docker
        };
    }
    
}

public record DbConnectionString
{

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? YuGiOh { get; init; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? Guilds { get; init; }

}