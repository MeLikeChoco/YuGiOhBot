using System.Collections.Concurrent;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Scraper.Models.Responses;

public class ProcessorResponse
{
    
    public int Count { get; set; }
    public ConcurrentBag<Error> Errors { get; set; }
    
}