using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using YuGiOhBot.Core;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Concurrent;

namespace YuGiOhBot.Services
{
    public class YuGiOhServices
    {

        private const string DataBasePath = @"Data Source=Databases\cards.cdb;";
        private const string CardTable = "texts";
        private const string DataTable = "datas";
        private const string BasePricesUrl = "http://yugiohprices.com/api/get_card_prices/";
        private const string BaseImagesUrl = "http://yugiohprices.com/api/card_image/";
        private static Dictionary<string, string> _hexcodeToArchetype;
        private static Dictionary<int, string> _hexcodeToType;
        private static Dictionary<string, string> _hexcodeToRace;
        private static Dictionary<string, string> _hexcodeToAttribute;

        public async Task<YuGiOhCard> GetCard(string cardName)
        {

            var card = new YuGiOhCard();

            //format, archetype(if any), atk, def, level(including pend, need to convert from dec to hex), race(beast, aqua, dragon etc etc), effect(if not 0)
            string ot, setcode, type, atk, def, level, race, attribute, category;

            using (var databaseConnection = new SqliteConnection(DataBasePath))
            {

                await databaseConnection.OpenAsync();

                string id;

                using (SqliteCommand getText = databaseConnection.CreateCommand())
                {

                    getText.CommandText = $"select id,name,desc from {CardTable} where name like @CARD";
                    getText.Parameters.Add("@CARD", SqliteType.Text);
                    getText.Parameters["@CARD"].Value = cardName;

                    using (SqliteDataReader dataReader = await getText.ExecuteReaderAsync())
                    {

                        if (!dataReader.HasRows) return card;

                        await dataReader.ReadAsync();

                        card.Name = dataReader["name"].ToString();
                        card.Description = dataReader["desc"].ToString();
                        id = dataReader["id"].ToString();

                    }

                }

                using (SqliteCommand getData = databaseConnection.CreateCommand())
                {

                    getData.CommandText = $"select * from {DataTable} where (id like @ID)";
                    getData.Parameters.Add("@ID", SqliteType.Integer);
                    getData.Parameters["@ID"].Value = id;

                    using (SqliteDataReader dataReader = await getData.ExecuteReaderAsync())
                    {

                        if (await dataReader.ReadAsync())
                        {

                            ot = dataReader["ot"].ToString();
                            setcode = dataReader["setcode"].ToString();
                            type = dataReader["type"].ToString();
                            atk = dataReader["atk"].ToString();
                            def = dataReader["def"].ToString();
                            level = dataReader["level"].ToString();
                            race = dataReader["race"].ToString();
                            attribute = dataReader["attribute"].ToString();
                            category = dataReader["category"].ToString();

                        }
                        else return new YuGiOhCard();

                    }

                }

                databaseConnection.Close();

            }

            card.Format = await ConvertOtToFormat(ot);
            card.Archetype = await ConvertSetCodeToArchetype(setcode);
            card.Types = await ConvertTypeCodeToType(type);
            card.Atk = await CheckAtkDefValues(atk, card.Types);
            card.Def = await CheckAtkDefValues(def, card.Types);

            List<string> levelsAndPend = await ConvertLevelToLevelPend(level);

            //if there is 1 value in levelsandpend, it will always contain regular level
            //if there are 0 values, then we do not assign anything because by default, all
            //properties in yugiohcard is string.default or a null list
            //therefore, if there are more than 1 value in levelsandpend, it will always be a
            //pendulum monster
            if (levelsAndPend.Count == 1)
            {

                card.Level = levelsAndPend.FirstOrDefault();

            }
            else if (levelsAndPend.Count > 1)
            {

                card.LeftPend = levelsAndPend[0];
                card.RightPend = levelsAndPend[1];
                card.Level = levelsAndPend.LastOrDefault();

            }

            card.Race = await ConvertHexToRace(race);
            card.Attribute = await ConvertHexToAttribute(attribute);
            card.Prices = await GetPrices(cardName);
            card.ImageUrl = await GetImageUrl(cardName);
            card.IsEffect = await IsEffect(card);

            ////debug purposes
            //Console.WriteLine(card.Format);
            //Console.WriteLine(card.Archetype);
            //card.Types.ForEach(t => Console.Write(t));
            //Console.WriteLine(card.Atk);
            //Console.WriteLine(card.Def);
            //Console.WriteLine(card.Level);
            //Console.WriteLine(card.LeftPend);
            //Console.WriteLine(card.RightPend);
            //Console.WriteLine(card.Race);
            //Console.WriteLine(card.Attribute);
            //Console.WriteLine(card.ImageUrl);

            return card;

        }

