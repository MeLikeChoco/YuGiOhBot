using Dapper;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects;
using Dapper.Contrib.Extensions;
using System.Collections.Concurrent;

namespace YuGiOhV2.Services
{
    public class Database
    {

        private const string DbPath = "Data Source = Databases/guilds.db";

        public ConcurrentDictionary<ulong, Settings> Settings;

        public Database(IEnumerable<SocketGuild> guilds)
        {

            using (var db = new SqliteConnection(DbPath))
            {

                db.Open();

                var settings = db.GetAll<Settings>().ToList();
                var unregGuilds = guilds.Where(guild => !settings.Any(setting => guild.Id == setting.Id));

                foreach(var guild in unregGuilds)
                {

                    var setting = new Settings(guild);
                    db.Insert(setting);
                    settings.Add(setting);

                }

                Settings = new ConcurrentDictionary<ulong, Settings>(settings.ToDictionary(setting => setting.Id, setting => setting));

                db.Close();

            }

        }

        public async Task AddGuild(SocketGuild guild)
        {

            var setting = new Settings(guild);

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

    }
}
