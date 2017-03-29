//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
//using Discord.Addons.InteractiveCommands;
//using YuGiOhBot.Services;
//using YuGiOhBot.Services.CardObjects;

//namespace YuGiOhBot.Commands
//{
//    public class ShortCoreCommands : InteractiveModuleBase
//    {

//        private YuGiOhServices _service;

//        public ShortCoreCommands(YuGiOhServices serviceParams)
//        {
            
//            _service = serviceParams;

//        }

//        [Command("card"), Alias("c")]
//        [Summary("Returns information based on given card name")]
//        public async Task CardCommand([Remainder]string cardName)
//        {

//            if (string.IsNullOrEmpty(cardName))
//            {

//                await ReplyAsync("There is no card with no name, unless it's the shadow realm. Everyone is just a soul there.");
//                return;

//            }

//            if (CacheService.YuGiOhCardCache.TryGetValue(cardName.ToLower(), out EmbedBuilder eBuilder))
//            {

//                await ReplyAsync("", embed: eBuilder);
//                return;

//            }

//            using (var typingState = Context.Channel.EnterTypingState())
//            {

//                YuGiOhCard card = await _service.GetCard(cardName);

//                if (string.IsNullOrEmpty(card.Name))
//                {

//                    await ReplyAsync($"No card by the name {cardName} was found!");
//                    typingState.Dispose();
//                    return;

//                }

//                var authorBuilder = new EmbedAuthorBuilder()
//                {

//                    Name = "YuGiOh",
//                    IconUrl = "http://card-masters.com/cardmasters/wp-content/uploads/2013/08/yugioh-product-icon-lg.png",
//                    Url = "http://www.yugioh-card.com/en/"

//                };

//                var footerBuilder = new EmbedFooterBuilder()
//                {

//                    Text = "It's time to d-d-d-d-duel | Database made by chinhodado",
//                    IconUrl = "http://i722.photobucket.com/albums/ww227/omar_alami/icon_gold_classic_zps66eae1c7.png"

//                };

//                var organizedDescription = new StringBuilder();

//                if (!string.IsNullOrEmpty(card.RealName)) organizedDescription.AppendLine($"**Real Name:** {card.RealName}");

//                organizedDescription.AppendLine($"**Format:** {GetFormat(card.TcgOnly, card.OcgOnly)}");
//                organizedDescription.AppendLine($"**Card Type:** {card.CardType}");

//                if (card is MonsterCard)
//                {

//                    var monster = card as MonsterCard;
//                    organizedDescription.AppendLine($"**Attribute:** {monster.Attribute}");

//                    if (monster is XyzMonster)
//                    {
//                        var xyz = monster as XyzMonster;
//                        organizedDescription.AppendLine($"**Rank:** {xyz.Rank}");
//                    }
//                    else if (monster is LinkMonster)
//                    {
//                        var link = monster as LinkMonster;
//                        organizedDescription.AppendLine($"**Links:** {link.Links}");
//                        organizedDescription.AppendLine($"**Link Markers:** {link.LinkMarkers}");
//                    }
//                    else
//                    {
//                        var regular = monster as RegularMonster;
//                        organizedDescription.AppendLine($"**Level:** {regular.Level}");
//                    }

//                    if (monster is PendulumMonster)
//                    {
//                        var pendulum = monster as PendulumMonster;
//                        organizedDescription.AppendLine($"**Scale:** {pendulum.PendulumScale}");
//                    }

//                }
//                else
//                {

//                    var spelltrap = card as SpellTrapCard;
//                    organizedDescription.AppendLine($"**Property:** {spelltrap.Property}");

//                }

//                eBuilder = new EmbedBuilder()
//                {

//                    Author = authorBuilder,
//                    Color = WhatColorIsTheCard(card),
//                    ImageUrl = card.ImageUrl,
//                    //ThumbnailUrl = card.ImageUrl,
//                    Title = card.Name,
//                    Description = organizedDescription.ToString(),
//                    Footer = footerBuilder

//                };

//                string description;

//                if (card.Lore.StartsWith("Pendulum Effect")) //not all pendulum monsters have an effect
//                {