        public async Task<List<string>> SearchCards(string search, bool isArchetypeSearch = false)
        {

            var searchResults = new List<string>();

            using (var databaseConnection = new SqliteConnection(DataBasePath))
            {

                await databaseConnection.OpenAsync();

                SqliteCommand searchCommand = databaseConnection.CreateCommand();
                //i can't sanitize this since the % marks all over the place will sanitize it, lmfao
                //das right, stahp
                searchCommand.CommandText = isArchetypeSearch ? $"select name from texts where (name like '%{search.Replace(" ", "%")}%') or (desc like '%{search.Replace(" ", "%")}%')" :
                    $"select name from texts where name like '%{search.Replace(" ", "%")}%'";


                using (SqliteDataReader dataReader = await searchCommand.ExecuteReaderAsync())
                {

                    while (await dataReader.ReadAsync())
                    {

                        var card = dataReader["name"].ToString();

                        if (!searchResults.Contains(card)) searchResults.Add(card);

                    }

                }

            }

            return searchResults;

        }

        private async Task<YuGiOhPriceSerializer> GetPrices(string cardName)
        {

            using (var http = new HttpClient())
            {

                var json = await http.GetStringAsync($"{BasePricesUrl}{Uri.EscapeUriString(cardName)}");

                if (json.StartsWith("{\"status\":\"fail\"")) return new YuGiOhPriceSerializer();

                return JsonConvert.DeserializeObject<YuGiOhPriceSerializer>(json);

            }

        }

        private async Task<string> GetImageUrl(string cardName)
        {

            //redirects are annoying eh?
            using (var http = new HttpClient())
            {

                HttpResponseMessage response = await http.GetAsync($"{BaseImagesUrl}{cardName}");

                return response.RequestMessage.RequestUri.ToString();

            }

        }

        private async Task<string> ConvertOtToFormat(string ot)
        {

            return await Task.Run(() =>
            {

                switch (int.Parse(ot))
                {

                    case 1:
                        return "OCG";
                    case 2:
                        return "TCG";
                    case 3:
                        return "OCG/TCG";
                    case 4:
                        return "Prerelease";
                    case 5:
                        return "OCG/Prerelease";
                    case 6:
                        return "TCG/Prerelease";
                    default:
                        return "Imaginary Format";

                }

            });

        }

        private async Task<string> ConvertSetCodeToArchetype(string setcode)
        {

            string archetype;
            long decimalForm = long.Parse(setcode);
            var hexcode = decimalForm.ToString("x"); //convert decimal to hexcode

            await Task.Run(() =>
            {

                foreach (KeyValuePair<string, string> archetypePair in _hexcodeToArchetype)
                {

                    if (hexcode.StartsWith(archetypePair.Key))
                    {

                        archetype = archetypePair.Value;
                        break;

                    }

                }

            });

            return string.Empty;

        }

        private async Task<List<string>> ConvertTypeCodeToType(string type)
        {

            var listOfTypes = new List<string>();
            var stringToInt = int.Parse(type);
            var intToHexcode = int.Parse(stringToInt.ToString("x")); //to hexcode

            //basically since it's sorted from large to small
            //everytime the value is larger or equal to what is in the dictionary, we will subtract it from the total value and
            //add what it is to the list of types
            //in the end, it should be equal to 0, in other words, all possible types will have been
            //went through
            await Task.Run(() =>
            {

                foreach (KeyValuePair<int, string> pair in _hexcodeToType)
                {

                    if (intToHexcode >= pair.Key)
                    {

                        listOfTypes.Add(pair.Value);
                        intToHexcode -= pair.Key;

                    }

                    if (intToHexcode == 0) break;

                }

            });

            return listOfTypes;

        }

