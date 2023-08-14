using Dapper.FluentMap.Mapping;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Common.DatabaseMappers
{
    internal class CardEntityMapper : EntityMap<CardEntity>
    {

        public CardEntityMapper()
        {

            Map(entity => entity.ArchetypesId)
                .ToColumn("archetypes", false);

            Map(entity => entity.SupportsId)
                .ToColumn("supports", false);

            Map(entity => entity.AntiSupportsId)
                .ToColumn("antisupports", false);

        }

    }
}