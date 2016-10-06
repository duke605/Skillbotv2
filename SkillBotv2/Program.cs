using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using SkillBotv2.Command;
using SkillBotv2.Command.Item;
using SkillBotv2.Command.Recipe;
using SkillBotv2.Command.User;
using SkillBotv2.Exceptions;
using SkillBotv2.Extensions;
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
            SetupStateListener();
            SetupMessageListener();
            Connect();
        }
        private static void SetupCommands()
        {
            ICommand c;
            Commands.Add("user", new CommandUser());
            Commands.Add("imagify", new CommandImagify());
            Commands.Add("help", new CommandHelp());
            Commands.Add("online", new CommandOnline());
            Commands.Add("price", new CommandPrice());
            Commands.Add("use", new CommandUse());
            Commands.Add("trigger", new CommandTrigger());

            // News aliases
            c = new CommandNews();
            Commands.Add("news", c);
            Commands.Add("new", c);

            // Clean up aliases
            c = new CommandClean();
            Commands.Add("clean", c);
            Commands.Add("clear", c);

            // Recipes aliases
            c = new CommandRecipe();
            Commands.Add("recipe", c);
            Commands.Add("recipes", c);

            // Item aliases
            c = new CommandItem();
            Commands.Add("item", c);
            Commands.Add("items", c);

            // About aliases
            c = new CommandAbout();
            Commands.Add("about", c);
            Commands.Add("info", c);

            // Portables aliases
            c = new CommandPortables();
            Commands.Add("portables", c);
            Commands.Add("portable", c);
        }

        private static void SetupStateListener()
        {
            Client.LeftServer += async (s, e) =>
            {
                // Creating settings for server
                using (var db = new Database())
                {
                    await db.Database.Transaction(async () =>
                    {
                        var ret = await db.Database.ExecuteSqlCommandAsync(
                            $"DELETE FROM channels WHERE ServerId = {e.Server.Id}");

                        // Checking if channels were deleted
                        if (ret < 0)
                            throw new Exception($"Failed to delete channels that belong to server {e.Server.Id}");

                        ret = await db.Database.ExecuteSqlCommandAsync(
                            $"DELETE FROM servers WHERE Id = {e.Server.Id}");

                        // Checking if channels were deleted
                        if (ret < 0)
                            throw new Exception($"Failed to delete server with the id {e.Server.Id}");
                    });
                }
            };

            Client.JoinedServer += async (s, e) =>
            {
                // Creating settings for server
                using (var db = new Database())
                {
                    db.servers.Add(new server
                    {
                        Id = e.Server.Id,
                        Trigger = "!"
                    });

                    if (await db.SaveChangesAsync() <= 0)
                        await e.Server.Leave();
                }
            };
        }

        private static void SetupMessageListener()
        {
            Client.MessageReceived += async (s, m) =>
            {
                ICommand command;
                var message = m.Message.RawText.Trim();

                // Checking if we can output in this channel
                using (var db = new Database())
                {
                    Match match;
                    var server = await db.servers.FirstAsync(o => o.Id == m.Server.Id);

                        // Failing fast
                    if (m.Message.IsAuthor
                        || m.Channel.IsPrivate
                        || !(match = Regex.Match(message, $"^\\{server.Trigger}(.+?)(?:\\s|$)")).Groups[1].Success
                        || !Commands.TryGetValue(match.Groups[1].Value.ToLower(), out command)
                        || (match.Groups[1].Value.ToLower() != "use" && !db.channels.Any(c => c.Id == m.Channel.Id)))
                    return;
   
                }

                dynamic args;

                try
                {
                    await m.Channel.SendIsTyping();
                }

                catch (Exception)
                {
                    return;
                }

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

                catch (ControlledException e)
                {
                    await m.Channel.SendMessage(e.Message);
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

                catch (ControlledException e)
                {
                    await m.Channel.SendMessage(e.Message);
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
