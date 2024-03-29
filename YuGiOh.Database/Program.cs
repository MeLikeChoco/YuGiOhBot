﻿//Yes yes, this looks like sin, but I'm trying top level statements out
//don't shame me ;_;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Npgsql;
using YuGiOh.Common.Repositories;
using YuGiOh.Common.Services;
using YuGiOh.Database;

Timer _timer;

Logger.Info("Database service has started.");

var config = await Config.GetConfig();
var scraperStartDelay = await GetScraperInterval(config);

_timer = new Timer(
    DumpDatabaseAndScrape,
    null,
    scraperStartDelay,
    config.ScraperInterval
);

await Task.Delay(-1);

async void DumpDatabaseAndScrape(object _)
{

    var config = await Config.GetConfig();

    await BackupYuGiOhDatabase(config);
    await BackupGuildConfigsDatabase(config);
    //await RecreateDatabase(config);
    //await StartScraper(config);
    //await CreateIndexes(config);

    var runDate = DateTime.Now;

    await File.WriteAllTextAsync("time.txt", runDate.ToString());
    Logger.Info($"Finished scraping at {runDate}");

}

async Task BackupYuGiOhDatabase(Config config)
{

    Logger.Info("Backing up the yugioh database.");

    var guid = Guid.NewGuid();

    using (var connection = GetNpgsqlConnection(config, "backups"))
    {

        await connection.OpenAsync();

        using (var recordBackupCmd = connection.CreateCommand())
        {

            recordBackupCmd.CommandText = "insert into yugioh(guid) values(@guid)";

            recordBackupCmd.Parameters.AddWithValue("guid", guid);
            await recordBackupCmd.ExecuteNonQueryAsync();

        }

        await connection.CloseAsync();

    }

    var backupFilePath = Path.Combine(config.Directories.YuGiOhBackups, guid.ToString());

    using (var process = new Process())
    {

        process.StartInfo = new ProcessStartInfo
        {

            FileName = "pg_dump",
            Arguments = string.Format(config.ProcessArgs.PgDump, config.Database.Username, "yugioh"),
            RedirectStandardOutput = true

        };

        process.Start();

        using var reader = process.StandardOutput;
        using var writer = new StreamWriter(backupFilePath);

        await reader.BaseStream.CopyToAsync(writer.BaseStream);
        writer.Close();
        await process.WaitForExitAsync();

    }

    Logger.Info($"Wrote to {backupFilePath}");
    Logger.Info("Finished backing up the yugioh database.");

}

async Task BackupGuildConfigsDatabase(Config config)
{

    Logger.Info("Backing up the guild configs database.");

    var guid = Guid.NewGuid();

    using (var connection = GetNpgsqlConnection(config, "backups"))
    {

        await connection.OpenAsync();

        using (var recordBackupCmd = connection.CreateCommand())
        {

            recordBackupCmd.CommandText = "insert into guilds(guid) values(@guid)";

            recordBackupCmd.Parameters.AddWithValue("guid", guid);
            await recordBackupCmd.ExecuteNonQueryAsync();

        }

        await connection.CloseAsync();

    }

    var backupFilePath = Path.Combine(config.Directories.GuildConfigsBackups, guid.ToString());

    using (var process = new Process())
    {

        process.StartInfo = new ProcessStartInfo
        {

            FileName = "pg_dump",
            Arguments = string.Format(config.ProcessArgs.PgDump, config.Database.Username, "guilds"),
            RedirectStandardOutput = true

        };

        process.Start();

        using var reader = process.StandardOutput;
        using var writer = new StreamWriter(backupFilePath);

        await reader.BaseStream.CopyToAsync(writer.BaseStream);
        writer.Close();
        await process.WaitForExitAsync();

    }

    Logger.Info($"Wrote to {backupFilePath}");
    Logger.Info("Finished backing up the guild configs database.");

}

//async Task RecreateDatabase(Config config)
//{

//    Logger.Info("Dropping tables.");

//    var sql = await File.ReadAllTextAsync(Path.Combine(config.Directories.SqlDirectory, "Table Deletion.sql"));

