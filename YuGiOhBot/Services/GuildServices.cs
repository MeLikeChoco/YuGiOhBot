using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;

namespace YuGiOhBot.Services
{
    public class GuildServices
    {

        private const string DatabasePath = "Data Source=Databases/guildinformation.db";
        private const string PrefixTable = "prefixes";
        public ConcurrentDictionary<ulong, string> _guildPrefixes { get; private set; }

        public async Task InitializeService()
        {

            _guildPrefixes = new ConcurrentDictionary<ulong, string>();

            using (SqliteConnection prefixesDatabase = new SqliteConnection(DatabasePath))
            {

                await prefixesDatabase.OpenAsync();

                using (SqliteCommand getPrefixes = prefixesDatabase.CreateCommand())
                {

                    getPrefixes.CommandText = $"select * from {PrefixTable}";

                    using (SqliteDataReader dataReader = await getPrefixes.ExecuteReaderAsync())
                    {

                        while (await dataReader.ReadAsync())
                        {

                            _guildPrefixes.TryAdd(ulong.Parse(dataReader["id"].ToString()), dataReader["prefix"].ToString());

                        }

                    }

                }

                prefixesDatabase.Close();

            }

        }

        public async Task SetPrefix(ulong guildId, string prefix)
        {

            var guildIdString = guildId.ToString();

            using (SqliteConnection prefixesDatabase = new SqliteConnection(DatabasePath))
            {

                bool doesPrefixExist;

                await prefixesDatabase.OpenAsync();

                using (SqliteCommand checkExist = prefixesDatabase.CreateCommand())
                {

                    checkExist.CommandText = $"select * from {PrefixTable} where id like @ID";
                    checkExist.Parameters.Add("@ID", SqliteType.Text);
                    checkExist.Parameters["@ID"].Value = guildIdString;

                    doesPrefixExist = (await checkExist.ExecuteScalarAsync()) == null ? false : true;

                }

                if (doesPrefixExist)
                {

                    using (SqliteCommand updatePrefix = prefixesDatabase.CreateCommand())
                    {

                        updatePrefix.CommandText = $"update {PrefixTable} set prefix = @PREFIX where id = @ID";
                        updatePrefix.Parameters.Add("@PREFIX", SqliteType.Text);
                        updatePrefix.Parameters.Add("@ID", SqliteType.Text);
                        updatePrefix.Parameters["@PREFIX"].Value = prefix;
                        updatePrefix.Parameters["@ID"].Value = guildIdString;

                        await updatePrefix.ExecuteNonQueryAsync();

                    }

                }
                else
                {

                    using (SqliteCommand insertPrefix = prefixesDatabase.CreateCommand())
                    {

                        insertPrefix.CommandText = $"insert into {PrefixTable} (id, prefix) values (@ID, @PREFIX)";
                        insertPrefix.Parameters.Add("@ID", SqliteType.Text);
                        insertPrefix.Parameters.Add("@PREFIX", SqliteType.Text);
                        insertPrefix.Parameters["@ID"].Value = guildIdString;
                        insertPrefix.Parameters["@PREFIX"].Value = prefix;

                        await insertPrefix.ExecuteNonQueryAsync();

                    }

                }

                prefixesDatabase.Close();

            }

            _guildPrefixes.AddOrUpdate(guildId, prefix, (id, oldprefix) => oldprefix = prefix);

        }

    }
}
