using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Fclp.Internals.Extensions;
using SkillBotv2.Extensions;
using SkillBotv2.Util;

namespace SkillBotv2.Command
{
    class CommandImagify : ICommand
    {
        public async Task<object> ParseArguments(string[] args, Message message)
        {
            return ulong.Parse(args.ElementAtOrDefault(0, "0"));
        }

        public async Task Execute(object arguments, Message m)
        {
            ulong messageId = (ulong) arguments;
            Message table;
            string parsedText;
            Stopwatch find = new Stopwatch(),
                      convert = new Stopwatch(),
                      upload = new Stopwatch();

            // Getting message
            table = messageId == 0 
                ? find.TimeTask(() => m.Channel.Messages.OrderByDescending(o => o.Id).FirstOrDefault(o => o.Text.Matches(@"\A```(.*?)```\z", RegexOptions.Singleline)))
                : find.TimeTask(() => m.Channel.GetMessage(messageId));

            // Chekcing if message was found
            if (table?.Text == null || (parsedText = Regex.Match(table.Text, @"\A```(.*?)```\z", RegexOptions.Singleline).Groups[1].Value) == null)
                throw new Exception("Message could not be found.");

            var img = convert.TimeTask(() => ImageUtil.ToImage(parsedText));
            var link = await upload.TimeTaskAsync(async () => await ImageUtil.PostToImgur(img));
            await m.Channel.SendMessage(link + "\n" +
                                        $"**Find Message**: {find.ElapsedMilliseconds}ms | " +
                                        $"**Convert Text**: {convert.ElapsedMilliseconds}ms | " +
                                        $"**Upload Image**: {upload.ElapsedMilliseconds}ms");
        }
    }
}
