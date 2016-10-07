using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using SkillBotv2.Exceptions;
using unirest_net.http;

namespace SkillBotv2.Command.TrasureTrail
{
    partial class CommandTreasureTrails
    {
        private async Task<object> ParseAnagramArguments(string[] args, Message m)
        {
            return new AnagramArguments
            {
                Anagram = string.Join(" ", args)
            };
        }

        private async Task LookupAnagram(AnagramArguments a, Message message)
        {
            var host = "https://sheets.googleapis.com/v4/spreadsheets";
            var sheetId = "1jhzsghQSaXCEwymch5aRwT7AXIDXKaL1gHGNdZHiA4o";
            var sheetName = "Anagrams";
            var range = "A2:D47";
            var url = $"{host}/{sheetId}/values/{sheetName}!{range}?key={Secret.GoogleApiKey}";

            var r = await Unirest.get(url)
                .asJsonAsync<SheetResponse>();

            var anagrams = r.Body;

            // Looking for our coordinates
            var c1 = anagrams.Values.FirstOrDefault(c => c[0].ToLower() == a.Anagram.ToLower());
            if (c1 == null)
                throw new ControlledException("No clue for that anagram could be found.");

            await message.Channel.SendMessage(
                $"Location found for anagram **{a.Anagram}**:\n" +
                $"**Solution**: {c1[1]}\n" +
                $"**Location**: {c1[2]}\n" +
                $"**Challenge answer**: {c1[3]}");
        }

        public struct AnagramArguments
        {
            public string Anagram { get; set; }
        }
    }
}
