using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp.Extensions;
using SkillBotv2.Exceptions;
using Tweetinvi;
using Message = Discord.Message;

namespace SkillBotv2.Command
{
    class CommandVos : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            return string.Empty;
        }

        public async Task Execute(object arguments, Message m)
        {
            var tweets = await TimelineAsync.GetUserTimeline(3429940841, 10);
            var vosRegex = @"^The Voice of Seren is now active in the (.+) and (.+) districts at (\d+):(\d+) UTC.";

            // Checking if the tweets could ne retrieved
            if (tweets == null)
                throw new ControlledException("Tweets from @JagexClock could not be retrieved.");
            
            // Finding VOS tweets
            tweets = tweets.Where(t => t.Text.Matches(vosRegex)).Take(3);

            // Getting districts
            var activeMatch = Regex.Match(tweets.ElementAt(0).Text, vosRegex).Groups;
            var cooldownMatchNew = Regex.Match(tweets.ElementAt(1).Text, vosRegex).Groups;
            var cooldownMatchOld = Regex.Match(tweets.ElementAt(2).Text, vosRegex).Groups;

            await m.Channel.SendMessage(
                $"**Active districts**: **{activeMatch[1]}** and **{activeMatch[2]}**.\n" +
                $"**Cooldown districts**: **{cooldownMatchNew[1]}**, **{cooldownMatchNew[2]}**, **{cooldownMatchOld[1]}** and **{cooldownMatchOld[2]}**."
            );
        }
    }
}
