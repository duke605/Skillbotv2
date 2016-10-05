using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Mono.Options;
using SkillBotv2.Extensions;
using SkillBotv2.Util;

namespace SkillBotv2.Command.Item
{
    partial class CommandItem
    {
        private async Task<object> ParseAddArguments(string[] arguments, Message message)
        {
            var args = new AddArguments();
            var optionSet = new OptionSet();
            var help = false;
            string error = null;

            optionSet.Add("?", "help", h => help = h != null);
            args.Items = optionSet.Parse(arguments).Select(s => s.ToSentenceCase()).ToList();

            // Checking if at least one item is supplied
            if (!args.Items.Any())
                error = "At least one item id or name must be given.";

            // Showing help
            if (help || error != null)
            {
                string m = "```";

                // Adding error
                if (error != null)
                    m += $"Error:\n{error}\n\n";

                m += "Usage:\n" +
                     "!item add <item-id | item-name>```";

                await message.Channel.SendMessage(m);
                return false;
            }

            return args;
        }

        private async Task AddItem(AddArguments args, Message m)
        {
            using (Database db = new Database())
            {
                item item;
                var message = "";

                // Removing item already in the DB
                for (var i = args.Items.Count() - 1; i >= 0; i--)
                {
                    string itemReadable = args.Items.ElementAt(i);
                    int temp;

                    // Checking if the string is an int
                    if (int.TryParse(itemReadable, out temp))
                        item = await db.items.FindAsync(temp);
                    else
                        item = await db.items.FirstOrDefaultAsync(it => it.Name == itemReadable);

                    // Checking if item is in DB
                    if (item != null)
                    {
                        message += $"**{item.Name}** already exists in the database.\n";
                        args.Items.Remove(itemReadable);
                    }
                }

                var tasks = new Task<item>[args.Items.Count];

                // Fetching item data
                for (var i = 0; i < args.Items.Count; i++)
                    tasks[i] = RSUtil.GetItemForDynamic(args.Items.ElementAt(i));

                // Waiting for all items to complete
                await Task.Run(() => { try { Task.WaitAll(tasks); } catch (Exception) { } });

                // Adding items
                foreach (var t in tasks)
                {
                    // Skipping if task errored
                    if (t.Status != TaskStatus.RanToCompletion)
                        continue;

                    item = t.Result;
                    db.items.Add(item);
                    message += $"**{item.Name}** has added to the database.\n";

                    // Removing completed from array
                    args.Items.Remove(item.Name);
                    args.Items.Remove(item.Id.ToString());
                }

                // Outputting message for those that errors
                foreach (var s in args.Items)
                    message += $"**{s}** could not be added to the database.\n";

                // Saving
                if (await db.SaveChangesAsync() < 0)
                    message += "There was an error saving the items to the database.\n";

                await m.Channel.SendMessage(message.Substring(0, message.Length - 1));
            }
        }

        private struct AddArguments
        {
            public List<string> Items { get; set; }
        }
    }
}
