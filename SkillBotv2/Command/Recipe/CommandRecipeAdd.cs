using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Fclp;
using SkillBotv2.Extensions;
using SkillBotv2.Util;

namespace SkillBotv2.Command.Recipe
{
    partial class CommandRecipe
    {
        private async Task<object> ParseAddArguments(string[] args, Message message)
        {
            AddArguments a = new AddArguments { Inputs = new List<AddArguments.Stack>(), Outputs = new List<AddArguments.Stack>() };
            FluentCommandLineParser parser = new FluentCommandLineParser();
            bool helpCalled = false;
            Action<string, string> transformer = (type, s) =>
            {
                // Checking if input is formatted correctly
                if (!s.Contains(":"))
                    throw new FormatException($"{type} not formatted correctly <itemName|itemId>:<quantity>");

                // Parsing input
                string[] t = s.Split(':');
                int qty = int.Parse(t[1]);
                string item = t[0].ToSentenceCase();

                if (qty < 1)
                    throw new Exception("Quantity must be greater than 0.");

                a.Inputs.Add(new AddArguments.Stack
                {
                    Id = item,
                    Quantity = qty
                });
            };

            parser.Setup<string>('n', "name")
                .Required()
                .Callback(n =>
                {
                    // Validating
                    if (n.Trim().Length < 4)
                        throw new Exception("Name must have at more than 4 characters");

                    a.Name = n;
                });

            parser.Setup<double>('e', "exp")
                .Required()
                .Callback(e =>
                {
                    // Validating
                    if (e < 0)
                        throw new Exception("Exp must be greater than 0.");

                    a.Exp = e;
                });

            parser.Setup<double>('u', "units")
                .Required()
                .Callback(u =>
                {
                    // Validating
                    if (u < 0)
                        throw new Exception("Units must be greater than 0.");

                    a.Units = u;
                });

            parser.Setup<int>('l', "level")
                .Required()
                .Callback(l =>
                {
                    // Validating 
                    if (l < 1 || l > 120)
                        throw new Exception("Level must be between 1 and 120.");

                    a.Level = (sbyte) l;
                });

            parser.Setup<AddArguments.Skill>('s', "skill")
                .Required()
                .Callback(s => a.Type = s);

            parser.Setup<int>('E', "extra")
                .SetDefault(0)
                .Callback(c =>
                {
                    // Validating
                    if (c < 0)
                        throw new Exception("Extra must be greater than -1.");

                    a.Extra = c;
                });

            parser.Setup<List<string>>('i', "input")
                .Callback(i => i.ForEach(input => transformer("Input", input)));

            parser.Setup<List<string>>('o', "output")
                .Callback(i => i.ForEach(output => transformer("Output", output)));

            parser.Setup<bool>('?', "help")
                .Callback(h => helpCalled = h);

            var r = parser.Parse(args);

            if (r.HasErrors || helpCalled)
            {
                if (!helpCalled)
                {
                    await message.Channel.SendMessage($"**Error:**```{r.ErrorText}```");
                    return false;
                }

                var m = "";

                m += "**Usage:**" +
                     "```!recipe add <-n|name> <-e|exp> <-l|level> <-u|units> <-s|skill> [-E|extra] [-i|input] [-o|output]```\n" +
                     "**Options:**\n" +
                     "`-n`, `--name` **REQUIRED** [string]\n" +
                     "The name of the training method. Usually named after the output.\n" +
                     "\n" +
                     "`-e`, `--exp` **REQUIRED** [double (> 0)]" +
                     "The amount of exp this recipe produces.\n" +
                     "\n" +
                     "`-l`, `--level` **REQUIRED** [int (1-120)]\n" +
                     "The level required to do this recipe.\n" +
                     "\n" +
                     "`-u`, `--units` **REQUIRED** [double (> 0)]\n" +
                     "The about of times this recipe can be done in one hour. Fractions are allowed.\n" +
                     "\n" +
                     "`-s`, `--skill` **REQUIRED** [enum]\n" +
                     "The skill the recipe is for.\n" +
                     "\n" +
                     "`-E`, `--extra` *OPTIONAL* [int (> -1)]\n" +
                     "Any extra cost the recipe might have from items that are not tradable but buyable from a shop\n" +
                     "like cleansing crystals.\n" +
                     "\n" +
                     "`-i`, `--input` *OPTIONAL* [custom]\n" +
                     "A list of inputs for the recipe if there are any. Format: <itemName|itemId>:<quantity>\n" +
                     "\tEg. --input \"Oak logs:1\" \"Yew logs:2\" would make the recipe required 1 oak log and 2 yew logs.\n" +
                     "\n" +
                     "`-o`, `--output` *OPTIONAL* [custom]\n" +
                     "A list of outputs for the recipe if there are any. Format <itemName|itemId>:<quantity>\n" +
                     "\tEg. --output \"Oak logs:1\" \"Yew logs:2\" would make the recipe output 1 oak log and 2 yew logs.\n";

                await message.Channel.SendMessage(m);
                return false;
            }

            return a;
        }