        private async Task<List<string>> ConvertLevelToLevelPend(string level)
        {

            return await Task.Run(() =>
            {

                //check if unconverted is a valid level
                int levelToInt = int.Parse(level);
                var levels = new List<string>();

                if (levelToInt == 0) return levels;

                if (levelToInt <= 12)
                {

                    levels.Add(levelToInt.ToString());
                    return levels;

                }
                //past this point, it is 100% likely it is a pendulum               

                //pendulums will never be double digit in hexcode
                //because anything past 9 will be in letter form
                //ex. 10 will be A, 11 will be B, etc etc
                var hexcode = levelToInt.ToString("x");
                var leftPend = hexcode[0].ToString();
                var rightPend = hexcode[2].ToString();
                char cardLevel = hexcode.Last();

                var leftPendToInt = int.Parse(leftPend, NumberStyles.HexNumber);
                var rightPendToInt = int.Parse(rightPend, NumberStyles.HexNumber);

                levels.Add(leftPendToInt.ToString());
                levels.Add(rightPendToInt.ToString());
                levels.Add(cardLevel.ToString());

                return levels;

            });

        }

        private async Task<string> ConvertHexToRace(string race)
        {

            return await Task.Run(() =>
            {

                var hexcode = int.Parse(race).ToString("x");

                foreach (KeyValuePair<string, string> pair in _hexcodeToRace)
                {

                    if (hexcode.Equals(pair.Key)) return pair.Value;

                }

                return string.Empty;

            });

        }

        private async Task<string> ConvertHexToAttribute(string attribute)
        {

            return await Task.Run(() =>
            {

                var hexcode = int.Parse(attribute).ToString("x");

                foreach (KeyValuePair<string, string> pair in _hexcodeToAttribute)
                {

                    if (pair.Key.Equals(hexcode)) return pair.Value;

                }

                return string.Empty;

            });

        }

        private async Task<bool> IsEffect(YuGiOhCard card)
        {

            return await Task.Run(() =>
            {

                if (card.Types.Contains("Spell") || card.Types.Contains("Trap") || card.Types.Contains("Effect") || card.Types.Contains("Pendulum")) return true;
                else return false;

            });


        }

        private async Task<string> CheckAtkDefValues(string atkdef, List<string> types)
        {

            return await Task.Run(() =>
            {

                if (types.Contains("Spell") || types.Contains("Trap")) return string.Empty;
                else if (atkdef.Equals("-2")) return "???";
                else return atkdef;

            });

        }

