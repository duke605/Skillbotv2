using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using SkillBotv2.Extensions;
using unirest_net.http;

namespace SkillBotv2.Command
{
    class CommandPortables : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message m)
        {
            if (!args.Any())
                throw new Exception("Missing required argument");

            if (args.First() == "-?" || args.First() == "--help")
            {
                await m.Channel.SendMessage("`!portables <portable>`");
                return false;
            }

            return Enum.Parse(typeof(Portables), args.FirstOrDefault()?.ToSentenceCase() ?? "Well");
        }

        public async Task Execute(object arguments, Message message)
        {
            var portable = (Portables) arguments;
            var host = "https://sheets.googleapis.com/v4/spreadsheets";
            var sheetId = "16Yp-eLHQtgY05q6WBYA2MDyvQPmZ4Yr3RHYiBCBj2Hc";
            var sheetName = "Home";
            var range = "A16:G18";
            var url = $"{host}/{sheetId}/values/{sheetName}!{range}?key={Secret.GoogleApiKey}";

            // Getting sheet
            HttpResponse<Cells> r = await Unirest.get(url)
                .asJsonAsync<Cells>();

            // Checking if response was ok
            if (r.Code < 200 || r.Code > 299)
                throw new Exception($"Portables spreadsheet could not be accessed. Server returned code {r.Code}.");

            await message.Channel.SendMessage(
                $"**Portable {portable}s**: {r.Body.Locations[$"{portable}s"]}\n" +
                $"**Last Updated**: {r.Body.LastUpdate} ago by {r.Body.UpdatedBy}");
        }

        public class Cells
        {
            public string UpdatedBy { get; set; }
            public string LastUpdate { get; set; }
            public Dictionary<string, string> Locations { get; set; }

            public Cells()
            {
                Locations = new Dictionary<string, string>();
            }

            private string[][] _values;
            public string[][] Values
            {
                get { return _values; }
                set
                {
                    _values = value;

                    // Parsing portable locations
                    _values[0].ForEachWithIndex((i, e) 
                        => Locations[_values[0][i].Trim()] = _values[1][i].Trim()
                            .ToLower()
                            .Replace(" ca", " Combat Academy")
                            .Replace(" ba", " Barbarian Assault")
                            .Replace(" bu", " Burthorpe")
                            .Replace(" cw", " Castle Wars")
                            .Replace(" sp", " Shanty Pass")
                            .Replace(" p", " Prifddinas"));

                    // Getting the person that last updated it
                    UpdatedBy = _values[2][3];

                    // Getting last time updated
                    var timeParts = _values[2][1].Split('@');

                    var first = timeParts[0].Trim().Split('/');
                    var second = timeParts[1].Trim().Split(':');

                    var day = first[0].ToInt();
                    var month = first[1].ToInt();
                    var year = first[2].ToInt();

                    var hour = second[0].ToInt();
                    var min = second[1].ToInt();

                    var diff = DateTime.Now - new DateTime(year, month, day, hour, min, 0, DateTimeKind.Utc).ToLocalTime();

                    // Formatting
                    if (diff.TotalDays >= 1)
                        LastUpdate = diff.TotalDays.Round() + "d";
                    else if (diff.TotalHours >= 1)
                        LastUpdate = diff.TotalHours.Round() + "h";
                    else if (diff.TotalMinutes >= 1)
                        LastUpdate = diff.TotalMinutes.Round() + "m";
                    else
                        LastUpdate = diff.TotalSeconds.Round() + "s";
                }
            }

        }

        public enum Portables
        {
            Crafter,
            Forge,
            Fletcher,
            Brazier,
            Sawmill,
            Range,
            Well
        }
    }
}
