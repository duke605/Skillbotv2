using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Discord;
using Fclp;
using Fclp.Internals.Extensions;
using unirest_net.http;

namespace SkillBotv2.Command
{
    class CommandNews : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            var a = new Arguments();
            var parser = new FluentCommandLineParser();

            parser.Setup<List<string>>('f', "filter")
                .SetDefault(new List<string>())
                .Callback(f => f.ForEach(c =>
                {
                    a.Filters.Add(Arguments.Category.Match(c)); 
                }));

            parser.Setup<List<string>>('o', "only")
                .SetDefault(new List<string>())
                .Callback(o => o.ForEach(c =>
                {
                    a.Only.Add(Arguments.Category.Match(c));
                }));

            parser.Parse(args);

            // Checking if both options were used
            if (!a.Filters.IsNullOrEmpty() && !a.Only.IsNullOrEmpty())
            {
                await message.Channel.SendMessage("--only and --filter may not be used together.");
                return false;
            }

            return a;
        }

        public async Task Execute(object arguments, Message message)
        {
            var a = (Arguments) arguments;
            var r = await Unirest.get("http://services.runescape.com/m=news/latest_news.rss")
                .asStringAsync();

            // Parsing XML
            var nf = new NewsFeed();
            var doc = XDocument.Parse(r.Body);
            var root = doc.Root.Element("channel");

            nf.Title = root.Element("title").Value;
            nf.Language = root.Element("language").Value;
            nf.Description = root.Element("description").Value;
            nf.Link = root.Element("link").Value;
            nf.Docs = root.Element("docs").Value;
            nf.LastBuildDate = Convert.ToDateTime(root.Element("lastBuildDate").Value);

            // Parsing items
            var items = new List<NewsFeed.Item>();
            foreach (var i in root.Elements("item"))
            {
                var cat = i.Element("category")?.Value;

                // Ignoring
                if (a.Filters.Any(f => f.Name.ToLower() == cat.ToLower()))
                    continue;

                if (a.Only.Any(o => o.Name.ToLower() != cat.ToLower()))
                    continue;

                var item = new NewsFeed.Item();

                item.Title = i.Element("title").Value;
                item.Description = i.Element("description").Value.Trim('\n', '\t', ' ');
                item.Category = cat;
                item.Link = i.Element("link").Value;
                item.PubDate = Convert.ToDateTime(i.Element("pubDate").Value);

                items.Add(item);
            }

            nf.Items = items;

            var messages = new List<string>();
            var m = "";

            foreach (var item in nf.Items)
            {
                m += $"**{item.Title}**\n" +
                     $"{item.Category} | {item.PubDate.ToString("d MMMM yyyy")}\n" + 
                     $"<{item.Link}>\n" +
                     $"```{item.Description}```\n";

                // Ending message
                if (m.Length < 2000 - 400) continue;

                messages.Add(m);
                m = "";
            }

            // Adding message to messages
            messages.Add(m);

            for (var i = 0; i < messages.Count; i++)
            {
                m = i == messages.Count - 1
                    ? messages.ElementAt(i).TrimEnd('\n')
                    : messages.ElementAt(i);

                await message.User.SendMessage(m);
            }
        }

        public class Arguments
        {
            public List<Category> Filters { get; set; }
            public List<Category> Only { get; set; }

            public Arguments()
            {
                Filters = new List<Category>();
                Only = new List<Category>();
            }

            public class Category
            {
                public static readonly Category GameUpdates = new Category("Game Updates News", "gu", "Game Updates", "gun");
                public static readonly Category FutureUpdates = new Category("Future Updates", "fu");
                public static readonly Category TreasureHunter = new Category("Treasure Hunter", "th");
                public static readonly Category Community = new Category("Community", "c");
                public static readonly Category BehindTheScenes = new Category("Behind the Scenes News", "bts", "btsn");
                public static readonly Category SolomonsStore = new Category("Solomon's Store", "store", "ss");
            
                public string Name { get; }
                public string[] Aliases { get; }

                public Category(params string[] aliases)
                {
                    Name = aliases[0];
                    Aliases = aliases;
                }

                /// <summary>
                /// Matches input to a category
                /// </summary>
                /// <param name="input">The input to match against</param>
                /// <returns>The match</returns>
                public static Category Match(string input)
                {
                    // Checking if input is game update
                    if (GameUpdates.Aliases.Any(c => c.ToLower() == input))
                        return GameUpdates;

                    // Checking if input is future update
                    if (FutureUpdates.Aliases.Any(c => c.ToLower() == input))
                        return FutureUpdates;

                    // Checking if input is TH
                    if (TreasureHunter.Aliases.Any(c => c.ToLower() == input))
                        return TreasureHunter;

                    // Checking if input is community
                    if (Community.Aliases.Any(c => c.ToLower() == input))
                        return Community;

                    // Checking if input is game Behind The Scenes
                    if (BehindTheScenes.Aliases.Any(c => c.ToLower() == input))
                        return BehindTheScenes;

                    throw new Exception($"Could not match category to string \"{input}\"");
                }
            }
        }

        public struct NewsFeed
        {
            public string Language { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public string Docs { get; set; }
            public DateTime LastBuildDate { get; set; }
            public IEnumerable<Item> Items { get; set; }

            public struct Item
            {
                public string Title { get; set; }
                public string Description { get; set; }
                public string Category { get; set; }
                public string Link { get; set; }
                public DateTime PubDate { get; set; }
            }
        }
    }
}
