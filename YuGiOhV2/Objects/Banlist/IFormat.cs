using System.Collections.Generic;

namespace YuGiOhV2.Objects.Banlist
{
    public interface IFormat
    {

        IEnumerable<string> Forbidden { get; set; }
        IEnumerable<string> Limited { get; set; }
        IEnumerable<string> SemiLimited { get; set; }

    }
}
