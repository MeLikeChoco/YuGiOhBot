﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhScraper.Extensions
{
    public static class GeneralExtensions
    {

        public static void DoIf<T>(this T x, Predicate<T> conditions, Action<T> work)
        {

            if (conditions.Invoke(x))
                work.Invoke(x);

        }

        public static T DoIf<T>(this T x, Predicate<T> conditions, Func<T, T> work)
        {

            if (conditions.Invoke(x))
                x = work.Invoke(x);

            return x;

        }

    }
}
