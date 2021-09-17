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

        Task<IEnumerable<Card>> GetCardsAsync(string input);
        Task<IEnumerable<Card>> SearchCardsAsync(string input);
        Task<IEnumerable<Card>> GetCardsContainsAllAsync(string input);

        Task<Card> GetCardAsync(string name);
        Task<Card> GetRandomCardAsync();
        Task<Card> GetClosestCardAsync(string input);

        Task<IEnumerable<Card>> GetCardsInArchetype(string input);
        Task<IEnumerable<Card>> GetCardsFromSupportAsync(string input);
        Task<IEnumerable<Card>> GetCardsFromAntisupportAsync(string input);

        Task<string> GetImageLinkAsync(string input);
        Task<string> GetNameWithPasscodeAsync(string passcode);

    }
}
