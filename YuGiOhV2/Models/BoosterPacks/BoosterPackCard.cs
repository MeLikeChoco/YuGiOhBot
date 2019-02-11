using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Models.BoosterPacks
{

    public struct BoosterPackCard
    {

        public string Name { get; private set; }
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public Rarity[] Rarity { get; private set; }

    }

}
