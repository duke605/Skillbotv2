using System.Threading.Tasks;
using Discord;

namespace SkillBotv2.Command.Item
{
    partial class CommandItem : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            var subcommand = args[0].ToUpper();
            var newArgs = new string[args.Length-1];

            for (int i = 0; i < newArgs.Length; i++)
                newArgs[i] = args[i+1];

            switch (subcommand)
            {
                case "ADD":
                    return await ParseAddArguments(newArgs, message);
                case "UPDATE":
                    return await ParseUpdateArguments(newArgs, message);
            }

            return false;
        }

        public async Task Execute(object arguments, Message message)
        {
            // Delegating to subcommand handler
            if (arguments is AddArguments)
                await AddItem((AddArguments) arguments, message);
            else if (arguments is UpdateArguments)
                await UpdateItem((UpdateArguments) arguments, message);
        }
    }
}
