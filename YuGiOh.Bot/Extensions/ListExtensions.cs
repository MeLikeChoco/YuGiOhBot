using System;
using System.Collections.Generic;

namespace YuGiOh.Bot.Extensions
{
    public static class ListExtensions
    {

        private static readonly Random Random = new();

        public static List<T> Shuffle<T>(this List<T> list)
        {

            var count = list.Count;

            for (var i = 0; i < count; i++)
            {

                var newPos = Random.Next(0, count);
                (list[i], list[newPos]) = (list[newPos], list[i]);

            }

            return list;

        }

        public static List<T> With<T>(this List<T> list, T item)
        {

            list.Add(item);

            return list;

        }

    }
}