//                    var monster = card as MonsterCard;
//                    var tempArray = card.Lore.Split(new string[] { "Monster Effect" }, StringSplitOptions.None);
//                    description = $"{monster.Types}\n" + "__Pendulum Effect__\n" + tempArray[0].Replace("Pendulum Effect", "").Replace("\" ", "\"").Trim() + "\n__Monster Effect__\n" + tempArray[1].Replace("Monster Effect", "").Replace("\" ", "\"").Trim();

//                }
//                else if (card is MonsterCard)
//                {

//                    var monster = card as MonsterCard;
//                    description = $"{monster.Types}\n" + card.Lore;

//                }
//                else description = card.Lore;

//                eBuilder.AddField(x =>
//                {

//                    x.Name = card.HasEffect ? "Effect" : "Description";
//                    x.Value = description;
//                    x.IsInline = false;

//                });

//                if (card is MonsterCard)
//                {

//                    var monster = card as MonsterCard;

//                    eBuilder.AddField(x =>
//                    {
//                        x.Name = "Attack";
//                        x.Value = monster.Atk;
//                        x.IsInline = !(monster is LinkMonster);
//                    });

//                    if (!(monster is LinkMonster))
//                    {

//                        eBuilder.AddField(x =>
//                        {
//                            x.Name = "Defense";
//                            x.Value = monster.Def;
//                            x.IsInline = true;
//                        });

//                    }

//                }

//                if (!string.IsNullOrEmpty(card.Archetype))
//                {

//                    eBuilder.AddField(x =>
//                    {

//                        x.Name = "Archetype(s)";
//                        x.Value = card.Archetype;
//                        x.IsInline = false;

//                    });

//                }

//                if (card.Prices.data != null)
//                {

//                    var organizedPrices = new StringBuilder();

//                    //debug usage
//                    //card.Prices.data.ForEach(d => Console.WriteLine(d.price_data.data.prices.average));
//                    if (card.Prices.data == null)
//                    {

//                        organizedPrices.Append("**No prices to show.**");

//                    }
//                    else if (card.Prices.data.Count > 4)
//                    {

//                        List<Datum> prices = card.Prices.data;

//                        organizedPrices.AppendLine("**Showing the first 3 prices due to too many available.**");

//                        for (int counter = 0; counter < 3; counter++)
//                        {

//                            Datum data = prices[counter];

//                            organizedPrices.AppendLine($"**Name:** {data.name}");
//                            organizedPrices.AppendLine($"\t\tRarity: {data.rarity}");
//                            //this is what redundancy looks like people, lmfao
//                            organizedPrices.AppendLine($"\t\tHigh: ${data.price_data.data.prices.high.ToString("0.00")}");
//                            organizedPrices.AppendLine($"\t\tLow: ${data.price_data.data.prices.low.ToString("0.00")}");
//                            organizedPrices.AppendLine($"\t\tAverage: ${data.price_data.data.prices.average.ToString("0.00")}");

//                        }

//                    }
//                    else if (card.Prices.data.Count < 4)
//                    {

//                        foreach (Datum data in card.Prices.data)
//                        {

//                            organizedPrices.AppendLine($"**Name:** {data.name}");
//                            organizedPrices.AppendLine($"\t\tRarity: {data.rarity}");
//                            //this is what redundancy looks like people, lmfao

//                            if (data.price_data.data == null)
//                            {

//                                organizedPrices.AppendLine($"\t\tError: No prices to display for this card variant.");

//                            }
//                            else
//                            {


//                                organizedPrices.AppendLine($"\t\tHigh: ${data.price_data.data.prices.high.ToString("0.00")}");
//                                organizedPrices.AppendLine($"\t\tLow: ${data.price_data.data.prices.low.ToString("0.00")}");
//                                organizedPrices.AppendLine($"\t\tAverage: ${data.price_data.data.prices.average.ToString("0.00")}");

//                            }

//                        }

//                    }

//                    eBuilder.AddField(x =>
//                    {

//                        x.Name = "Prices";
//                        x.Value = organizedPrices.ToString();
//                        x.IsInline = false;

//                    });

//                }

//            }

//            await ReplyAsync("", embed: eBuilder);
//            CacheService.YuGiOhCardCache.TryAdd(cardName.ToLower(), eBuilder);

//        }

//        [Command("lcard"), Alias("lc")]
//        [Summary("Word position does not matter and will pull the first available result in the search")]
//        public async Task LazyCardCommand([Remainder]string cardName)
//        {

