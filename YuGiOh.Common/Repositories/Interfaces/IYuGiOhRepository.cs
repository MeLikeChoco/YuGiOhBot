using System.Collections.Generic;
using System.Threading.Tasks;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Common.Repositories.Interfaces;

public interface IYuGiOhRepository
{

    Task InsertCardAsync(CardEntity card);
    Task InsertCardHashAsync(int id, string hash);
    Task InsertBoosterPack(BoosterPackEntity boosterPack);
    Task InsertAnimeCardAsync(AnimeCardEntity card);
    Task InsertErrorAsync(Error error);

    Task<CardEntity> GetCardAsync(string name);
    Task<IEnumerable<CardEntity>> SearchCardsAsync(string input);
    Task<IEnumerable<AnimeCardEntity>> SearchAnimeCardsAsync(string input);
    Task<IEnumerable<CardEntity>> GetCardsContainsAllAsync(string input);
    Task<CardEntity> GetCardFuzzyAsync(string input);
    Task<CardEntity> GetRandomCardAsync();
    Task<CardEntity> GetRandomMonsterAsync();
    Task<string> GetCardHashAsync(int id);

    Task<IEnumerable<CardEntity>> GetCardsInArchetypeAsync(string input);
    Task<IEnumerable<CardEntity>> GetCardsInSupportAsync(string input);
    Task<IEnumerable<CardEntity>> GetCardsInAntisupportAsync(string input);

    Task<IEnumerable<string>> GetCardsAutocompleteAsync(string input);
    Task<IEnumerable<string>> GetArchetypesAutocompleteAsync(string input);
    Task<IEnumerable<string>> GetSupportsAutocompleteAsync(string input);
    Task<IEnumerable<string>> GetAntisupportsAutocompleteAsync(string input);
    Task<IEnumerable<string>> GetAnimeCardsAutocompleteAsync(string input);

    Task<string> GetNameWithPasscodeAsync(string passcode);
    Task<string> GetImageLinkAsync(string input);
    Task<Banlist> GetBanlistAsync(BanlistFormats format);

    Task<BoosterPackEntity> GetBoosterPackAsync(string input);
    Task<IEnumerable<string>> GetBoosterPacksAutocompleteAsync(string input);

}