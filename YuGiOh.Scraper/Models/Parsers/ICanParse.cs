using System.Threading.Tasks;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Scraper.Models.Parsers;

public interface ICanParse<T>
{

    Task<T> ParseAsync();

}