using YuGiOh.Bot.Models.Interfaces;

namespace YuGiOh.Bot.Models;

public record struct MemoryMetrics : IMemoryMetrics
{
    
    public double TotalMem { get; init; }
    public double UsedMem { get; init; }
    
}