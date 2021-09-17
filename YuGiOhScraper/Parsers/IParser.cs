using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper.Parsers
{
    public interface IParser<T>
    {

        T Parse(HttpClient httpClient);

    }
}
