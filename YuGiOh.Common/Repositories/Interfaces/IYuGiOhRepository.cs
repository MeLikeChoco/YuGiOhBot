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

        Task InsertCardAsync(CardEntity card);
        Task InsertBoosterPack(BoosterPack boosterPack);
        Task InsertErrorAsync(Error error);

        Task<CardEntity> GetCardAsync(string name);
        Task<IEnumerable<CardEntity>> SearchCardsAsync(string input);
        Task<IEnumerable<CardEntity>> GetCardsContainsAllAsync(string input);
        Task<CardEntity> GetCardFuzzyAsync(string input);
        Task<CardEntity> GetRandomCardAsync();

        Task<IEnumerable<CardEntity>> GetCardsInArchetypeAsync(string input);
        Task<IEnumerable<CardEntity>> GetCardsInSupportAsync(string input);
        Task<IEnumerable<CardEntity>> GetCardsInAntisupportAsync(string input);

        Task<string> GetNameWithPasscodeAsync(string passcode);
        Task<string> GetImageLinkAsync(string input);
        Task<Banlist> GetBanlistAsync(BanlistFormats format);

    }
}