//            if (string.IsNullOrEmpty(cardName))
//            {

//                await ReplyAsync("There are no cards with a lack of name.");
//                return;

//            }

//            EmbedBuilder eBuilder;

//            using (var typingState = Context.Channel.EnterTypingState())
//            {

//                YuGiOhCard card = await _service.LazyGetCard(cardName);

//                if (card.Name.Equals(string.Empty))
//                {

//                    await ReplyAsync($"No card by the name {cardName} was found!");
//                    typingState.Dispose();
//                    return;

//                }

//                if (CacheService.YuGiOhCardCache.TryGetValue(card.Name.ToLower(), out eBuilder))
//                {

//                    await ReplyAsync("", embed: eBuilder);
//                    typingState.Dispose();
//                    return;

//                }

//                var authorBuilder = new EmbedAuthorBuilder()
//                {

//                    Name = "YuGiOh",
//                    IconUrl = "http://card-masters.com/cardmasters/wp-content/uploads/2013/08/yugioh-product-icon-lg.png",
//                    Url = "http://www.yugioh-card.com/en/"

//                };

//                var footerBuilder = new EmbedFooterBuilder()
//                {

//                    Text = "It's time to d-d-d-d-duel | Database made by chinhodado",
//                    IconUrl = "http://i722.photobucket.com/albums/ww227/omar_alami/icon_gold_classic_zps66eae1c7.png"

//                };

//                var organizedDescription = new StringBuilder();

//                if (!string.IsNullOrEmpty(card.RealName)) organizedDescription.AppendLine($"**Real Name:** {card.RealName}");

//                organizedDescription.AppendLine($"**Format:** {GetFormat(card.TcgOnly, card.OcgOnly)}");
//                organizedDescription.AppendLine($"**Card Type:** {card.CardType}");

//                if (card is MonsterCard)
//                {

//                    var monster = card as MonsterCard;
//                    organizedDescription.AppendLine($"**Attribute:** {monster.Attribute}");

//                    if (monster is XyzMonster)
//                    {
//                        var xyz = monster as XyzMonster;
//                        organizedDescription.AppendLine($"**Rank:** {xyz.Rank}");
//                    }
//                    else if (monster is LinkMonster)
//                    {
//                        var link = monster as LinkMonster;
//                        organizedDescription.AppendLine($"**Links:** {link.Links}");
//                        organizedDescription.AppendLine($"**Link Markers:** {link.LinkMarkers}");
//                    }
//                    else
//                    {
//                        var regular = monster as RegularMonster;
//                        organizedDescription.AppendLine($"**Level:** {regular.Level}");
//                    }

//                    if (monster is PendulumMonster)
//                    {
//                        var pendulum = monster as PendulumMonster;
//                        organizedDescription.AppendLine($"**Scale:** {pendulum.PendulumScale}");
//                    }

//                }
//                else
//                {

//                    var spelltrap = card as SpellTrapCard;
//                    organizedDescription.AppendLine($"**Property:** {spelltrap.Property}");

//                }

//                eBuilder = new EmbedBuilder()
//                {

//                    Author = authorBuilder,
//                    Color = WhatColorIsTheCard(card),
//                    ImageUrl = card.ImageUrl,
//                    //ThumbnailUrl = card.ImageUrl,
//                    Title = card.Name,
//                    Description = organizedDescription.ToString(),
//                    Footer = footerBuilder

//                };

//                string description;

//                if (card.Lore.StartsWith("Pendulum Effect")) //not all pendulum monsters have an effect
//                {

//                    var tempArray = card.Lore.Split(new string[] { "Monster Effect" }, StringSplitOptions.None);
//                    description = "__Pendulum Effect__\n" + tempArray[0].Replace("Pendulum Effect", "").Replace("\" ", "\"").Trim() + "\n__Monster Effect__\n" + tempArray[1].Replace("Monster Effect", "").Replace("\" ", "\"").Trim();

//                }
//                else description = card.Lore;

//                eBuilder.AddField(x =>
//                {

//                    x.Name = card.HasEffect ? "Effect" : "Description";
//                    x.Value = description;
//                    x.IsInline = false;

//                });

//                if (card is MonsterCard)
//                {

//                    var monster = card as MonsterCard;

