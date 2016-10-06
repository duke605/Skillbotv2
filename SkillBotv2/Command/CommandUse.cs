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
    class CommandUse : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            return message.Channel.Id;
        }

        public async Task Execute(object arguments, Message m)
        {
            // Checking permission
            if (m.User.Id != 136856172203474944 && m.User.Id != m.Server.Owner.Id)
                throw new ControlledException("You do not have permission to use this command.");

            using (var db = new Database())
            {
                db.channels.Add(new channel {Id = m.Channel.Id, ServerId = m.Server.Id});

                if (await db.SaveChangesAsync() < 1)
                {
                    // Checking if the channel is already in use
                    if (await db.channels.FirstOrDefaultAsync(c => m.Channel.Id == c.Id) != null)
                        await m.Channel.SendMessage("Channel is already in use.");
                    else
                        await m.Channel.SendMessage("Channel could not be added.");

                    return;
                }

                await m.Channel.SendMessage("Can now use commands on this channel.");
            }
        }
    }
}
