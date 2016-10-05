using System;
using System.Threading.Tasks;

namespace SkillBotv2.Extensions
{
    static class DatabaseExtensions
    {
        public static async Task Transaction(this System.Data.Entity.Database db, Func<Task> cb)
        {
            using (var tx = db.BeginTransaction())
            {
                try
                {
                    await cb();
                    tx.Commit();
                }
                catch (Exception)
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}
