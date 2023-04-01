using System;

namespace YuGiOh.Scraper.Models.Exceptions;

public class SkillCardException : Exception
{

    public string Id { get; set; }
    public string Name { get; set; }

    public SkillCardException(string id, string name, string message)
        : base(message)
    {

        Id = id;
        Name = name;

    }

}