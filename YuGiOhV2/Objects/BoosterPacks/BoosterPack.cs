using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.BoosterPacks
{
    public class BoosterPack
    {

        public string Name { get; private set; }
        public string JpDate { get; private set; }
        public string NaDate { get; private set; }
        
        private BoosterPackCard[] _cards;
        public BoosterPackCard[] Cards
        {
            get
            {

                if (_cards == null)
                    _cards = JsonConvert.DeserializeObject<BoosterPackCard[]>(_cardsJson, new JsonSerializerSettings() { ContractResolver = new PrivatePropertyContractResolver() });

                return _cards;

            }
        }

        [JsonProperty("CardsJson")]
        private string _cardsJson;

    }
}
