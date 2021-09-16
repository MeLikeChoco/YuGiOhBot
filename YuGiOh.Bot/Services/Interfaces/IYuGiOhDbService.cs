using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Models.Cards;

namespace YuGiOh.Bot.Services.Interfaces
{
    public interface IYuGiOhDbService
    {

        Task<IEnumerable<string>> GetCardsAsync(string input);
        Task<IEnumerable<string>> SearchCardsAsync(string input);

        Task<Card> GetCardAsync(string name);
        Task<Card> GetRandomCardAsync();
        Task<Card> GetClosestCardAsync(string input);

        Task<IEnumerable<string>> GetCardsInArchetype(string input);
        Task<IEnumerable<string>> GetCardsFromSupportAsync(string input);
        Task<IEnumerable<string>> GetCardsFromAntisupportAsync(string input);

        Task<string> GetImageLinkAsync(string input);
        Task<string> GetNameWithPasscodeAsync(string passcode);

    }
}
