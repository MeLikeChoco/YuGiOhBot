using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper.Extensions
{
    public static class AnglesharpExtensions
    {

        public static IElement GetElementByClassName(this IElement element, string className)
            => element.GetElementsByClassName(className).FirstOrDefault();

        public static IElement GetElementByClassName(this IDocument document, string className)
            => document.GetElementsByClassName(className).FirstOrDefault();

    }
}
