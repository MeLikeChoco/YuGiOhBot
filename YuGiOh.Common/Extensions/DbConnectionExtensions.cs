using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace YuGiOh.Common.Extensions
{
    public static class DbConnectionExtensions
    {

        public static Task<IEnumerable<T>> QueryProcAsync<T>(this DbConnection connection, string proc, object @params = null)
            => connection.QueryAsync<T>(proc, @params, commandType: CommandType.StoredProcedure);

        public static Task<T> QuerySingleProcAsync<T>(this DbConnection connection, string proc, object @params = null)
            => connection.QuerySingleAsync<T>(proc, @params, commandType: CommandType.StoredProcedure);

    }
}
