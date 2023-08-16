using Fergun.Interactive;
using Fergun.Interactive.Pagination;

namespace YuGiOh.Bot.Models;

public class PaginatorFactory
{

    private readonly InteractiveConfig _interactiveConfig;

    public PaginatorFactory(InteractiveConfig interactiveConfig)
    {
        _interactiveConfig = interactiveConfig;
    }

    public StaticPaginatorBuilder CreateStaticPaginatorBuilder(GuildConfig guildConfig)
    {

        var actionOnTimeout = ActionOnStop.DisableInput;

        if (guildConfig.AutoDelete)
            actionOnTimeout |= ActionOnStop.DeleteMessage;

        return new StaticPaginatorBuilder()
            .WithFooter(PaginatorFooter.PageNumber)
            .WithActionOnTimeout(actionOnTimeout)
            .WithActionOnCancellation(ActionOnStop.DeleteMessage);

    }

    public LazyPaginatorBuilder CreateLazyPaginatorBuilder(GuildConfig guildConfig)
    {

        var actionOnTimeout = ActionOnStop.DisableInput;

        if (guildConfig.AutoDelete)
            actionOnTimeout |= ActionOnStop.DeleteMessage;

        return new LazyPaginatorBuilder()
            .WithFooter(PaginatorFooter.PageNumber)
            .WithActionOnTimeout(actionOnTimeout)
            .WithActionOnCancellation(ActionOnStop.DeleteMessage);

    }

}