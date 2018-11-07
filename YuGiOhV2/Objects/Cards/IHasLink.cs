using System;
using System.Collections.Generic;
using System.Text;

namespace YuGiOhV2.Objects.Cards
{
    public interface IHasLink
    {

        int Link { get; set; }
        string[] LinkArrows { get; set; }

    }
}
