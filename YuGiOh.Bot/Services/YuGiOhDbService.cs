using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.BoosterPacks;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Services.Interfaces;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Common.Repositories.Interfaces;

namespace YuGiOh.Bot.Services
{
    public class YuGiOhDbService : IYuGiOhDbService
    {

        private readonly IYuGiOhRepository _repo;

        public YuGiOhDbService(IYuGiOhRepository repo)
            => _repo = repo;

        public async Task<Card> GetCardAsync(string name)
        {

            var card = await _repo.GetCardAsync(name);

            return card?.ToModel();

        }

        public async Task<IEnumerable<Card>> SearchCardsAsync(string input)
        {

            var entities = await _repo.SearchCardsAsync(input);

            return entities.Select(entity => entity.ToModel());

        }

        public async Task<IEnumerable<Card>> GetCardsContainsAllAsync(string input)
        {

            var entities = await _repo.GetCardsContainsAllAsync(input);

            return entities.Select(entity => entity.ToModel());

        }

        public async Task<Card> GetRandomCardAsync()
        {

            var entity = await _repo.GetRandomCardAsync();

            return entity.ToModel();

        }

        public async Task<IEnumerable<Card>> GetCardsAsync(string input)
        {

            var entities = await _repo.SearchCardsAsync(input);

            return entities.Select(entity => entity.ToModel());

        }

        public async Task<IEnumerable<Card>> GetCardsInArchetypeAsync(string input)
        {

            var entities = await _repo.GetCardsInArchetypeAsync(input);

            return entities.Select(entity => entity.ToModel());

        }

        public async Task<IEnumerable<Card>> GetCardsFromSupportAsync(string input)
        {

            var entities = await _repo.GetCardsInSupportAsync(input);

            return entities.Select(entity => entity.ToModel());

        }

        public async Task<IEnumerable<Card>> GetCardsFromAntisupportAsync(string input)
        {

            var entities = await _repo.GetCardsInAntisupportAsync(input);

            return entities.Select(entity => entity.ToModel());

        }

        public Task<IEnumerable<string>> GetCardsAutocompleteAsync(string input)
            => _repo.GetCardsAutocompleteAsync(input);

        public Task<IEnumerable<string>> GetArchetypesAutocompleteAsync(string input)
            => _repo.GetArchetypesAutocompleteAsync(input);

        public Task<IEnumerable<string>> GetSupportsAutocompleteAsync(string input)
            => _repo.GetSupportsAutocompleteAsync(input);

        public Task<IEnumerable<string>> GetAntisupportsAutocompleteAsync(string input)
            => _repo.GetAntisupportsAutocompleteAsync(input);

        public async Task<Card> GetClosestCardAsync(string input)
        {

            var entity = await _repo.GetCardFuzzyAsync(input);

            return entity.ToModel();

        }

        public Task<string> GetImageLinkAsync(string input)
            => _repo.GetImageLinkAsync(input);

        public Task<string> GetNameWithPasscodeAsync(string passcode)
            => _repo.GetNameWithPasscodeAsync(passcode);

        public Task<Banlist> GetBanlistAsync(BanlistFormats format)
            => _repo.GetBanlistAsync(format);

        public async Task<BoosterPack> GetBoosterPackAsync(string input)
        {

            var entity = await _repo.GetBoosterPackAsync(input);

            return entity.ToModel();

        }

    }
}