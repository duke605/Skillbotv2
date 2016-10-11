using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Mono.Options;
using SkillBotv2.Entities;
using SkillBotv2.Util;
using Tweetinvi.Core.Extensions;

namespace SkillBotv2.Command
{
    class CommandStats : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            var a = new Arguments();
            var optionSet = new OptionSet();

            optionSet.Add("i|image", i => a.Image = i != null);
            optionSet.Add("o|oldschool", o => a.OSRS = o != null);
            a.Username = string.Join(" ", optionSet.Parse(args));

            return a;
        }

        public async Task Execute(object arguments, Message message)
        {
            var a = (Arguments) arguments;
            var table = new Table();
            var temp = a.OSRS ? "OLDSCHOOL" : "RS3";
            string m;

            // Getting user's stats
            var stats = a.OSRS
                ? (object) await OSRSUtil.GetStatsForUser(a.Username)
                : (object) await RSUtil.GetStatsForUser(a.Username);

            // Table making
            table.SetTitle($"VIEWING {temp} STATS FOR {a.Username.ToUpper()}");
            table.SetHeadings("Skill", "Level", "Experience", "Rank");
            stats.GetType().GetProperties().ForEach(p =>
            {
                var stat = (Stat) p.GetMethod.Invoke(stats, null);

                table.AddRow(
                    p.Name, 
                    new Table.Column(stat.Level, Table.Column.Alignment.Right),
                    new Table.Column(stat.Exp.ToString("#,##0"), Table.Column.Alignment.Right),
                    new Table.Column(stat.Rank.ToString("#,##0"), Table.Column.Alignment.Right));
            });
            
            // Uploading image
            if (a.Image)
                m = await ImageUtil.PostToImgur(ImageUtil.ToImage(table.ToString()));
            else
                m = $"```{table}```";

            await message.Channel.SendMessage(m);
        }

        public struct Arguments
        {
            public string Username { get; set; }
            public bool Image { get; set; }
            public bool OSRS { get; set; }
        }
    }
}
