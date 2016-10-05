using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Fclp.Internals.Extensions;
using SkillBotv2.Extensions;

namespace SkillBotv2.Command.User
{
    partial class CommandUser
    {
        private async Task<object> ParseRemoveArguments(string[] args, Message m)
        {
            if (m.User.Id != 136856172203474944)
            {
                await m.Channel.SendMessage("**Not autorized.**");
                return false;
            }

            return "remove";
        }

        private async Task RemoveUser(Message m)
        {
            using (var db = new Database())
            {
                // Checking if there are any users mentioned
                if (!m.MentionedUsers.Any())
                {
                    await m.Channel.SendMessage("No user(s) mentioned or mentioned user(s) could not be found.");
                    return;
                }

                var ids = string.Join(",", m.MentionedUsers.Select(u => u.Id));

                // Deleting users items from DB
                await db.Database.Transaction(async () =>
                {
                    int ret = await db
                        .Database
                        .ExecuteSqlCommandAsync("DELETE FROM users WHERE Id IN (@p0)", ids);

                    if (ret < 0)
                        throw new Exception("Could not remove user(s).");

                    await m.Channel.SendMessage("User(s) successfully removed.");
                });
            }
        }
    }
}
