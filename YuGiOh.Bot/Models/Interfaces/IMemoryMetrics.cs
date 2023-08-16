namespace YuGiOh.Bot.Models.Interfaces
{
    public interface IMemoryMetrics
    {

        double TotalMem { get; }
        double UsedMem { get; }

    }
}