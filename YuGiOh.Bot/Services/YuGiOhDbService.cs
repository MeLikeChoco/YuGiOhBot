using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public Task<Card> GetCardAsync(string name)
        {

            throw new NotImplementedException();

        }

        public Task<IEnumerable<string>> GetCardsAsync(string input)
            => _repo.GetCardsAsync(input);

        public Task<IEnumerable<string>> GetCardsFromAntisupportAsync(string input)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetCardsFromArchetypeAsync(string input)
            => _repo.GetCardsFromArchetypeAsync(input);

        public Task<IEnumerable<string>> GetCardsFromSupportAsync(string input)
        {
            throw new NotImplementedException();
        }

        public Task<Card> GetClosestCardAsync(string input)
            => _repo.GetCardFuzzyAsync(input);

        public Task<Card> GetRandomCardAsync()
        {
            throw new NotImplementedException();
        }

    }
}
