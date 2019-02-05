using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhScraper.Entities;

namespace YuGiOhScraper.Parsers
{
    public abstract class CardParserBase
    {

        protected string Name { get; set; }
        protected IElement Dom { get; set; }

        public CardParserBase(string name, string url, string elementId)
        {

            Name = name;
            Dom = GetDom(url);

        }

        public abstract Card Parse();

        protected abstract IElement GetDom(string url);

    }
}
