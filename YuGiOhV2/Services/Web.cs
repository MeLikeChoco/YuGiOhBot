using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects.Deserializers;

namespace YuGiOhV2.Services
{
    public class Web
    {

        private HttpClient _http;

        private const string PricesBaseUrl = "http://yugiohprices.com/api/get_card_prices/";
        private const string ImagesBaseUrl = "http://yugiohprices.com/api/card_image/";

        public Web()
        {

            _http = new HttpClient();

        }

        public async Task<YuGiOhPrices> GetPrices(string name, string realName)
        {

            var response = await GetDeserializedContent<YuGiOhPrices>($"{PricesBaseUrl}{Uri.EscapeUriString(name)}");

            if ((response == null || response.Status == "fail") && !string.IsNullOrEmpty(realName))
                response = await GetDeserializedContent<YuGiOhPrices>($"{PricesBaseUrl}{Uri.EscapeUriString(realName)}");

            return response;

        }

        private async Task<T> GetDeserializedContent<T>(string url)
        {

            var json = await _http.GetStringAsync(url);

            return JsonConvert.DeserializeObject<T>(json);

        }

    }
}
