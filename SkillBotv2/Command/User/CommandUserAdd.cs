using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp.Internals.Extensions;

namespace SkillBotv2.Command.User
{
    partial class CommandUser
    {
        private async Task<object> ParseAddArguments(string[] args, Message m)
        {
            if (m.User.Id != 136856172203474944)
            {
                await m.Channel.SendMessage("**Not authorized.**");
                return false;
            }

            return "add";
        }

        private async Task AddUser(Message m)
        {
            using (var db = new Database())
            {
                // Checking if there are any users mentioned
                if (!m.MentionedUsers.Any())
                {
                    await m.Channel.SendMessage("No users mentioned or mentioned user could not be found.");
                    return;
                }

                // Adding the users
                m.MentionedUsers.ForEach(u => db.users.Add(new user { Id = u.Id}));

                // Saving
                if (await db.SaveChangesAsync() < 1)
                    await m.Channel.SendMessage("Users could not be saved.");
                else
                    await m.Channel.SendMessage("Users are now authorized to use restricted commands.");
            }
        }
    }
}