//                    eBuilder.AddField(x =>
//                    {
//                        x.Name = "Attack";
//                        x.Value = monster.Atk;
//                        x.IsInline = !(monster is LinkMonster);
//                    });

//                    if (!(monster is LinkMonster))
//                    {

//                        var regular = monster as RegularMonster;

//                        eBuilder.AddField(x =>
//                        {
//                            x.Name = "Defense";
//                            x.Value = regular.Def;
//                            x.IsInline = true;
//                        });

//                    }

//                }

//                if (!string.IsNullOrEmpty(card.Archetype))
//                {

//                    eBuilder.AddField(x =>
//                    {

//                        x.Name = "Archetype(s)";
//                        x.Value = card.Archetype;
//                        x.IsInline = false;

//                    });

//                }

//                if (card.Prices.data != null)
//                {

//                    var organizedPrices = new StringBuilder();

//                    //debug usage
//                    //card.Prices.data.ForEach(d => Console.WriteLine(d.price_data.data.prices.average));
//                    if (card.Prices.data == null)
//                    {

//                        organizedPrices.Append("**No prices to show.**");

//                    }
//                    else if (card.Prices.data.Count > 4)
//                    {

//                        List<Datum> prices = card.Prices.data;

//                        organizedPrices.AppendLine("**Showing the first 7 prices due to too many available.**");

//                        for (int counter = 0; counter < 3; counter++)
//                        {

//                            Datum data = prices[counter];

//                            organizedPrices.AppendLine($"**Name:** {data.name}");
//                            organizedPrices.AppendLine($"\t\tRarity: {data.rarity}");
//                            //this is what redundancy looks like people, lmfao
//                            organizedPrices.AppendLine($"\t\tHigh: ${data.price_data.data.prices.high.ToString("0.00")}");
//                            organizedPrices.AppendLine($"\t\tLow: ${data.price_data.data.prices.low.ToString("0.00")}");
//                            organizedPrices.AppendLine($"\t\tAverage: ${data.price_data.data.prices.average.ToString("0.00")}");

//                        }

//                    }
//                    else if (card.Prices.data.Count < 4)
//                    {

//                        foreach (Datum data in card.Prices.data)
//                        {

//                            organizedPrices.AppendLine($"**Name:** {data.name}");
//                            organizedPrices.AppendLine($"\t\tRarity: {data.rarity}");
//                            //this is what redundancy looks like people, lmfao

//                            if (data.price_data.data == null)
//                            {

//                                organizedPrices.AppendLine($"\t\tError: No prices to display for this card variant.");

//                            }
//                            else
//                            {


//                                organizedPrices.AppendLine($"\t\tHigh: ${data.price_data.data.prices.high.ToString("0.00")}");
//                                organizedPrices.AppendLine($"\t\tLow: ${data.price_data.data.prices.low.ToString("0.00")}");
//                                organizedPrices.AppendLine($"\t\tAverage: ${data.price_data.data.prices.average.ToString("0.00")}");

//                            }

//                        }

//                    }

//                    eBuilder.AddField(x =>
//                    {

//                        x.Name = "Prices";
//                        x.Value = organizedPrices.ToString();
//                        x.IsInline = false;

//                    });

//                }

//                CacheService.YuGiOhCardCache.TryAdd(card.Name.ToLower(), eBuilder);

//            }

//            await ReplyAsync("", embed: eBuilder);

//        }

//        [Command("rcard"), Alias("r", "random", "rc")]
//        [Summary("Returns a random card!")]
//        public async Task RandomCardCommand()
//        {

//            EmbedBuilder eBuilder;

//            using (var typingState = Context.Channel.EnterTypingState())
//            {

//                YuGiOhCard card = await _service.GetRandomCard();

//                if (CacheService.YuGiOhCardCache.TryGetValue(card.Name.ToLower(), out eBuilder))
//                {

//                    await ReplyAsync("", embed: eBuilder);
//                    typingState.Dispose();
//                    return;

//                }

//                var authorBuilder = new EmbedAuthorBuilder()
//                {

//                    Name = "YuGiOh",
//                    IconUrl = "http://card-masters.com/cardmasters/wp-content/uploads/2013/08/yugioh-product-icon-lg.png",
//                    Url = "http://www.yugioh-card.com/en/"

//                };

//                var footerBuilder = new EmbedFooterBuilder()
//                {

