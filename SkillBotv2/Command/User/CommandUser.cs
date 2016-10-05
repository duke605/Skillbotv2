using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SkillBotv2.Command.User
{
    partial class CommandUser : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            var subcommand = args[0].ToUpper();
            var newArgs = new string[args.Length - 1];

            for (int i = 0; i < newArgs.Length; i++)
                newArgs[i] = args[i + 1];

            switch (subcommand)
            {
                case "ADD":
                    return await ParseAddArguments(newArgs, message);
                case "REMOVE":
                    return await ParseRemoveArguments(newArgs, message);
            }

            return false;
        }

        public async Task Execute(object arguments, Message message)
        {
            if (arguments.ToString() == "add")
                await AddUser(message);
            else if (arguments.ToString() == "remove")
                await RemoveUser(message);
        }
    }
}
