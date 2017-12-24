using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace YuGiOhV2.Objects.BoosterPacks
{
    public class Booster
    {

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string EnReleaseDate { get; set; }
        public string JpReleaseDate { get; set; }
        public string SkReleaseDate { get; set; }
        public string WorldReleaseDate { get; set; }
        public string Cards { get; set; }

    }
}
