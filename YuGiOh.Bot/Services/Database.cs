using Dapper;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Models;
using Dapper.Contrib.Extensions;
using System.Collections.Concurrent;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Services
{
    public class Database
    {

        private const string DbPath = "Data Source = Databases/guilds.db";

        public ConcurrentDictionary<ulong, Setting> Settings;

        public async Task Initialize(IEnumerable<SocketGuild> guilds)
        {

            using (var db = new SqliteConnection(DbPath))
            {

                await db.OpenAsync();

                var settings = (await db.GetAllAsync<Setting>()).ToList();
                var unregGuilds = guilds.Where(guild => !settings.Any(setting => setting.Id == guild.Id)).ToList();

                foreach (var guild in unregGuilds)
                {

                    var setting = new Setting(guild);
                    await db.InsertAsync(setting);
                    settings.Add(setting);

                }

                Settings = new ConcurrentDictionary<ulong, Setting>(settings.ToDictionary(setting => setting.Id, setting => setting));

                db.Close();

            }

        }

        public async Task AddGuild(SocketGuild guild)
        {

            var setting = new Setting(guild);

            using (var db = new SqliteConnection(DbPath))
            {

                await db.OpenAsync();
                await db.InsertAsync(setting);
                db.Close();

            }

            Settings[setting.Id] = setting;

        }

        public async Task SetPrefix(ulong id, string prefix)
        {

            var setting = Settings[id];
            setting.Prefix = prefix;

            using (var db = new SqliteConnection(DbPath))
            {

                await db.OpenAsync();
                await db.UpdateAsync(setting);
                db.Close();

            }

        }

        public async Task SetMinimal(ulong id, bool minimal)
        {

            var setting = Settings[id];
            setting.Minimal = minimal;

            using (var db = new SqliteConnection(DbPath))
            {

                await db.OpenAsync();
                await db.UpdateAsync(setting);
                db.Close();

            }

        }

        public async Task SetGuessTime(ulong id, int seconds)
        {

            var setting = Settings[id];
            setting.GuessTime = seconds;

            using (var db = new SqliteConnection(DbPath))
            {

                await db.OpenAsync();
                await db.UpdateAsync(setting);
                db.Close();

            }

        }

        public async Task SetAutoDelete(ulong id, bool delete)
        {

            var setting = Settings[id];
            setting.AutoDelete = delete;

            using (var db = new SqliteConnection(DbPath))
            {

                await db.OpenAsync();
                await db.UpdateAsync(setting);
                db.Close();

            }

        }

        public async Task SetInline(ulong id, bool inline)
        {

            var setting = Settings[id];
            setting.Inline = inline;

            using (var db = new SqliteConnection(DbPath))
            {

                await db.OpenAsync();
                await db.UpdateAsync(setting);
                db.Close();

            }

        }

    }
}
