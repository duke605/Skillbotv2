using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Mono.Options;
using SkillBotv2.Extensions;
using SkillBotv2.Util;

namespace SkillBotv2.Command.Item
{
    partial class CommandItem
    {
        private async Task<object> ParseUpdateArguments(string[] arguments, Message message)
        {
            var args = new UpdateArguments();
            var optionSet = new OptionSet();
            var help = false;
            string error = null;

            optionSet.Add("?|help", h => help = h != null);
            optionSet.Add("a|all", b => args.All = b != null);
            args.Items = optionSet.Parse(arguments).Select(s => s.ToSentenceCase()).ToList();
            
            // Showing help
            if (help || error != null)
            {
                string m = "```";

                // Adding error
                if (error != null)
                    m += $"Error:\n{error}\n\n";

                m += "Usage:\n" +
                     "!item update <item-id | item-name> [--all]\n" +
                     "\n" +
                     "Options:\n" +
                     "-a, --all Updates all items.```";

                await message.Channel.SendMessage(m);
                return false;
            }

            return args;
        }

        private async Task UpdateItem(UpdateArguments args, Message message)
        {
            using (Database db = new Database())
            {
                // Excluding items not in the db
                decimal temp;
                var names = args.Items.Where(i => !decimal.TryParse(i, out temp)).ToList();
                var ids = args.Items.Where(i => decimal.TryParse(i, out temp)).Select(i => decimal.Parse(i)).ToList();
                var count = 0;
                var total = 0;
                var updated = 0;

                // Updating
                do
                {
                    // Getting item batch
                    var items = await db.items.Where(i => args.All || ids.Contains(i.Id) || names.Contains(i.Name))
                        .Where(i => i.Id != 0)
                        .OrderBy(i => i.Name)
                        .Skip(count++ * 100)
                        .Take(100)
                        .ToListAsync();

                    total += items.Count;

                    // Breaking if nothing returned
                    if (items.Count <= 0)
                        break;

                    // Creating task array
                    Task<item>[] tasks = new Task<item>[items.Count];


                    // Looping through items and starting the task
                    var j = 0;
                    foreach (item item in items)
                        tasks[j++] = RSUtil.GetItemForName(item.Name);

                    // Waiting for tasks to complete
                    await Task.Run(() =>
                    {
                        try
                        {
                            Task.WaitAll(tasks);
                        }
                        catch (Exception)
                        {
                        }
                    });

                    // Pulling items from completed tasks
                    foreach (Task<item> task in tasks)
                    {
                        // Checking if task completed successfully
                        if (task.Status != TaskStatus.RanToCompletion)
                            continue;

                        item item = task.Result;
                        item.Name = item.Name.Replace(@"\", "");
                        db.items.AddOrUpdate(task.Result);
                    }
                    
                    // Saving
                    if ((updated += await db.SaveChangesAsync()) < 0)
                        await message.Channel.SendMessage("There was an error updating some item prices.");
                } while (true);

                
                await message.Channel.SendMessage($"**{updated}**/**{total}** items updated.");
            }
        }

        private struct UpdateArguments
        {
            public bool All { get; set; }
            public List<string> Items { get; set; }
        }
    }
}
