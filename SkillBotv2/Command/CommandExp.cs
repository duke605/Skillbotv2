using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using SkillBotv2.Exceptions;
using SkillBotv2.Extensions;
using SkillBotv2.Util;

namespace SkillBotv2.Command
{
    class CommandExp : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            args = string.Join(" ", args).Replace("-", " ").Split(' ');
            byte level1 = args[0].ToByte(), 
                 level2 = args[1].ToByte();

            // Validating
            if (level1 > 120)
                throw new ControlledException("Level1 must be equal to or less than 120.");

            // Validating
            if (level2 > 120)
                throw new ControlledException("Level1 must be equal to or less than 120.");

            // Validating
            if (level1 <= 0)
                throw new ControlledException("Level1 must be greater than 0.");

            if (level2 <= 0)
                throw new ControlledException("Level2 must be greater than 0.");

            return new Arguments
            {
                Level1 = args[0].ToByte(),
                Level2 = args[1].ToByte()
            };
        }

        public async Task Execute(object arguments, Message message)
        {
            var a = (Arguments) arguments;
            var expDiff = RSUtil.ExpBetweenLevels(a.Level1, a.Level2);

            await message.Channel.SendMessage(
                $"The total experience between levels **{a.Level1}** and **{a.Level2}** " +
                $"is **{expDiff.ToString("#,##0")}**");
        }

        public struct Arguments
        {
            public byte Level1 { get; set; }
            public byte Level2 { get; set; }
        }
    }
}
