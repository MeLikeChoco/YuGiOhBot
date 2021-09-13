using System;
using System.Collections.Generic;
using System.Text;

namespace YuGiOh.Bot.Models.Cards
{
    public interface IHasLink
    {

        int Link { get; set; }
        string[] LinkArrows { get; set; }

    }
}
