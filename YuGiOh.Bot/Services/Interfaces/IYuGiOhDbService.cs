using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Bot.Services.Interfaces
{
    public interface IYuGiOhDbService
    {

        Task<IEnumerable<string>> GetCardsAsync(string input);
        Task<Card> GetCardAsync(string name);
        Task<Card> GetRandomCardAsync();
        Task<Card> GetClosestCardAsync(string input);
        Task<IEnumerable<string>> GetCardsFromArchetypeAsync(string input);
        Task<IEnumerable<string>> GetCardsFromSupportAsync(string input);
        Task<IEnumerable<string>> GetCardsFromAntisupportAsync(string input);

    }
}
