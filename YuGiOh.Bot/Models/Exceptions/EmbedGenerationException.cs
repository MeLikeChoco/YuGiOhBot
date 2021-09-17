using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOh.Bot.Models.Exceptions
{
    public class EmbedGenerationException : Exception
    {

        public EmbedGenerationException(string cardName, Exception ex)
            : base($"{cardName}\n{ex.Message}", ex) { }

    }
}
