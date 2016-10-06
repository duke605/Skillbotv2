using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using SkillBotv2.Util;

namespace SkillBotv2.Command
{
    class CommandPrice : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            var item = string.Join(" ", args);
            return await RSUtil.GetItemForDynamic(item);
        }

        public async Task Execute(object a, Message message)
        {
            var item = (item) a;

            // Outputing information
            await message.Channel.SendMessage($"**{item.Name}:** `{item.Price.ToString("#,##0")}` GP\n");

            using (var db = new Database())
            {
                // Adding item to db cause why not
                db.items.AddOrUpdate(item);
            }
        }
    }
}
