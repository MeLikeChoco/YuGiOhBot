using AngleSharp;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MoreLinq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects.Criterion;
using YuGiOhV2.Objects.Deserializers;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    public class Games : CustomBase
    {

        private Cache _cache;
        private Web _web;
        private Random _rand;
        private Criteria<SocketMessage> _criteria;

        private static ConcurrentDictionary<ulong, object> InProgress = new ConcurrentDictionary<ulong, object>();

        public Games(Cache cache, Web web, Random random)
        {

            _cache = cache;
            _rand = random;
            _web = web;
            _criteria = new Criteria<SocketMessage>()
                .AddCriterion(new EnsureSourceChannelCriterion());

        }

        [Command("guess")]
        [RequireContext(ContextType.Guild)]
        [Summary("Starts an image/card guessing game!")]
        public async Task GuessCommand()
        {

            if (!InProgress.ContainsKey(Context.Channel.Id))
                InProgress[Context.Channel.Id] = null;
            else
            {

                await ReplyAsync(":game_die: There is a game in progress!");
                return;

            }

            var art = await GetArt();

            Console.WriteLine(art.Value);

            using (var stream = await _web.GetStream(art.Value).ConfigureAwait(false))
            {

                await UploadAsync(stream, $"{GenObufscatedString()}.png", ":stopwatch: You have **60** seconds to guess what card this art belongs to! Case sensitive!");
                await stream.FlushAsync();

            }

            _criteria.AddCriterion(new GuessCriteria(art.Key));

            var answer = await NextMessageAsync(_criteria, TimeSpan.FromSeconds(60));

            if (answer != null)
            {

                var author = answer.Author as SocketGuildUser;

                await ReplyAsync($":trophy: The winner is **{author.Nickname ?? author.Username}**! The card was `{art.Key}`!");

            }
            else
                await ReplyAsync($":stop_button: Ran out of time! The card was `{art.Key}`!");

            InProgress.Remove(Context.Channel.Id, out var blarg);

        }

        private string GenObufscatedString()
        {

            var str = "";

            for (int i = 0; i < _rand.Next(10, 20); i++)
            {

                var displacement = _rand.Next(0, 26);
                str += (char)('a' + displacement);

            }

            return str;

        }

        private async Task<KeyValuePair<string, string>> GetArt()
        {

            var offset = _rand.Next(0, _cache.FYeahYgoCardArtPosts - 20);
            var response = await _web.GetDeserializedContent<JObject>($"https://api.tumblr.com/v2/blog/fyeahygocardart/posts/photo?api_key={_cache.TumblrKey}&limit=20&offset={offset}");
            var post = response["response"]["posts"].ToObject<JArray>().RandomSubset(1).First().ToObject<YgoCardArtPost>();
            var key = post.Name;
            var value = post.Photos.First().OriginalSize.Url;

            return new KeyValuePair<string, string>(key, value);

        }

    }
}
