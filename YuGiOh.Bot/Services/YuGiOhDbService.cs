using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Services.Interfaces;
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

        public Task<IEnumerable<string>> SearchCardsAsync(string input)
            => _repo.SearchCardsAsync(input);

        public async Task<Card> GetRandomCardAsync()
        {

            var entity = await _repo.GetRandomCardAsync();

            return entity.ToModel();

        }

        public Task<IEnumerable<string>> GetCardsAsync(string input)
            => _repo.SearchCardsAsync(input);

        public Task<IEnumerable<string>> GetCardsInArchetype(string input)
            => _repo.GetCardsInArchetypeAsync(input);

        public Task<IEnumerable<string>> GetCardsFromSupportAsync(string input)
            => _repo.GetCardsInSupportAsync(input);

        public Task<IEnumerable<string>> GetCardsFromAntisupportAsync(string input)
            => _repo.GetCardsInAntisupportAsync(input);

        public async Task<Card> GetClosestCardAsync(string input)
        {

            var card = await _repo.GetCardFuzzyAsync(input);

            throw new NotImplementedException();

        }

        public Task<string> GetImageLinkAsync(string input)
            => _repo.GetImageLinkAsync(input);

        public Task<string> GetNameWithPasscodeAsync(string passcode)
            => _repo.GetNameWithPasscodeAsync(passcode);

    }
}
