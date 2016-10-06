using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using SkillBotv2.Exceptions;

namespace SkillBotv2.Command
{
    class CommandTrigger : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            return args[0][0];
        }

        public async Task Execute(object trigger, Message m)
        {
            // Checking permission
            if (m.User.Id != 136856172203474944 && m.User.Id != m.Server.Owner.Id)
                throw new ControlledException("You do not have permission to use this command.");

            using (var db = new Database())
            {
                var server = await db.servers.FirstOrDefaultAsync(s => s.Id == m.Server.Id);
                server.Trigger = trigger.ToString();

                if (await db.SaveChangesAsync() < 0)
                    await m.Channel.SendMessage("Could not save trigger.");
                else
                    await m.Channel.SendMessage($"Trigger has been changed to **{trigger}**.");
            }
        }
    }
}
