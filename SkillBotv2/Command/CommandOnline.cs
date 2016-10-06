using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using RestSharp.Extensions;
using SkillBotv2.Extensions;
using unirest_net.http;

namespace SkillBotv2.Command
{
    class CommandOnline : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            return null;
        }

        public async Task Execute(object a, Message m)
        {
            var url = "http://www.runescape.com/player_count.js?varname=iPlayerCount&callback=jQuery000000000000000_0000000000&_=0";

            HttpResponse<string> r = await Unirest.get(url)
                .asStringAsync();

            // Checking if the request was successful
            if (r.Code < 200 || r.Code > 299)
            {
                await m.Channel.SendMessage($"Error code **{r.Code}** was returned from the server.");
                return;
            }

            // Getting number
            var jsonReadable = Regex.Match(r.Body, @"jQuery000000000000000_0000000000\((\d+?)\)").Groups[1].Value.ToInt();
            await m.Channel.SendMessage($"**{jsonReadable.ToString("#,##0")}** players online");
        }
    }
}