//                    Text = "It's time to d-d-d-d-duel | Database made by chinhodado",
//                    IconUrl = "http://i722.photobucket.com/albums/ww227/omar_alami/icon_gold_classic_zps66eae1c7.png"

//                };

//                var organizedDescription = new StringBuilder();

//                if (!string.IsNullOrEmpty(card.RealName)) organizedDescription.AppendLine($"**Real Name:** {card.RealName}");

//                organizedDescription.AppendLine($"**Format:** {GetFormat(card.TcgOnly, card.OcgOnly)}");
//                organizedDescription.AppendLine($"**Card Type:** {card.CardType}");

//                if (card is MonsterCard)
//                {

//                    var monster = card as MonsterCard;
//                    organizedDescription.AppendLine($"**Attribute:** {monster.Attribute}");

//                    if (monster is XyzMonster)
//                    {
//                        var xyz = monster as XyzMonster;
//                        organizedDescription.AppendLine($"**Rank:** {xyz.Rank}");
//                    }
//                    else if (monster is LinkMonster)
//                    {
//                        var link = monster as LinkMonster;
//                        organizedDescription.AppendLine($"**Links:** {link.Links}");
//                        organizedDescription.AppendLine($"**Link Markers:** {link.LinkMarkers}");
//                    }
//                    else
//                    {
//                        var regular = monster as RegularMonster;
//                        organizedDescription.AppendLine($"**Level:** {regular.Level}");
//                    }

//                    if (monster is PendulumMonster)
//                    {
//                        var pendulum = monster as PendulumMonster;
//                        organizedDescription.AppendLine($"**Scale:** {pendulum.PendulumScale}");
//                    }

//                }
//                else
//                {

//                    var spelltrap = card as SpellTrapCard;
//                    organizedDescription.AppendLine($"**Property:** {spelltrap.Property}");

//                }

//                eBuilder = new EmbedBuilder()
//                {

//                    Author = authorBuilder,
//                    Color = WhatColorIsTheCard(card),
//                    ImageUrl = card.ImageUrl,
//                    //ThumbnailUrl = card.ImageUrl,
//                    Title = card.Name,
//                    Description = organizedDescription.ToString(),
//                    Footer = footerBuilder

//                };

//                string description;

//                if (card.Lore.StartsWith("Pendulum Effect")) //not all pendulum monsters have an effect
//                {

//                    var tempArray = card.Lore.Split(new string[] { "Monster Effect" }, StringSplitOptions.None);
//                    description = "__Pendulum Effect__\n" + tempArray[0].Replace("Pendulum Effect", "").Replace("\" ", "\"").Trim() + "\n__Monster Effect__\n" + tempArray[1].Replace("Monster Effect", "").Replace("\" ", "\"").Trim();

//                }
//                else description = card.Lore;

//                eBuilder.AddField(x =>
//                {

//                    x.Name = card.HasEffect ? "Effect" : "Description";
//                    x.Value = description;
//                    x.IsInline = false;

//                });

//                if (card is MonsterCard)
//                {

//                    var monster = card as MonsterCard;

//                    eBuilder.AddField(x =>
//                    {
//                        x.Name = "Attack";
//                        x.Value = monster.Atk;
//                        x.IsInline = !(monster is LinkMonster);
//                    });

//                    if (!(monster is LinkMonster))
//                    {

//                        var regular = monster as RegularMonster;

//                        eBuilder.AddField(x =>
//                        {
//                            x.Name = "Defense";
//                            x.Value = regular.Def;
//                            x.IsInline = true;
//                        });

//                    }

//                }

//                if (!string.IsNullOrEmpty(card.Archetype))
//                {

//                    eBuilder.AddField(x =>
//                    {

//                        x.Name = "Archetype(s)";
//                        x.Value = card.Archetype;
//                        x.IsInline = false;

//                    });

//                }

//                if (card.Prices.data != null)
//                {

//                    var organizedPrices = new StringBuilder();

//                    //debug usage
//                    //card.Prices.data.ForEach(d => Console.WriteLine(d.price_data.data.prices.average));
//                    if (card.Prices.data == null)
//                    {

//                        organizedPrices.Append("**No prices to show.**");

//                    }
//                    else if (card.Prices.data.Count > 4)
//                    {

//                        List<Datum> prices = card.Prices.data;

//                        organizedPrices.AppendLine("**Showing the first 3 prices due to too many available.**");

