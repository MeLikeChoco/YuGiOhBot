using System;

namespace YuGiOh.Scraper.Models;

[AttributeUsage(AttributeTargets.Class)]
public class ParserModuleAttribute : Attribute
{

    public string Name { get; }

    public ParserModuleAttribute(string name)
    {
        Name = name;
    }

}