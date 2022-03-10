using System;

namespace YuGiOh.Bot.Models.Exceptions
{
    public class EmbedGenerationException : Exception
    {

        public EmbedGenerationException(string cardName, Exception ex)
            : base($"{cardName}\n{ex.Message}", ex) { }

    }
}
