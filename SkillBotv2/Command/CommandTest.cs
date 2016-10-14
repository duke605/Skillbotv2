using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SkillBotv2.Command
{
    class CommandTest : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            return null;
        }

        public async Task Execute(object arguments, Message message)
        {
            using (var @in = File.OpenText("C:/Users/Cole/Desktop/items.csv"))
            {
                string line;

                using (var db = new Database())
                {
                    while ((line = @in.ReadLine()) != null)
                    {
                        var split = line.Split(',');
                        var item = new item
                        {
                            Id = decimal.Parse(split[0]),
                            Name = split[1],
                            Price = int.Parse(split[2]),
                            UpdatedAt = DateTime.Now
                        };

                        db.items.AddOrUpdate(item);
                    }

                    if (await db.SaveChangesAsync() <= 0)
                        await message.Channel.SendMessage("ERROR!");
                    else
                        await message.Channel.SendMessage("SUCCESS!");
                }
            }
        }
    }
}
