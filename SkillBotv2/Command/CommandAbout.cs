using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SkillBotv2.Command
{
    class CommandAbout : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            return "";
        }

        public async Task Execute(object arguments, Message m)
        {
            await m.Channel.SendMessage("__Author:__ @Duke605#4705\n" +
                                        "__Library:__ Discord.Net\n" +
                                        "__Version:__ 2.0.0\n");
        }
    }
}
