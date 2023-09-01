using System.Threading.Tasks;

namespace YuGiOh.Scraper.Models.Parsers;

public interface ICanParse<T>
{

    Task<T> ParseAsync();

}