﻿using System.Linq;
using AngleSharp.Dom;

namespace YuGiOh.Scraper.Extensions;

public static class HtmlDocumentExtensions
{

    public static IElement GetElementByClassName(this IDocument dom, string className)
        => dom.GetElementsByClassName(className).FirstOrDefault();

    public static IElement GetElementByClassName(this IElement element, string className)
        => element.GetElementsByClassName(className).FirstOrDefault();

    public static IElement GetElementByTagName(this IElement element, string tagName)
        => element.GetElementsByTagName(tagName).FirstOrDefault();

}