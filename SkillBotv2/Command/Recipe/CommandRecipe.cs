using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SkillBotv2.Command.Recipe
{
    partial class CommandRecipe : ICommand
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
                case "SEARCH":
                    return await ParseSearchArguments(newArgs, message);
            }

            return false;
        }

        public async Task Execute(object arguments, Message message)
        {
            if (arguments is AddArguments)
                await AddRecipe((AddArguments) arguments, message);
            else if (arguments is RemoveArguments)
                await RemoveRecipe((RemoveArguments) arguments, message);
            else if (arguments is SearchArguments)
                await SearchRecipe((SearchArguments) arguments, message);
        }
    }
}
