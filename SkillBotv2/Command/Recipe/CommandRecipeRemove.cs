using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp;
using Mono.Options;
using SkillBotv2.Extensions;

namespace SkillBotv2.Command.Recipe
{
    partial class CommandRecipe
    {
        private async Task<object> ParseRemoveArguments(string[] args, Message message)
        {
            RemoveArguments a = new RemoveArguments();
            FluentCommandLineParser parser = new FluentCommandLineParser();
            OptionSet set = new OptionSet
            {
                { "a|all", "", v => "".Trim() }
            };

            parser.Setup<bool>('a', "all")
                .SetDefault(false)
                .Callback(e =>
                {
                    // Checking if me
                    if (e && message.User.Id != 136856172203474944)
                        throw new Exception("You do not have permission to use this command with the `--all` switch.");

                    a.All = e;
                });


            parser.Parse(args);
            var extra = set.Parse(args);
            var temp = extra.ElementAtOrDefault(0)?.Trim().ToLower();
            ulong t;

            // Checking if there an id was supplied
            if (temp == null)
                throw new Exception("Recipe id or **Lastest** must be supplied for argument 1.");

            if (!ulong.TryParse(temp, out t) && temp != "latest")
                throw new Exception("First argument must be a number or **Latest**.");

            using (var db = new Database())
            {
                // Getting recipe
                if (temp == "latest")
                    a.Recipe = await db.recipes.OrderByDescending(r => r.Id)
                        .FirstOrDefaultAsync(r => r.UserId == message.User.Id);
                else
                    a.Recipe = await db.recipes.OrderByDescending(r => r.Id)
                        .FirstOrDefaultAsync(r => r.Id == t);
            }

            if (a.Recipe == null)
                throw new Exception("No recipe found.");

            // Checking if recipe belongs to user
            if (a.Recipe?.UserId != message.User.Id && message.User.Id != 136856172203474944)
                throw new Exception("You cannot delete a recipe that does not belong to you.");

            return a;
        }

        private async Task RemoveRecipe(RemoveArguments a, Message message)
        {
            using (var db = new Database())
            {
                await db.Database.Transaction(async () =>
                {
                    int ret;

                    // Removing all inputs for user
                    if (a.All)
                        ret = await db.Database.ExecuteSqlCommandAsync(
                            "DELETE FROM i " +
                            "USING inputs AS i " +
                            "INNER JOIN recipes r " +
                            "ON i.RecipeId = r.Id " +
                            $"WHERE r.UserId = {a.Recipe.UserId};");

                    // Removing inputs for recipe
                    else
                        ret = await db.Database.ExecuteSqlCommandAsync(
                            "DELETE FROM i " +
                            "USING inputs AS i " +
                            $"WHERE i.RecipeId = {a.Recipe.Id}");

                    // Checking code
                    if (ret < 0)
                        throw new Exception("An error occured when removing inputs.");

                    // Removing all outputs for user
                    if (a.All)
                        ret = await db.Database.ExecuteSqlCommandAsync(
                            "DELETE FROM o " +
                            "USING outputs AS o " +
                            "INNER JOIN recipes r " +
                            "ON o.RecipeId = r.Id " +
                            $"WHERE r.UserId = {a.Recipe.UserId};");

                    // Removing outs for recipe
                    else
                        ret = await db.Database.ExecuteSqlCommandAsync(
                            "DELETE FROM o " +
                            "USING outputs AS o " +
                            $"WHERE o.RecipeId = {a.Recipe.Id}");

                    // Checking code
                    if (ret < 0)
                        throw new Exception("An error occured when removing outputs.");

                    // Removing all recipes for user
                    if (a.All)
                        ret = await db.Database.ExecuteSqlCommandAsync(
                            "DELETE FROM r " +
                            "USING recipes AS r " +
                            $"WHERE r.UserId = {a.Recipe.UserId}");

                    // Removing recipe
                    else
                        ret = await db.Database.ExecuteSqlCommandAsync(
                            "DELETE FROM r " +
                            "USING recipes AS r " +
                            $"WHERE r.Id = {a.Recipe.Id}");

                    // Checking code
                    if (ret < 0)
                        throw new Exception("An error occured when removing recipe(s).");
                });

                if (a.All)
                    await message.Channel.SendMessage($"All recipes from <@{a.Recipe.UserId}> have been removed.");
                else
                    await message.Channel.SendMessage($"Recipe **{a.Recipe.Name}** has been removed.");
            }
        }

        public struct RemoveArguments
        {
            public bool All { get; set; }
            public recipe Recipe { get; set; }
        }
    }
}