//                        for (int counter = 0; counter < 3; counter++)
//                        {

//                            Datum data = prices[counter];

//                            organizedPrices.AppendLine($"**Name:** {data.name}");
//                            organizedPrices.AppendLine($"\t\tRarity: {data.rarity}");
//                            //this is what redundancy looks like people, lmfao
//                            organizedPrices.AppendLine($"\t\tHigh: ${data.price_data.data.prices.high.ToString("0.00")}");
//                            organizedPrices.AppendLine($"\t\tLow: ${data.price_data.data.prices.low.ToString("0.00")}");
//                            organizedPrices.AppendLine($"\t\tAverage: ${data.price_data.data.prices.average.ToString("0.00")}");

//                        }

//                    }
//                    else if (card.Prices.data.Count < 4)
//                    {

//                        foreach (Datum data in card.Prices.data)
//                        {

//                            organizedPrices.AppendLine($"**Name:** {data.name}");
//                            organizedPrices.AppendLine($"\t\tRarity: {data.rarity}");
//                            //this is what redundancy looks like people, lmfao

//                            if (data.price_data.data == null)
//                            {

//                                organizedPrices.AppendLine($"\t\tError: No prices to display for this card variant.");

//                            }
//                            else
//                            {


//                                organizedPrices.AppendLine($"\t\tHigh: ${data.price_data.data.prices.high.ToString("0.00")}");
//                                organizedPrices.AppendLine($"\t\tLow: ${data.price_data.data.prices.low.ToString("0.00")}");
//                                organizedPrices.AppendLine($"\t\tAverage: ${data.price_data.data.prices.average.ToString("0.00")}");

//                            }

//                        }

//                    }

//                    eBuilder.AddField(x =>
//                    {

//                        x.Name = "Prices";
//                        x.Value = organizedPrices.ToString();
//                        x.IsInline = false;

//                    });

//                }

//                CacheService.YuGiOhCardCache.TryAdd(card.Name.ToLower(), eBuilder);

//            }

//            await ReplyAsync("", embed: eBuilder);

//        }

//        [Command("search", RunMode = RunMode.Async), Alias("s")]
//        [Summary("Searches for cards based on name given")]
//        public async Task CardSearchCommand([Remainder]string search)
//        {

//            if (string.IsNullOrEmpty(search))
//            {

//                await ReplyAsync("Please search for something or else I will search the purple realm.");
//                return;

//            }

//            StringBuilder organizedResults;
//            List<string> searchResults;

//            using (var typingState = Context.Channel.EnterTypingState())
//            {

//                //<card names>
//                searchResults = await _service.SearchCards(search);

//                if (searchResults.Count == 0)
//                {

//                    await ReplyAsync($"Nothing was found with the search of {search}!");
//                    typingState.Dispose();
//                    return;

//                }
//                else if (searchResults.Count > 50)
//                {

//                    await ReplyAsync($"Too many results were returned, please refine your search!");
//                    typingState.Dispose();
//                    return;

//                }

//                var str = $"```There are {searchResults.Count} results based on your search!\n\n";
//                organizedResults = new StringBuilder(str);
//                var counter = 1;

//                foreach (string card in searchResults)
//                {

//                    organizedResults.AppendLine($"{counter}. {card}");
//                    counter++;

//                }

//                organizedResults.Append("\nHit a number to look at that card. Expires in a minute!```");
//                typingState.Dispose();
//            }

//            await ReplyAsync(organizedResults.ToString());

//            IUserMessage response = await WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(60));

//            if (!int.TryParse(response.Content, out int searchNumber)) return;
//            else if (searchNumber > searchResults.Count)
//            {

//                await ReplyAsync($"{Context.User.Mention} Not a valid search!");
//                return;

//            }

//            await CardCommand(searchResults[searchNumber - 1]);

//        }

//        [Command("lsearch", RunMode = RunMode.Async), Alias("ls")]
//        [Summary("A lazy search of all cards that CONTAIN the words entered, it may be not be in any particular order")]
//        public async Task LazySearchCommand([Remainder]string search)
//        {

//            if (string.IsNullOrEmpty(search))
//            {

//                await ReplyAsync("Please don't enter the Shadow Realm as a search.");
//                return;

//            }

//            StringBuilder organizedResults;
//            List<string> searchResults;

