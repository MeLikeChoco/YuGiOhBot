using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Extensions
{
    public static class ListExtensions
    {

        public static Random random = new Random();

        public static List<T> Shuffle<T>(this List<T> list)
        {

            var count = list.Count;

            for (int i = 0; i < count; i++)
            {

                var newPos = random.Next(0, count);
                var temp = list[i];

                list[i] = list[newPos];
                list[newPos] = temp;

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
