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
        Task<IEnumerable<string>> SearchCardsAsync(string input);
        Task<CardEntity> GetCardFuzzyAsync(string input);
        Task<CardEntity> GetRandomCardAsync();

        Task<IEnumerable<string>> GetCardsInArchetypeAsync(string input);
        Task<IEnumerable<string>> GetCardsInSupportAsync(string input);
        Task<IEnumerable<string>> GetCardsInAntisupportAsync(string input);

        Task<string> GetNameWithPasscodeAsync(string passcode);
        Task<string> GetImageLinkAsync(string input);
        Task<IEnumerable<string>> GetBanlistCards(CardEntityFormats format);

    }
}
