using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOh.Bot.Extensions;
using YuGiOh.Bot.Services;

namespace YuGiOh.Bot.Modules.Commands
{
    public class Wikia : CustomBase
    {

        public Web Web { get; set; }

        [Command("wikia")]
        [Summary("Search for stuff on the yugioh wikia")]
        public async Task WikiaCommand([Remainder]string search)
        {

            var dom = await Web.GetDom($"http://yugioh.wikia.com/wiki/Special:Search?query={search}");
            var results = dom.GetElementsByClassName("unified-search__results").FirstOrDefault();

            if (results is not null)
            {

                var children = results.Children.Where(element => !element.ClassList.Contains("video-addon-results"));
                var count = dom.GetElementsByClassName("unified-search__results__count").First();
                var limit = children.Count() >= 10 ? 10 : children.Count();
                var author = new EmbedAuthorBuilder()
                    .WithIconUrl("https://cdn3.iconfinder.com/data/icons/7-millennium-items/512/Milennium_Puzzle_Icon_Colored-512.png")
                    .WithName("YuGiOh Wikia");

                var footer = new EmbedFooterBuilder()
                    .WithText(count.TextContent);

                var body = new EmbedBuilder()
                    .WithRandomColor()
                    .WithAuthor(author)
                    .WithFooter(footer);

                var builder = new StringBuilder();

                for(int i = 1; i <= limit; i++)
                {
                    
                    var topic = children.ElementAt(i - 1)   .GetElementsByTagName("a").First();
                    var title = topic.TextContent;
                    var link = topic.GetAttribute("href").Replace("(", "%28").Replace(")", "%29");

                    //discord keeps cutting off the parentheses at the end of links, cause it thinks the links end there
                    builder.Append("**").Append(i).Append("**. [").Append(title).Append("](").Append(link).AppendLine(")");

                }

                body.WithDescription(builder.ToString());
                await SendEmbedAsync(body);

            }
            else
                await NoResultError("search results", search);

        }

    }
}