        private async Task AddRecipe(AddArguments a, Message message)
        {
            using (var db = new Database())
            {
                // Checking user autorization
                if (message.User.Id != 136856172203474944 && await db.users.FindAsync(message.User.Id) == null)
                {
                    await message.Channel.SendMessage("You do not have permission to use that command.");
                    return;
                }

                // Getting items
                foreach (var stack in a.Inputs)
                    await stack.GetItemFromDb(db);

                var snowflake = TimeUtil.GenerateSnowflake(0, (ushort) (message.User.Id%4095));

                // Doing the stuff, the good stuff
                await db.Database.Transaction(async () =>
                {
                    var recipe = new recipe();
                    var itemVoid = new item {Id = 0};

                    // Putting in dummy for inputs and putputs
                    if (!a.Inputs.Any()) a.Inputs.Add(new AddArguments.Stack { Item = itemVoid, Quantity = 0 });
                    if (!a.Outputs.Any()) a.Outputs.Add(new AddArguments.Stack { Item = itemVoid, Quantity = 0 });

                    foreach (var i in a.Inputs)
                        recipe.inputs.Add(new input { ItemId = i.Item.Id, recipe = recipe, Quantity = i.Quantity});

                    foreach (var o in a.Outputs)
                        recipe.outputs.Add(new output { ItemId = o.Item.Id, recipe = recipe, Quantity = o.Quantity });

                    recipe.Id = snowflake;
                    recipe.UserId = message.User.Id;
                    recipe.Name = a.Name;
                    recipe.Exp = a.Exp;
                    recipe.Units = a.Units;
                    recipe.Level = a.Level;
                    recipe.Skill = (sbyte) a.Type;
                    recipe.Extra = a.Extra;
                    db.recipes.Add(recipe);

                    if (await db.SaveChangesAsync() <= 0)
                        throw new Exception("Recipe could not be saved.");
                });

                await message.Channel.SendMessage($"Recipe **{a.Name}** has been successfully added.");
            }
        }

        public struct AddArguments
        {
            public string Name { get; set; }
            public List<Stack> Inputs { get; set; }
            public List<Stack> Outputs { get; set; }
            public double Exp { get; set; }
            public double Units { get; set; }
            public sbyte Level { get; set; }
            public Skill Type { get; set; }
            public int Extra { get; set; }

            public class Stack
            {
                public string Id { get; set; }
                public item Item { get; set; }
                public int Quantity { get; set; }

                public async Task GetItemFromDb(Database db)
                {
                    int temp;
                    string name = Id.ToSentenceCase();

                    // Getting them item from the DB
                    if (int.TryParse(Id, out temp))
                        Item = await db.items.FindAsync(temp);
                    else
                        Item = await db.items.FirstOrDefaultAsync(i => i.Name == name);

                    // Checking if the item could be retrieved
                    if (Item == null)
                        throw new ObjectNotFoundException($"The item with the name/id \"{name}\" could not be found.\n" +
                                                          "If it is a valid item please add it using the \"!item add\" command.");
                }
            }
           
            public enum Skill : sbyte
            {
                Woodcutting,
                Firemaking,
                Cooking,
                Construction,
                Crafting,
                Prayer,
                Summoning,
                Magic,
                Herblore,
                Mining,
                Smithing,
                Fishing,
                Fletching,
                Runecrafting,
                Farming,
                Hunter
            }
        }
    }
}