        public async Task InitializeService()
        {

            await Task.Run(() =>
            {

                var lines = File.ReadAllLines("Files/Archetypes.txt");
                var tempDictionary = new SortedDictionary<string, string>(new LengthComparer());
                var textInFile = new List<string>(lines);

                textInFile.ForEach(line =>
                {

                    var hexcode = line.Substring(0, line.IndexOf(' '));
                    var archetype = line.Remove(0, line.IndexOf(' ') + 1);
                    tempDictionary.Add(hexcode, archetype);

                });

                _hexcodeToArchetype = new Dictionary<string, string>();

                //the reason for this is because there is no reliable way to check archetype by hexcode except by checking
                //if the hexcode starts with the constant values, if its a single 1, it can return the WRONG archetype, therefore
                //i need to compare by longest hexcode to smallest
                //also, thank the lord for linq, never leaving c# again
                //_hexcodeToArchetype = new SortedDictionary<string, string>(tempDictionary);
                foreach (KeyValuePair<string, string> kv in tempDictionary.Reverse())
                {

                    _hexcodeToArchetype.Add(kv.Key, kv.Value);

                }

                //debug purposes
                //foreach (KeyValuePair<string, string> pair in _hexcodeToArchetype)
                //{

                //    Console.WriteLine(pair.Key + " " + pair.Value);

                //}

                lines = File.ReadAllLines("Files/Types.txt");
                var veryTempDictionary = new Dictionary<int, string>(lines.Length);
                textInFile = new List<string>(lines);

                textInFile.ForEach(line =>
                {

                    var hexcode = int.Parse(line.Substring(0, line.IndexOf(' ')));
                    var type = line.Remove(0, line.IndexOf(' ') + 1);
                    veryTempDictionary.Add(hexcode, type);

                });

                //sort be descending because i need to subtract from large to small
                //to eliminate cases of accidental race assignments
                _hexcodeToType = new Dictionary<int, string>(veryTempDictionary.OrderByDescending(keyvaluepair => keyvaluepair.Key).ToDictionary(key => key.Key, value => value.Value));

                //debug purposes
                //foreach (KeyValuePair<int, string> pair in _hexcodeToType)
                //{

                //    Console.WriteLine(pair.Key + " " + pair.Value);

                //}

                lines = File.ReadAllLines("Files/Races.txt");
                tempDictionary.Clear();
                textInFile = new List<string>(lines);

                textInFile.ForEach(line =>
                {

                    var hexcode = line.Substring(0, line.IndexOf(' '));
                    var race = line.Remove(0, line.IndexOf(' ') + 1);
                    tempDictionary.Add(hexcode, race);

                });

                _hexcodeToRace = new Dictionary<string, string>(tempDictionary);

                //debug purposes
                //foreach(KeyValuePair<string, string> pair in _hexcodeToRace)
                //{

                //    Console.WriteLine(pair.Key + " " + pair.Value);

                //}

                lines = File.ReadAllLines("Files/Attributes.txt");
                tempDictionary.Clear();
                textInFile = new List<string>(lines);

                textInFile.ForEach(line =>
                {

                    var hexcode = line.Substring(0, line.IndexOf(' '));
                    var attribute = line.Remove(0, line.IndexOf(' ') + 1);
                    tempDictionary.Add(hexcode, attribute);

                });

                _hexcodeToAttribute = new Dictionary<string, string>(tempDictionary);

                //debug purposes
                //foreach (KeyValuePair<string, string> pair in _hexcodeToAttribute)
                //{

                //    Console.WriteLine(pair.Key + " " + pair.Value);

                //}        

            });

        }

        //public async Task<Dictionary<string,string>> SearchCard(string cardName)
        //{

        //    var cards = new Dictionary<string, string>();

        //    //C:\Users\Edgar Chang\Documents\Visual Studio 2017\Projects\YunoBot\src\YunoBot\Databases\cards.cdb;
        //    using (var databaseConnection = new SqliteConnection(DataBasePath))
        //    {

        //        await databaseConnection.OpenAsync();

        //        SqliteCommand getMatchingNames = databaseConnection.CreateCommand();
        //        getMatchingNames.CommandText = $"select name,desc from {CardTable} where name like '%{cardName.Replace(" ", "%")}%'";

        //        using(SqliteDataReader dataReader = await getMatchingNames.ExecuteReaderAsync())
        //        {

        //            while(await dataReader.ReadAsync())
        //            {

        //                if (!cards.ContainsKey(dataReader["name"].ToString()))
        //                {

        //                    cards.Add(dataReader["name"].ToString(), dataReader["desc"].ToString());

        //                }                        

        //            }

        //        }

        //        databaseConnection.Close();

        //    }

        //    return cards;

        //}

    }

    class LengthComparer : IComparer<String>
    {
        public int Compare(string x, string y)
        {
            int lengthComparison = x.Length.CompareTo(y.Length);

            if (lengthComparison == 0)
            {
                return x.CompareTo(y);
            }
            else
            {
                return lengthComparison;
            }
        }
    }
}
