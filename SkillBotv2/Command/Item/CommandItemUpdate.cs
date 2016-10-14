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
using Tweetinvi.Core.Extensions;

namespace SkillBotv2.Command.Item
{
    partial class CommandItem
    {
        private async Task<object> ParseUpdateArguments(string[] arguments, Message message)
        {
            var args = new UpdateArguments();
            var optionSet = new OptionSet();
            
            optionSet.Parse(arguments);

            return args;
        }

        private async Task UpdateItem(UpdateArguments args, Message message)
        {
            var m = await message.Channel.SendMessage("Updating...");

            using (var db = new Database())
            {
                // Excluding items not in the db
                var updated = 0;

                var runeday = await RSUtil.GetRuneday();
                var total = await db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) " +
                        "FROM items " +
                        "WHERE Id <> 0 " +
                        "AND UpdatedAtRD <> @p0", runeday)
                        .FirstAsync();

                db.Database.Log = Console.WriteLine;

                for (var i = 0;;i++)
                {
                    // Getting item batch
                    var items = await db.Database.SqlQuery<item>(
                        "SELECT * " +
                        "FROM items " +
                        "WHERE Id <> 0 " +
                        "AND UpdatedAtRD <> @p0 " +
                        "LIMIT @p1, 50"
                        , runeday
                        , i * 50)
                        .ToListAsync();
                    
                    if (items.Count <= 0)
                        break;

                    var tasks = new Task<item>[items.Count];

                    // Starting tasks
                    items.ForEachWithIndex((index, item) 
                        => tasks[index] = RSUtil.GetItemForId((int)item.Id, item.Name, runeday));
                    
                    // Waiting for tasks to complete
                    await Task.Run(() =>
                    {
                        try { Task.WaitAll(tasks); } catch(Exception) {}
                    });

                    var updateQuery = "UPDATE items SET Price = (CASE";

                    var predicatedTasks = tasks.Where(t => t.Status == TaskStatus.RanToCompletion);
                    predicatedTasks.ForEach(t => updateQuery += $" WHEN Id = {t.Result.Id} THEN {t.Result.Price}");

                    updateQuery += " ELSE Price END), UpdatedAtRD = @p0, UpdatedAt = @p1 WHERE Id In " +
                                   $"({string.Join(",", predicatedTasks.Select(t => t.Result.Id))});";

                    updated += db.Database.ExecuteSqlCommand(updateQuery
                        , runeday
                        , DateTime.Now);

                    await m.Edit($"Updated **{updated}**/**{total}** items.");
                }

                await message.Channel.SendMessage($"Item updates completed. Updated **{updated}**/**{total}** items.");
            }
        }

        private struct UpdateArguments
        {
        }
    }
}
