using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using SkillBotv2.Command;
using SkillBotv2.Command.Item;
using SkillBotv2.Command.Recipe;
using SkillBotv2.Command.User;
using SkillBotv2.Util;
using SuperSocket.ClientEngine;
using unirest_net.http;

namespace SkillBotv2
{
    class Program
    {
        public static DiscordClient Client = new DiscordClient();
        public static Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand>();
        
        static void Main(string[] args)
        {
            SetupCommands();
            SetupMessageListener();
            Connect();
        }
        private static void SetupCommands()
        {
            ICommand c;
            Commands.Add("item", new CommandItem());
            Commands.Add("recipe", new CommandRecipe());
            Commands.Add("user", new CommandUser());
            Commands.Add("imagify", new CommandImagify());
            Commands.Add("clean", new CommandClean());
            Commands.Add("help", new CommandHelp());

            // About aliases
            c = new CommandAbout();
            Commands.Add("about", c);
            Commands.Add("info", c);

            // Portables aliases
            c = new CommandPortables();
            Commands.Add("portables", c);
            Commands.Add("portable", c);
        }

        private static void SetupMessageListener()
        {
            Client.MessageReceived += async (s, m) =>
            {
                Match match;
                ICommand command;
                string message = m.Message.RawText.Trim();

                // Failing fast
                if (m.Message.IsAuthor
                    || (m.Channel.Id != 143730268614688768 && m.Channel.Id != 231188496642080770)
                    || !(match = Regex.Match(message, @"^!(.+?)(?:\s|$)")).Groups[1].Success
                    || !Commands.TryGetValue(match.Groups[1].Value, out command))
                    return;

                dynamic args;

                await m.Channel.SendIsTyping();

                // Parsing input
                try
                {
                    var commandLine = message.Contains(' ') 
                        ? message.Substring(message.IndexOf(' ') + 1) 
                        : "";

                    args = await command.ParseArguments(CommandLineUtil.CommandLineToArgs(commandLine), m.Message);

                    // Checking if args returns a boolean
                    if (args is bool)
                        return;
                } 
                
                catch (Exception e)
                {
                    await m.Channel.SendMessage("An error occured when parsing your input.\n" +
                                                $"```{e.GetBaseException().Message}```");
                    Console.WriteLine(e.StackTrace);
                    return;
                }

                // Executing command
                try
                { 
                    // Running command
                    await command.Execute(args, m.Message);
                } 
                
                catch (Exception e)
                {
                    await m.Channel.SendMessage("An error occured when executing the command.\n" +
                                                $"```{e.GetBaseException().Message}```");
                    Console.WriteLine(e.StackTrace);
                }
            };
        }

        private static void Connect()
        {
            Client.ExecuteAndWait(async () =>
            {
                Console.Write("Connecting... ");
                await Client.Connect(Secret.Token, TokenType.User);
                Console.WriteLine("Done.");
            });
        }
    }
}
