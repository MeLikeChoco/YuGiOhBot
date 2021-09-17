using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YuGiOh.Scraper
{
    public class Config
    {

        public Database Database { get; set; }
        public TimeSpan RetryDelay { get; set; }
        public int MaxRetry { get; set; }

    }

    public class Database
    {

        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

    }
}
