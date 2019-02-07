using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper.Parsers
{
    public interface IParser<T>
    {

        Task<T> ParseAsync();

    }
}
