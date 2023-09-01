using System;

namespace YuGiOh.Scraper.Models.Exceptions;

public class RushDuelException : Exception
{

    public string Id { get; set; }
    public string Name { get; set; }

    public RushDuelException(string id, string name, string message)
        : base(message)
    {

        Id = id;
        Name = name;

    }

}