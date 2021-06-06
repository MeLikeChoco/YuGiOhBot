using AngleSharp.Html.Dom;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;

namespace YuGiOhScraper.Parsers
{
    public abstract class MediaWikiParser<T> : IParser<T>
    {

        protected string Name { get; set; }
        protected string Url { get; set; }

        protected MediaWikiParser(string name, string url)
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

        protected string GetHtml(HttpClient httpClient, string url)
        {

            var json = httpClient.GetStringAsync(url).Result;
            var jObject = JObject.Parse(json);

            try
            {
                return jObject["parse"].Value<string>("text");
            }
            catch (Exception)
            {
                if (jObject.ContainsKey("parse"))
                    return jObject["parse"]["text"].Value<string>("*");
                else
                    throw new PageDoesNotExistException();
            }

        }

        protected string GetHtml(HttpClient httpClient)
            => GetHtml(httpClient, Url);

        protected IHtmlDocument GetDom(string html)
            => ScraperConstants.HtmlParser.ParseDocument(html);

        protected IHtmlDocument GetDom(HttpClient httpClient, string url)
            => ScraperConstants.HtmlParser.ParseDocument(GetHtml(httpClient, url));

        protected IHtmlDocument GetDom(HttpClient httpClient)
            => GetDom(httpClient, Url);

        public abstract T Parse(HttpClient client);

    }
}
