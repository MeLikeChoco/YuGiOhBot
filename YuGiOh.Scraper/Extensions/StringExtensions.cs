﻿using System.IO;
using System.Text;

namespace YuGiOh.Scraper.Extensions
{
    public static class StringExtensions
    {

        private static readonly uint Seed = Options.Instance.Config.HashSeed;

        public static string GetMurMurHash(this string str)
        {

            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;

            var h1 = Seed;
            uint streamLength = 0;

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var reader = new BinaryReader(stream))
            {
                var chunk = reader.ReadBytes(4);
                while (chunk.Length > 0)
                {
                    streamLength += (uint) chunk.Length;
                    uint k1 = 0;
                    switch (chunk.Length)
                    {
                        case 4:
                            /* Get four bytes from the input into an uint */
                            k1 = (uint)
                                (chunk[0]
                                 | chunk[1] << 8
                                 | chunk[2] << 16
                                 | chunk[3] << 24);

                            /* bitmagic hash */
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;

                            h1 ^= k1;
                            h1 = Rotl32(h1, 13);
                            h1 = h1 * 5 + 0xe6546b64;
                            break;
                        case 3:
                            k1 = (uint)
                                (chunk[0]
                                 | chunk[1] << 8
                                 | chunk[2] << 16);
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;
                        case 2:
                            k1 = (uint)
                                (chunk[0]
                                 | chunk[1] << 8);
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;
                        case 1:
                            k1 = chunk[0];
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;

                    }

                    chunk = reader.ReadBytes(4);
                }
            }

            // finalization, magic chants to wrap it all up
            h1 ^= streamLength;
            h1 = Fmix(h1);

            unchecked //ignore overflow
            {
                return ((int) h1).ToString("X");
            }
        }

        private static uint Rotl32(uint x, byte r)
        {
            return (x << r) | (x >> (32 - r));
        }

        private static uint Fmix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }

    }
}