using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Common.Repositories.Interfaces
{
    public interface IYuGiOhRepository
    {

        Task InsertCardAsync(Card card);

        Task InsertBoosterPack(BoosterPack boosterPack);

        Task InsertErrorAsync(Error error);

        Task<IEnumerable<string>> GetCardsAsync(string input);

        Task<Card> GetCardFuzzyAsync(string input);

        Task<IEnumerable<string>> GetCardsFromArchetypeAsync(string input);

    }
}
