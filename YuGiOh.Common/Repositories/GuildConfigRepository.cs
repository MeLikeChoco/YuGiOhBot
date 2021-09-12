using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Common.Interfaces;

namespace YuGiOh.Common.Repositories
{
    public class GuildConfigRepository
    {

        private readonly IGuildConfigConfiguration _config;

        public GuildConfigRepository(IGuildConfigConfiguration config)
        {
            _config = config;
        }

        

    }
}
