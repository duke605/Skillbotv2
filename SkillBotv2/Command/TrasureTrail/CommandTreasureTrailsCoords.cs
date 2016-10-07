using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using SkillBotv2.Exceptions;
using unirest_net.http;

namespace SkillBotv2.Command.TrasureTrail
{
    partial class CommandTreasureTrails
    {
        private async Task<object> ParseCoordArguments(string[] args, Message m)
        {
            var hPattern = @"(?:^|\s)(\d+?)\.(\d+?)([WwEe])";
            var vPattern = @"(?:^|\s)(\d+?)\.(\d+?)([SsNn])";
            var input = string.Join(" ", args);
            Match hMatch;
            Match vMatch;

            // Matching horizontal
            if (!(hMatch = Regex.Match(input, hPattern)).Success)
                throw new ControlledException("Horizontal degrees are formatted incorrectly. ##.##W|E");

            // Matching vertical
            if (!(vMatch = Regex.Match(input, vPattern)).Success)
                throw new ControlledException("Vertical degrees are formatted incorrectly. ##.##N|S");

            // Parsing
            return new CoordArguments
            {
                HDegree = int.Parse(hMatch.Groups[1].Value),
                HMinute = int.Parse(hMatch.Groups[2].Value),
                HDirection = hMatch.Groups[3].Value[0],

                VDegree = int.Parse(vMatch.Groups[1].Value),
                VMinute = int.Parse(vMatch.Groups[2].Value),
                VDirection = vMatch.Groups[3].Value[0]
            };
        }

        private async Task LookupCoords(CoordArguments a, Message message)
        {
            var vsh = a.VSortHand;
            var hsh = a.HSortHand;
            var bsh = $"{vsh},{hsh}";

            var host = "https://sheets.googleapis.com/v4/spreadsheets";
            var sheetId = "1jhzsghQSaXCEwymch5aRwT7AXIDXKaL1gHGNdZHiA4o";
            var sheetName = "Coordinates";
            var range = "A2:F71";
            var url = $"{host}/{sheetId}/values/{sheetName}!{range}?key={Secret.GoogleApiKey}";

            var r = await Unirest.get(url)
                .asJsonAsync<SheetResponse>();

            var coords = r.Body;

            // Looking for our coordinates
            var c1 = coords.Values.FirstOrDefault(c => c[1] == bsh);
            if (c1 == null)
                throw new ControlledException("No clue with those coordinates could be found.");

            await message.Channel.SendMessage(
                $"Location found for coordinates `{vsh}` `{hsh}`:\n" +
                $"**Requirements**: {c1[2]}\n" +
                $"**Fight**: {c1[3]}\n" +
                $"**Notes**: ```{c1[5]}```" +
                $"{c1[4]}");
        }

        public struct CoordArguments
        {
            public int HDegree { get; set; }
            public int HMinute { get; set; }
            public int VDegree { get; set; }
            public int VMinute { get; set; }
            public char HDirection { get; set; }
            public char VDirection { get; set; }
            public string HSortHand => $"{HDegree.ToString("00")}.{HMinute.ToString("00")}{HDirection}".ToUpper();
            public string VSortHand => $"{VDegree.ToString("00")}.{VMinute.ToString("00")}{VDirection}".ToUpper();
            public string BSortHand => $"{VSortHand},{HSortHand}";
        }
    }
}
