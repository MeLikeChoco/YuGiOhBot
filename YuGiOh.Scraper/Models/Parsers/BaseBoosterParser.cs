using System.Collections.Generic;
using System.Threading.Tasks;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Scraper.Models.Parsers;

public abstract class BaseBoosterParser : ICanParse<BoosterPackEntity>
{

    protected virtual Task BeforeParseAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task AfterParseAsync()
    {
        return Task.CompletedTask;
    }

    public virtual async Task<BoosterPackEntity> ParseAsync()
    {

        await BeforeParseAsync();

        var booster = new BoosterPackEntity
        {

            Id = await GetId(),
            Name = await GetName(),
            Dates = await GetDates(),
            Cards = await GetCards(),
            Url = await GetUrl(),
            TcgExists = await GetTcgExists(),
            OcgExists = await GetOcgExists()

        };

        await AfterParseAsync();

        return booster;

    }

    protected abstract Task<int> GetId();
    protected abstract Task<string> GetName();
    protected abstract Task<List<BoosterPackDateEntity>> GetDates();
    protected abstract Task<List<BoosterPackCardEntity>> GetCards();
    protected abstract Task<string> GetUrl();
    protected abstract Task<bool> GetOcgExists();
    protected abstract Task<bool> GetTcgExists();

}