//            using (var typingState = Context.Channel.EnterTypingState())
//            {

//                //<card names>
//                searchResults = await _service.LazySearchCards(search);

//                if (searchResults.Count == 0)
//                {

//                    await ReplyAsync($"Nothing was found with the search of {search}!");
//                    typingState.Dispose();
//                    return;

//                }
//                else if (searchResults.Count > 50)
//                {

//                    await ReplyAsync($"Too many results were returned, please refine your search!");
//                    typingState.Dispose();
//                    return;

//                }

//                var str = $"```There are {searchResults.Count} results based on your search!\n\n";
//                organizedResults = new StringBuilder(str);
//                var counter = 1;

//                foreach (string card in searchResults)
//                {

//                    organizedResults.AppendLine($"{counter}. {card}");
//                    counter++;

//                }

//                organizedResults.Append("\nHit a number to look at that card. Expires in a minute!```");

//            }

//            await ReplyAsync(organizedResults.ToString());

//            IUserMessage response = await WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(60));

//            if (!int.TryParse(response.Content, out int searchNumber)) return;
//            else if (searchNumber > searchResults.Count)
//            {

//                await ReplyAsync($"{Context.User.Mention} Not a valid search!");
//                return;

//            }

//            await CardCommand(searchResults[searchNumber - 1]);

//        }

//        [Command("archetype", RunMode = RunMode.Async), Alias("a", "arch")]
//        [Summary("Attempt to search all cards associated with searched archetype")]
//        public async Task ArchetypeSearchCommand([Remainder]string archetype)
//        {

//            if (string.IsNullOrEmpty(archetype))
//            {

//                await ReplyAsync("There is no archetype with a blank name, unless it's Marik's fantasies.");
//                return;

//            }

//            StringBuilder organizedResults;
//            List<string> searchResults;

//            using (var typingState = Context.Channel.EnterTypingState())
//            {

//                searchResults = await _service.ArchetypeSearch(archetype);

//                if (searchResults.Count == 0)
//                {

//                    await ReplyAsync($"Nothing was found with the search of {archetype}!");
//                    typingState.Dispose();
//                    return;

//                }
//                else if (searchResults.Count > 50)
//                {

//                    await ReplyAsync($"Too many results were returned, please refine your search!");
//                    typingState.Dispose();
//                    return;

//                }

//                var str = $"```There are {searchResults.Count} results based on your search!\n\n";
//                organizedResults = new StringBuilder(str);
//                var counter = 1;

//                foreach (string card in searchResults)
//                {

//                    organizedResults.AppendLine($"{counter}. {card}");
//                    counter++;

//                }

//                organizedResults.Append("\nHit a number to look at that card. Expires in a minute!```");

//            }

//            await ReplyAsync(organizedResults.ToString());

//            IUserMessage response = await WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(60));

//            if (!int.TryParse(response.Content, out int searchNumber)) return;
//            else if (searchNumber > searchResults.Count)
//            {

//                await ReplyAsync($"{Context.User.Mention} Not a valid search!");
//                return;

//            }

//            await CardCommand(searchResults[searchNumber - 1]);

//        }

//        [Command("banlist"), Alias("bans")]
//        [Summary("Returns the current banlist")]
//        public async Task BanListCommand(string format = "")
//        {

//            if (string.IsNullOrEmpty(format) || !int.TryParse(format, out int formatInt))
//            {

//                await ReplyAsync("That is not a valid format! ```http\n1 -> OCG\n2 -> TCG ADV\n3 -> TCG TRN```");
//                return;

//            }

//            if (!CacheService.DMChannelCache.TryGetValue(Context.User.Id, out IDMChannel channel))
//            {

//                var dm = await Context.User.CreateDMChannelAsync();
//                CacheService.DMChannelCache.TryAdd(Context.User.Id, dm);
//                channel = dm;

//            }

//            await ReplyAsync("Sending your dms to the purple realm...");

//            switch (formatInt)
//            {

//                case 1:
//                    {

//                        var forbidden = $"```OCG Forbidden\n\n";
//                        _service.OcgBanList.TryGetValue("Forbidden", out List<string> forbiddenList);
//                        forbiddenList.ForEach(card => forbidden += $"{card}\n");
//                        forbidden += "```";