//    using var connection = GetNpgsqlConnection(config, "yugioh");

//    await connection.OpenAsync();
//    await connection.ExecuteAsync(sql);

//    Logger.Info("Wiped tables.");
//    Logger.Info("Creating tables.");

//    sql = await File.ReadAllTextAsync(Path.Combine(config.Directories.SqlDirectory, "Table Creation.sql"));

//    await connection.ExecuteAsync(sql);

//    Logger.Info("Created tables.");

//}

//async Task StartScraper(Config config)
//{

//    if (config.ShouldRunScraper)
//    {

//        Logger.Info("Starting scraper.");

//        if (!File.Exists(config.ScraperDllPath))
//        {
//            Logger.Error("Scraper is currently not available.");
//            return;
//        }

//        var processInfo = new ProcessStartInfo
//        {

//            FileName = "dotnet",
//            Arguments = string.Format(config.ProcessArgs.Scraper, config.ScraperDllPath),
//            RedirectStandardOutput = true,
//            RedirectStandardError = true

//        };

//        var process = new Process { StartInfo = processInfo };

//        process.OutputDataReceived += (_, eventArgs) => Logger.Info(eventArgs.Data);
//        process.ErrorDataReceived += (_, eventArgs) => Logger.Error(eventArgs.Data);

//        process.Start();
//        process.BeginOutputReadLine();
//        process.BeginErrorReadLine();
//        await process.WaitForExitAsync();

//        Logger.Info("Scraper finished.");

//    }

//}

//async Task CreateIndexes(Config config)
//{

//    Logger.Info("Creating indexes.");

//    var sql = await File.ReadAllTextAsync(Path.Combine(config.Directories.SqlDirectory, "Index Creation.sql"));

//    using (var connection = GetNpgsqlConnection(config, "yugioh"))
//    {

//        await connection.OpenAsync();
//        await connection.ExecuteAsync(sql);
//        await connection.CloseAsync();

//    }

//    Logger.Info("Created indexes.");

//}

NpgsqlConnection GetNpgsqlConnection(Config config, string database = null)
{

    var connectionString = new NpgsqlConnectionStringBuilder
    {

        Host = config.Database.Host,
        Port = config.Database.Port,
        Username = config.Database.Username,
        Password = config.Database.Password

    };

    if (!string.IsNullOrEmpty(database))
        connectionString.Database = database;

    return new NpgsqlConnection(connectionString.ToString());

}

async Task<TimeSpan> GetScraperInterval(Config config)
{

    if (!config.ShouldInstantStart)
    {

        var lastScrapeTimeTxt = await File.ReadAllTextAsync("time.txt");

        if (!DateTime.TryParse(lastScrapeTimeTxt, out var lastScrapeTime))
            lastScrapeTime = DateTime.Now;

        var difference = Math.Abs((DateTime.Now - lastScrapeTime).TotalDays);

        return difference < config.ScraperInterval.TotalDays ? TimeSpan.FromDays(difference) : config.ScraperInterval;

    }

    return TimeSpan.FromSeconds(10);

}

namespace YuGiOh.Database
{

    public class Config
    {

        public Database Database { get; set; }
        public Directories Directories { get; set; }
        public ProcessArgs ProcessArgs { get; set; }
        public string ScraperDllPath { get; set; }
        public TimeSpan ScraperInterval { get; set; }
        public bool ShouldRunScraper { get; set; }
        public bool ShouldInstantStart { get; set; }

        public static async Task<Config> GetConfig()
        {

            var configTxt = await File.ReadAllTextAsync("config.json");

            return JsonConvert.DeserializeObject<Config>(configTxt);

        }

    }

    public class Database
    {

        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

    }

    public class Directories
    {

        public string Sql { get; set; }
        public string YuGiOhBackups { get; set; }
        public string GuildConfigsBackups { get; set; }

    }

    public class ProcessArgs
    {

        public string PgDump { get; set; }
        public string Scraper { get; set; }

    }

}
