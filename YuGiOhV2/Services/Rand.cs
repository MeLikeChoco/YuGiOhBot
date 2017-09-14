using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Services
{
    public static class Rand
    {

        private static Random _rand = new Random();

        public static int NextInt(int exclusiveMax)
        {

            return _rand.Next(exclusiveMax);

        }

        public static int NextInt(int inclusiveMin, int exclusiveMax)
        {

            return _rand.Next(inclusiveMin, exclusiveMax);

        }

    }
}
