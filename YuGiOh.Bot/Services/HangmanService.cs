using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YuGiOh.Common.Extensions;

namespace YuGiOh.Bot.Services
{
    public class HangmanService
    {

        public string Word { get; }

        public CompletionStatus CompletionStatus
        {

            get
            {

                if (_nooseStage == 6)
                    return CompletionStatus.Hanged;
                else if (Word.SequenceEqual(_current))
                    return CompletionStatus.Complete;
                else
                    return CompletionStatus.Incomplete;

            }

        }


        private readonly char[] _current;
        private int _nooseStage;
        private readonly HashSet<string> _guesses;

        public HangmanService(string word)
        {

            Word = word;
            _current = new char[word.Length];
            _nooseStage = 0;
            _guesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < word.Length; i++)
            {

                var character = word[i];

                if (!char.IsLetterOrDigit(character))
                    _current[i] = character;

            }

        }

        public GuessStatus AddGuess(string guess)
        {

            if (!Word.ContainsIgnoreCase(guess))
            {

                _guesses.Add(guess);
                _nooseStage++;

                return GuessStatus.Nonexistent;

            }

            if (_guesses.Contains(guess))
                return GuessStatus.Duplicate;

            _guesses.Add(guess);

            guess = Regex.Escape(guess);

            foreach (Match match in Regex.Matches(Word, guess, RegexOptions.IgnoreCase))
            {

                for (var i = match.Index; i < match.Index + match.Length; i++)
                    _current[i] = Word[i];

            }

            return GuessStatus.Accepted;

        }

        public string GetCurrentDisplay()
        {

            var strBuilder = new StringBuilder();
            var character = _current[0];

            if (character == default(char))
                strBuilder.Append("\\_");
            else if (char.IsLetterOrDigit(character))
                strBuilder.Append("__").Append(character).Append("__");
            else
                strBuilder.Append(character);

            return _current
                .Skip(1)
                .Aggregate(
                    strBuilder,
                    (strBuilder, character) =>
                    {

                        if (character == default(char))
                            return strBuilder.Append(" \\_");
                        else if (char.IsWhiteSpace(character))
                            return strBuilder.Append("  "); //double space instead of triple because the others add a space before their character
                        else if (char.IsLetterOrDigit(character))
                            return strBuilder.Append(" __").Append(character).Append("__");
                        else
                            return strBuilder.Append(' ').Append(character);

                    })
                .ToString();

        }

        public string GetHangman()
        {

            const string noose = " _________     \n" +
                                 "|         |    \n";

            return _nooseStage switch
            {
                1 => noose +
                    "|         0    \n",
                2 => noose +
                    "|         0    \n" +
                    "|         |    \n",
                3 => noose +
                    "|         0    \n" +
                    "|        /|    \n",
                4 => noose +
                    "|         0    \n" +
                    "|        /|\\  \n",
                5 => noose +
                    "|         0    \n" +
                    "|        /|\\  \n" +
                    "|        /     \n",
                6 => noose +
                    "|         0    \n" +
                    "|        /|\\  \n" +
                    "|        / \\  \n",
                _ => "",
            };

        }

    }

    public enum GuessStatus
    {

        Accepted,
        Duplicate,
        Nonexistent

    }

    public enum CompletionStatus
    {

        Complete,
        Incomplete,
        Hanged

    }
}
