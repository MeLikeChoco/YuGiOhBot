using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects;

namespace YuGiOhV2.Services
{
    public class Database
    {

        private const string DbPath = "Data Source = guilds.db";

        public Dictionary<ulong, Settings> Settings;

        public Database()
        {

            using (var db = new SqliteConnection(DbPath))
            {

                db.Open();



            }

        }

    }
}
