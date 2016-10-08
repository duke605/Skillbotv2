using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fclp;
using RestSharp.Extensions;
using SkillBotv2.Exceptions;
using SkillBotv2.Extensions;
using Tweetinvi;
using Tweetinvi.Models;
using Message = Discord.Message;

namespace SkillBotv2.Command
{
    class CommandVos : ICommand
    {
        private const string VosRegex = @"^The Voice of Seren is now active in the (.+) and (.+) districts at (\d+):(\d+) UTC.";

        public async Task<object> ParseArguments(string[] args, Message message)
        {
            var force = 0;
            var parser = new FluentCommandLineParser();

            parser.Setup<bool>('f', "force")
                .SetDefault(false)
                .Callback(f => force = f ? 1 : 0);

            parser.Parse(args);

            return force;
        }

        public async Task Execute(object arguments, Message m)
        {
            var force = (int) arguments;
            var tweets = await GetVosTweets(force == 1);
            var districts = new [] {
                "Amlodd",
                "Cadarn",
                "Crwys",
                "Hefin",
                "Iorwerth",
                "Ithell",
                "Meilyr",
                "Trahaearn"
            };
            
            // Getting districts
            var activeMatch = Regex.Match(tweets.ElementAt(0).Text, VosRegex).Groups;
            var previous = Regex.Match(tweets.ElementAt(1).Text, VosRegex).Groups;
            var next = districts.Except(new[]
            {
                activeMatch[1].Value,
                activeMatch[2].Value,
                previous[1].Value,
                previous[2].Value
            })
            .Select(s => $"**{s}**");
            
            await m.Channel.SendMessage(
                $"**Active districts**: **{activeMatch[1]}** and **{activeMatch[2]}**.\n" +
                $"**Next districts**: {string.Join(", ", next).ReplaceLast(",", " and")}\n" +
                $"**Previous districts**: **{previous[1]}** and **{previous[2]}**"
            );
        }

        /// <summary>
        /// Gets the VoS tweets from the memory cache or twitter api
        /// </summary>
        /// <param name="ignoreCache">Whether or not to use the cache</param>
        /// <returns>VoS tweets</returns>
        private async Task<IEnumerable<ITweet>> GetVosTweets(bool ignoreCache = false)
        {
            if (!ignoreCache && MemoryCache.Default.Get("vosTweets") != null)
                return (IEnumerable<ITweet>) MemoryCache.Default.Get("vosTweets");

            var tweets = await TimelineAsync.GetUserTimeline(3429940841, 10);

            // Checking if request was successful
            if (tweets == null)
                throw new ControlledException("Tweets from @JagexClock could not be retrieved.");

            // Getting only vos tweets
            tweets = tweets.Where(t => t.Text.Matches(VosRegex));
            
            // Getting invalidation time
            DateTime invalTime = DateTime.Today.AddHours(DateTime.Now.TimeOfDay.TotalHours.Ceiling());

            // Storing tweets
            MemoryCache.Default.Set("vosTweets", tweets, new DateTimeOffset(invalTime));

            return tweets;
        }
    }
}
