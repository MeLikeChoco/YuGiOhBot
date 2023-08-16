using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOh.Bot.Models.Criteria;

public class Criteria : ICriteria
{

    private readonly List<ICriteria> _criteria = new();

    public Criteria AddCriteria(ICriteria criteria)
    {

        _criteria.Add(criteria);

        return this;

    }

    public async Task<bool> ValidateAsync(ICommandContext context, SocketMessage message)
    {

        foreach (var criteria in _criteria)
        {

            if (!await criteria.ValidateAsync(context, message))
                return false;

        }

        return true;

    }

    public async Task<bool> ValidateAsync(IInteractionContext context, SocketMessage message)
    {

        foreach (var criteria in _criteria)
        {

            if (!await criteria.ValidateAsync(context, message))
                return false;

        }

        return true;

    }

}