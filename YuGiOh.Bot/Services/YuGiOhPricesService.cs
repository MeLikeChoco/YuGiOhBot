using System;
using System.Threading.Tasks;
using YuGiOh.Bot.Models.Cards;
using YuGiOh.Bot.Models.Deserializers;
using YuGiOh.Bot.Services.Interfaces;

namespace YuGiOh.Bot.Services;

public class YuGiOhPricesService : IYuGiOhPricesService
{

    private const string PricesBaseUrl = "https://yugiohprices.com/api/get_card_prices/";
    // private const string ImagesBaseUrl = "https://yugiohprices.com/api/card_image/";
    
    private Web _web;

    public YuGiOhPricesService(Web web)
    {
        _web = web;
    }
    
    public async Task<YuGiOhPrices> GetPrices(Card card)
    {

        var response = await _web.GetDeserializedContent<YuGiOhPrices>($"{PricesBaseUrl}{Uri.EscapeDataString(card.Name)}").ConfigureAwait(false);

        if ((response is null || response.Status == "fail") && !string.IsNullOrEmpty(card.RealName))
            response = await _web.GetDeserializedContent<YuGiOhPrices>($"{PricesBaseUrl}{Uri.EscapeDataString(card.RealName)}").ConfigureAwait(false);

        return response;

    }
    
}