using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Fclp;
using Fclp.Internals.Extensions;
using SkillBotv2.Entities;
using SkillBotv2.Extensions;
using SkillBotv2.Util;

namespace SkillBotv2.Command.Recipe
{
    partial class CommandRecipe
    {
        private async Task<object> ParseSearchArguments(string[] args, Message message)
        {
            SearchArguments a = new SearchArguments { Orders = new List<SearchArguments.Order>() };
            FluentCommandLineParser parser = new FluentCommandLineParser();
            string usernme = null;
            bool helpCalled = false;
            
            parser.Setup<AddArguments.Skill>('s', "skill")
                .Callback(s => a.Skill = s)
                .Required();

            parser.Setup<List<string>>('u', "username")
                .Required()
                .Callback(u => usernme = string.Join(" ", u));

            parser.Setup<bool>('i', "image")
                .Callback(i => a.Image = i);

            parser.Setup<int>('p', "page")
                .SetDefault(1)
                .Callback(p => a.Page = p);

            parser.Setup<int>('l', "limit")
                .SetDefault(10)
                .Callback(l => a.Limit = l);

            parser.Setup<int>('L', "level")
                .Required()
                .Callback(l => a.Level = l);

            parser.Setup<List<SearchArguments.Order>>('o', "order")
                .SetDefault(new List<SearchArguments.Order> {SearchArguments.Order.Name})
                .Callback(o => a.Orders = o);

            parser.Setup<bool>('?', "help")
                .Callback(h => helpCalled = h);

            var r = parser.Parse(args);

            if (r.HasErrors || helpCalled)
            {
                if (!helpCalled)
                {
                    await message.Channel.SendMessage($"```{r.ErrorText}```");
                    return false;
                }

                await message.Channel.SendMessage(
                    "**Usage:**\n" +
                    "```!recipe search <-s|skill> <-u|username> <-L|level> [-q|query] [-p|page] [-l|limit] [-o|order]```\n" +
                    "**Options:**\n" +
                    "`-s`, `--skill` **REQUIRED**\n" +
                    "The skill you wish to get methods of training for.\n" +
                    "\n" +
                    "`-u`, `--username` **REQUIRED**\n" +
                    "Your in game name.\n" +
                    "\n" +
                    "`-L`, `--level` **REQUIRED**\n" +
                    "The level you wish to obtain\n" +
                    "\n" +
                    "`-p`, `--page` *OPTIONAL*\n" +
                    "The page of results to show.\n" +
                    "\n" +
                    "`-l`, `--limit` *OPTIONAL*\n" +
                    "Results per page.\n" +
                    "\n" +
                    "`-o`, `--order` *OPTIONAL*\n" +
                    "How the results will be ordered. Values can be Cheap, Expensive, Slow, Fast, Name, Level");
                return false;
            }

            // Getting username
            a.RS3Stats = await RSUtil.GetStatsForUser(usernme);

            return a;
        }

