using System.Threading.Tasks;
using Discord;
using SkillBotv2.Exceptions;

namespace SkillBotv2.Command.TrasureTrail
{
    partial class CommandTreasureTrails : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            var subcommand = args[0].ToUpper();
            var newArgs = new string[args.Length - 1];

            for (int i = 0; i < newArgs.Length; i++)
                newArgs[i] = args[i + 1];

            switch (subcommand)
            {
                case "COORD":
                case "COORDS":
                    return await ParseCoordArguments(newArgs, message);

                case "ANAGRAM":
                case "ANAGRAMS":
                    return await ParseAnagramArguments(newArgs, message);
            }

            throw new ControlledException($"No subcommand called **{subcommand}** for this command.");
        }

        public async Task Execute(object arguments, Message message)
        {
            if (arguments is CoordArguments)
                await LookupCoords((CoordArguments) arguments, message);
            else if (arguments is AnagramArguments)
                await LookupAnagram((AnagramArguments) arguments, message);
        }

        public struct SheetResponse
        {
            public string Range { get; set; }
            public string MajorDimension { get; set; }
            public string[][] Values { get; set; }
        }
    }
}
