using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp.Internals.Extensions;

namespace SkillBotv2.Command
{
    class CommandClean : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            return "";
        }

        public async Task Execute(object arguments, Message message)
        {
            // Getting messages
            var messages = (await message.Channel.DownloadMessages())
                .Where(m => m.User.Id == Program.Client.CurrentUser.Id)
                .ToArray();

            await message.Channel.DeleteMessages(messages);

            var m1 = await message.Channel.SendMessage($"Deleted **{messages.Length}** messages.");
            await Task.Delay(5000);
            await m1.Delete();
        }
    }
}
