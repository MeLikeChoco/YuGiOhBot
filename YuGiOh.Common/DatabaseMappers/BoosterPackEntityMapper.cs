using Dapper.FluentMap.Mapping;
using YuGiOh.Common.Models.YuGiOh;

namespace YuGiOh.Common.DatabaseMappers
{

    public class BoosterPackEntityMapper : EntityMap<BoosterPackEntity>
    {

        public BoosterPackEntityMapper()
        {

            Map(entity => entity.DatesId)
                .ToColumn("dates", false);

            Map(entity => entity.CardsId)
                .ToColumn("cards", false);

        }

    }

}