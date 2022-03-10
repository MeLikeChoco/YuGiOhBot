using System.Collections.Generic;
using System.Threading.Tasks;
using YuGiOh.Bot.Models.BoosterPacks;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Common.Models.YuGiOh;

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

        Task<IEnumerable<Card>> GetCardsInArchetypeAsync(string input);
        Task<IEnumerable<Card>> GetCardsInSupportAsync(string input);
        Task<IEnumerable<Card>> GetCardsInAntisupportAsync(string input);
        
        Task<IEnumerable<string>> GetCardsAutocompleteAsync(string input);
        Task<IEnumerable<string>> GetArchetypesAutocompleteAsync(string input);
        Task<IEnumerable<string>> GetSupportsAutocompleteAsync(string input);
        Task<IEnumerable<string>> GetAntisupportsAutocompleteAsync(string input);

        Task<string> GetImageLinkAsync(string input);
        Task<string> GetNameWithPasscodeAsync(string passcode);

        Task<Banlist> GetBanlistAsync(BanlistFormats format);

        Task<BoosterPack> GetBoosterPackAsync(string input);
        Task<IEnumerable<string>> GetBoosterPacksAutocompleteAsync(string input);

    }
}
