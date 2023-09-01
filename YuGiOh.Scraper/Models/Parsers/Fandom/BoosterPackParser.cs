using System;
using System.Threading.Tasks;
using YuGiOh.Common.Models.YuGiOh;
using YuGiOh.Scraper.Constants;
using YuGiOh.Scraper.Models.ParserOptions;

namespace YuGiOh.Scraper.Models.Parsers.Fandom;

[ParserModule(ConstantString.FandomModuleName)]
public class BoosterPackParser : ICanParse<BoosterPackEntity>
{

    private readonly string _id, _name;
    private readonly Options _options;

    public BoosterPackParser(string name, string id, Options options)
    {
        _id = id;
        _name = name;
        _options = options;
    }

    public async Task<BoosterPackEntity> ParseAsync()
    {
        throw new NotImplementedException();
    }

}