        private async Task SearchRecipe(SearchArguments a, Message message)
        {
            using (var db = new Database())
            {
                Stat stat = a.RS3Stats.GetStatForName(a.Skill.ToString());
                int levelDiff = RSUtil.ExpBetweenLevels(stat, a.Level);
                var dbLookup = new Stopwatch();
                var queryBuild = new Stopwatch();
                var upload = new Stopwatch();
                var convertText = new Stopwatch();

                queryBuild.Start();
                var tempQuery = db.recipes
                    .Where(r => r.Skill == (int) a.Skill)
                    .Where(r => r.Level <= stat.Level)
                    .Select(r => new Entity
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Level = r.Level,
                        Exp = r.Exp,
                        ExpH = r.Exp * r.Units,
                        Number = (int) Math.Ceiling(levelDiff / r.Exp),
                        Cost = (db.outputs.Where(o => o.RecipeId == r.Id).Sum(o => o.Quantity * o.item.Price)
                               - (db.inputs.Where(i => i.RecipeId == r.Id).Sum(i => i.Quantity * i.item.Price) + r.Extra))
                               * Math.Ceiling(levelDiff/r.Exp),
                        Time = Math.Ceiling(levelDiff/r.Exp)/r.Units
                    });

                var firstOrder = a.Orders.First();
                IOrderedQueryable<Entity> query = null; 
                
                // Doing first sort
                switch (firstOrder)
                {
                    case SearchArguments.Order.Expensive:
                        query = tempQuery.OrderBy(r => r.Cost);
                        break;
                    case SearchArguments.Order.Fast:
                        query = tempQuery.OrderBy(r => r.Time);
                        break;
                    case SearchArguments.Order.Cheap:
                        query = tempQuery.OrderByDescending(r => r.Cost);
                        break;
                    case SearchArguments.Order.Slow:
                        query = tempQuery.OrderByDescending(r => r.Time);
                        break;
                    case SearchArguments.Order.Name:
                        query = tempQuery.OrderBy(r => r.Name);
                        break;
                    case SearchArguments.Order.Level:
                        query = tempQuery.OrderBy(r => r.Level);
                        break;
                }

                // Consecutive sort
                for (var i = 1; i < a.Orders.Count; i++)
                {
                    switch (firstOrder)
                    {
                        case SearchArguments.Order.Expensive:
                            query = query.ThenByDescending(r => r.Cost);
                            break;
                        case SearchArguments.Order.Fast:
                            query = query.ThenBy(r => r.Time);
                            break;
                        case SearchArguments.Order.Cheap:
                            query = query.ThenBy(r => r.Cost);
                            break;
                        case SearchArguments.Order.Slow:
                            query = query.ThenByDescending(r => r.Time);
                            break;
                        case SearchArguments.Order.Name:
                            query = query.ThenBy(r => r.Name);
                            break;
                        case SearchArguments.Order.Level:
                            query = query.ThenBy(r => r.Level);
                            break;
                    }
                }

                // Stopping query builder timer
                queryBuild.Stop();

                var pages = Math.Ceiling((decimal) await query.LongCountAsync() / a.Limit);

                // Starting db lookup
                dbLookup.Start();

                var recipes = await query.Skip((a.Page - 1)*a.Limit)
                    .Take(a.Limit)
                    .ToListAsync();
               
                // END OF DB LOOKUP
                dbLookup.Stop();

                if (!recipes.Any())
                {
                    await message.Channel.SendMessage("No recipes found.");
                    return;
                }
                
                // Formatting
                Table table = new Table();
                table.SetTitle($"{a.Skill} Recipes ({a.Page}/{pages})");
                table.SetHeadings("Id", "Name", "Level", "Xp", "Xp/H", "Number", "Cost", "Time");
                recipes.ForEach(r => table.AddRow(
                    r.Id
                    , r.Name
                    , new Table.Column(r.Level, Table.Column.Alignment.Right)
                    , new Table.Column(r.Exp.ToString("#,##0.##"), Table.Column.Alignment.Right)
                    , new Table.Column(r.ExpH.ToString("#,##0.##"), Table.Column.Alignment.Right)
                    , new Table.Column(r.Number.ToString("#,##0"), Table.Column.Alignment.Right)
                    , new Table.Column(r.Cost.ToString("#,##0"), Table.Column.Alignment.Right)
                    , new Table.Column(r.Time.ToFormattedTime(), Table.Column.Alignment.Right)));

                string @out = $"{table}";

                // Outputing image
                if (a.Image)
                {
                    var img = convertText.TimeTask(() => ImageUtil.ToImage(@out));
                    var link = await upload.TimeTaskAsync(async () => await ImageUtil.PostToImgur(img));
                    await message.Channel.SendMessage(link + "\n" +
                                                      $"**Build Query**: {dbLookup.ElapsedMilliseconds}ms | " +
                                                      $"**Query DB**: {queryBuild.Elapsed.Milliseconds}ms | " +
                                                      $"**Convert Text**: {convertText.ElapsedMilliseconds}ms | " +
                                                      $"**Upload Image**: {upload.ElapsedMilliseconds}ms");
                }
                else
                    await message.Channel.SendMessage($"**Build Query**: {dbLookup.ElapsedMilliseconds}ms | " +
                                                      $"**Query DB**: {queryBuild.Elapsed.Milliseconds}ms" +
                                                      $"```{@out}```");
            }
        }

        public class Entity
        {
            public decimal Id { get; set; }
            public int Number { get; set; }
            public string Name { get; set; }
            public sbyte Level { get; set; }
            public double Exp { get; set; }
            public double ExpH { get; set; }
            public double Cost { get; set; }
            public double Time { get; set; }
        }

        public struct SearchArguments
        {
            public AddArguments.Skill Skill { get; set; }
            public RS3Stats RS3Stats { get; set; }
            public List<Order> Orders { get; set; }
            public int Limit { get; set; }
            public int Page { get; set; }
            public int Level { get; set; }
            public bool Image { get; set; }
            
            public enum Order : int
            {
                Cheap,
                Expensive,
                Slow,
                Fast,
                Level,
                Name
            }
        }
    }
}
