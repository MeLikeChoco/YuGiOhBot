using Dapper.Contrib.Extensions;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using YuGiOhV2.Objects.Attributes;
using YuGiOhV2.Objects.BoosterPacks.BoosterPackItems;

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
        [Column("worldwideReleaseDate")]
        public string WorldReleaseDate { get; set; }
        public string Cards { get; private set; }
        public string Breakdown { get; private set; }

        private int _amount;

        public int Amount
        {
            get
            {

                if(_amount == 0)
                    _amount = Breakdown.Split(" | ").Sum(line => int.Parse(line.Split('/').First()));

                return _amount;

            }
        }

        private BoosterPackBreakdown _boosterPackBreakdown;

        public BoosterPackBreakdown BoosterPackBreakdown
        {

            get
            {

                if (_boosterPackBreakdown == null)
                {

                    var breakdown = new BoosterPackBreakdown();
                    var array = Breakdown.Split(" | ");

                    foreach (var line in array)
                    {

                        var temp = line.Split('/');
                        var amount = temp[0].Split(' ').First();
                        var rarity = temp[1];

                        breakdown.GetType().GetProperties().FirstOrDefault(property => property.GetCustomAttributes().Any(attribute =>
                        {

                            if (attribute is RarityAttribute rarityAttr)
                                return rarityAttr.Rarity == rarity;

                            return false;


                        })).SetValue(breakdown, int.Parse(amount));

                    }

                    _boosterPackBreakdown = breakdown;

                }

                return _boosterPackBreakdown;

            }

        }

        private List<BoosterPackCard> _boosterPackCards;

        public List<BoosterPackCard> BoosterPackCards
        {

            get
            {

                if (_boosterPackCards == null)
                {

                    var array = Cards.Split(" | ");
                    var cards = new List<BoosterPackCard>(array.Length);

                    foreach (var line in array)
                    {

                        var temp = line.Split('/');
                        BoosterPackCard card;

                        if(temp.Length == 2)
                        {

                            var name = temp[0];
                            var rarity = temp[1];
                            card = new BoosterPackCard(name, rarity);

                        }
                        else
                        {

                            var cardNumber = temp[0];
                            var name = temp[1];
                            var rarity = temp[2];
                            card = new BoosterPackCard(name, rarity, cardNumber);

                        }                       

                        cards.Add(card);

                    }

                    _boosterPackCards = cards;

                }

                return _boosterPackCards;

            }

        }

        public IEnumerable<BoosterPackCard> Open()
        {
            
            var properties = BoosterPackBreakdown.GetType().GetProperties().Where(property => (int)property.GetValue(BoosterPackBreakdown) > 0);
            var cards = new List<BoosterPackCard>(Amount);

            foreach (var property in properties)
            {

                var attribute = property.GetCustomAttribute<RarityAttribute>();
                var amount = (int)property.GetValue(BoosterPackBreakdown);
                var possibilities = BoosterPackCards.Where(card => card.Rarity == attribute.Rarity);

                for (int i = 0; i < amount; i++)
                {

                    if(amount < possibilities.Count())
                        cards.Add(possibilities.RandomSubset(1).First());
                    else
                    {

                        BoosterPackCard card;

                        do
                            card = possibilities.RandomSubset(1).First();
                        while (cards.Contains(card));

                        cards.Add(card);

                    }

                }

            }

            return cards;

        }

        public bool IsEllegible()
        {

            if (Cards == null && Breakdown == null)
                return false;

            try
            {

                Breakdown.Split(", ").Select(rarity => rarity.Split('/')).Select(rarityArray => rarityArray[0]).ForEach(amount => int.Parse(amount));
                return true;

            }
            catch (Exception)
            {

                return false;

            }

        }

    }
}
