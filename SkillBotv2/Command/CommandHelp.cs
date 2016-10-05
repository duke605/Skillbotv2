using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SkillBotv2.Command
{
    class CommandHelp : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            return "";
        }

        public async Task Execute(object arguments, Message message)
        {
            await message.Channel.SendMessage("<https://github.com/duke605/Skillbotv2/wiki/Commands>");
        }
    }
}