//                        var limited = $"```OCG Limited\n\n";
//                        _service.OcgBanList.TryGetValue("Limited", out List<string> limitedList);
//                        limitedList.ForEach(card => limited += $"{card}\n");
//                        limited += "```";

//                        var semilimited = $"```OCG Semi-Limited\n";
//                        _service.OcgBanList.TryGetValue("Semi-Limited", out List<string> semiList);
//                        semiList.ForEach(card => semilimited += $"{card}\n");
//                        semilimited += "```";

//                        await channel.SendMessageAsync(forbidden);
//                        await channel.SendMessageAsync(limited);
//                        await channel.SendMessageAsync(semilimited);
//                        break;

//                    }
//                case 2:
//                    {

//                        var forbidden = $"```TCG ADV Forbidden\n\n";
//                        _service.OcgBanList.TryGetValue("Forbidden", out List<string> forbiddenList);
//                        forbiddenList.ForEach(card => forbidden += $"{card}\n");
//                        forbidden += "```";

//                        var limited = $"```TCG ADV Limited\n\n";
//                        _service.OcgBanList.TryGetValue("Limited", out List<string> limitedList);
//                        limitedList.ForEach(card => limited += $"{card}\n");
//                        limited += "```";

//                        var semilimited = $"```TCG ADV Semi-Limited\n\n";
//                        _service.OcgBanList.TryGetValue("Semi-Limited", out List<string> semiList);
//                        semiList.ForEach(card => semilimited += $"{card}\n");
//                        semilimited += "```";

//                        await channel.SendMessageAsync(forbidden);
//                        await channel.SendMessageAsync(limited);
//                        await channel.SendMessageAsync(semilimited);
//                        break;

//                    }
//                case 3:
//                    {

//                        var forbidden = $"```TCG Traditional Forbidden\n\n";
//                        _service.OcgBanList.TryGetValue("Forbidden", out List<string> forbiddenList);
//                        forbiddenList.ForEach(card => forbidden += $"{card}\n");
//                        forbidden += "```";

//                        var limited = $"```TCG Traditional Limited\n\n";
//                        _service.OcgBanList.TryGetValue("Limited", out List<string> limitedList);
//                        limitedList.ForEach(card => limited += $"{card}\n");
//                        limited += "```";

//                        var semilimited = $"```TCG Traditional Semi-Limited\n\n";
//                        _service.OcgBanList.TryGetValue("Semi-Limited", out List<string> semiList);
//                        semiList.ForEach(card => semilimited += $"{card}\n");
//                        semilimited += "```";

//                        await channel.SendMessageAsync(forbidden);
//                        await channel.SendMessageAsync(limited);
//                        await channel.SendMessageAsync(semilimited);
//                        break;

//                    }
//                default:
//                    break;

//            }

//        }

//        private string GetFormat(bool tcgOnly, bool ocgOnly)
//        {

//            if (tcgOnly || ocgOnly)
//            {
//                if (ocgOnly) return "OCG";
//                else return "TCG";
//            }
//            else return "TCG/OCG";

//        }

//        private Color WhatColorIsTheCard(YuGiOhCard card)
//        {

//            if (card.Name.Equals("Slifer the Sky Dragon")) return new Color(255, 0, 0);
//            else if (card.Name.Equals("The Winged Dragon of Ra")) return new Color(255, 215, 0);
//            else if (card.Name.Equals("Obelisk the Tormentor")) return new Color(50, 50, 153);

//            if (card.CardType.Equals("Trap")) return new Color(188, 90, 132);
//            else if (card.CardType.Equals("Spell")) return new Color(29, 158, 116);

//            if (card is PendulumMonster) return new Color(175, 219, 205);
//            else if (card is XyzMonster) return new Color(0, 0, 1);
//            else if (card is RegularMonster)
//            {

//                var monster = card as RegularMonster;
//                if (monster.Types.Contains("Synchro")) return new Color(204, 204, 204);
//                else if (monster.Types.Contains("Fusion")) return new Color(160, 134, 183);
//                else if (monster.Types.Contains("Ritual")) return new Color(157, 181, 204);
//                else if (monster.Types.Contains("Effect")) return new Color(174, 121, 66);
//                else if (monster.Types.Contains("Token")) return new Color(192, 192, 192);

//            }

//            return new Color(216, 171, 12);

//        }

//    }
//}
