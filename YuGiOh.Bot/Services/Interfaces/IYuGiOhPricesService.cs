using System.Threading.Tasks;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Models.Deserializers;

namespace YuGiOh.Bot.Services.Interfaces;

public interface IYuGiOhPricesService
{
    Task<YuGiOhPrices> GetPrices(Card card);
}