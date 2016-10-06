using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp;
using SkillBotv2.Exceptions;

namespace SkillBotv2.Command
{
    class CommandUse : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            byte allow = 1;
            var parser = new FluentCommandLineParser();

            parser.Setup<bool>('r', "revoke")
                .SetDefault(false)
                .Callback(r => allow = (byte) (r ? 0 : 1));

            parser.Parse(args);

            return allow;
        }

        public async Task Execute(object arguments, Message m)
        {
            // Checking permission
            if (m.User.Id != 136856172203474944 && m.User.Id != m.Server.Owner.Id)
                throw new ControlledException("You do not have permission to use this command.");

            using (var db = new Database())
            {
                var ret = await db.Database.ExecuteSqlCommandAsync((byte) arguments == 1
                    ? $"INSERT INTO channels VALUES ({m.Channel.Id}, {m.Server.Id})" 
                    : $"DELETE FROM channels WHERE Id = {m.Channel.Id}");

                if (ret < 1)
                {
                    // Adding channel
                    if ((byte) arguments == 1)
                    {
                        // Checking if the channel is already in use
                        if (await db.channels.FirstOrDefaultAsync(c => m.Channel.Id == c.Id) != null)
                            await m.Channel.SendMessage("Channel is already in use.");
                        else
                            await m.Channel.SendMessage("Channel could not be used.");
                    }
                    
                    // Revoking channel
                    else
                    {
                        // Checking if the channel is already in use
                        if (await db.channels.FirstOrDefaultAsync(c => m.Channel.Id == c.Id) != null)
                            await m.Channel.SendMessage("Channel is not use.");
                        else
                            await m.Channel.SendMessage("Channel could not be revoked.");
                    }

                    return;
                }

                if ((byte)arguments == 1)
                    await m.Channel.SendMessage("Can now use commands on this channel.");
                else
                    await m.Channel.SendMessage("Can no longer use commands on this channel.");
            }
        }
    }
}
