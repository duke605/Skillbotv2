using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SkillBotv2.Command
{
    interface ICommand
    {
        Task<object> ParseArguments(string[] args, Message message);
        Task Execute(object arguments, Message message);
    }
}
