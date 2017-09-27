using AngleSharp;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MoreLinq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        //private static ConcurrentDictionary<ulong, object> _inProgress = new ConcurrentDictionary<ulong, object>();
        private static ConcurrentDictionary<ulong, object> _inProgress = new ConcurrentDictionary<ulong, object>();

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

            if (_inProgress.TryAdd(Context.Channel.Id, null))
            {

                //var art = await GetArt();

                //Console.WriteLine(art.Value);

                //using (var stream = await _web.GetStream(art.Value).ConfigureAwait(false))
                //{

                //    await UploadAsync(stream, $"{GenObufscatedString()}.png", ":stopwatch: You have **60** seconds to guess what card this art belongs to! Case sensitive!");
                //    await stream.FlushAsync();

                //}

                var passcode = _cache.Passcodes.RandomSubset(1).First();
                Console.WriteLine($"https://raw.githubusercontent.com/shadowfox87/YGOTCGOCGPics323x323/master/{passcode.Key}.png");

                using (var stream = await GetArtGithub(passcode.Key))
                    await UploadAsync(stream, $"{GenObufscatedString()}.png", ":stopwatch: You have **60** seconds to guess what card this art belongs to! Case sensitive!");

                //_criteria.AddCriterion(new GuessCriteria(art.Key));
                _criteria.AddCriterion(new GuessCriteria(passcode.Value));

                var answer = await NextMessageAsync(_criteria, TimeSpan.FromSeconds(60));

                if (answer != null)
                {

                    var author = answer.Author as SocketGuildUser;

                    //await ReplyAsync($":trophy: The winner is **{author.Nickname ?? author.Username}**! The card was `{art.Key}`!");
                    await ReplyAsync($":trophy: The winner is **{author.Nickname ?? author.Username}**! The card was `{passcode.Value}`!");

                }
                else
                    await ReplyAsync($":stop_button: Ran out of time! The card was `{passcode.Value}`!");

                _inProgress.Remove(Context.Channel.Id, out var blarg);

            }
            else
                await ReplyAsync($":game_die: There is a game in progress!");

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

        private Task<Stream> GetArtGithub(string passcode)
        {

            var url = $"https://raw.githubusercontent.com/shadowfox87/YGOTCGOCGPics323x323/master/{passcode}.png";
            return _web.GetStream(url);

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
