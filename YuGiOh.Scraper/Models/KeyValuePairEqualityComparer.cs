using System;
using System.Collections.Generic;

namespace YuGiOh.Scraper.Models;

public class KeyValuePairEqualityComparer : IEqualityComparer<KeyValuePair<string, string>>
{
    
    public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
    {
        return x.Key == y.Key && x.Value == y.Value;
    }

    public int GetHashCode(KeyValuePair<string, string> obj)
    {
        return HashCode.Combine(obj.Key, obj.Value);
    }
    
}