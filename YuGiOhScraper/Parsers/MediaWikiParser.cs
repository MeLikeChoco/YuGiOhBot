using AngleSharp.Html.Dom;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper.Parsers
{
    public abstract class MediaWikiParser<T> : IParser<T>
    {

        protected string Name { get; set; }
        protected string Url { get; set; }

        public MediaWikiParser(string name, string url)
        {

            Name = name;
            Url = url;

        }

        protected string TrimName(string name)
        {

            if (name.StartsWith('"') && name.EndsWith('"'))
            {

                if (name[name.Length - 1] == '"' && name[name.Length - 2] == '"')
                    name = name.TrimStart('"').Substring(0, name.Length - 2);
                else
                    name = name.Trim('"');

            }

            return name.Trim();

        }

        protected string GetHtml(HttpClient httpClient)
        {

            var json = httpClient.GetStringAsync(Url).Result;
            var jObject = JObject.Parse(json);

            try
            {
                return jObject["parse"].Value<string>("text");
            }
            catch (Exception)
            {
                return jObject["parse"]["text"].Value<string>("*");
            }

        }

        protected IHtmlDocument GetDom(string html)
            => ScraperConstants.HtmlParser.ParseDocument(html);

        protected IHtmlDocument GetDom(HttpClient httpClient)
            => ScraperConstants.HtmlParser.ParseDocument(GetHtml(httpClient));

        public abstract T Parse(HttpClient client);

    }
}
