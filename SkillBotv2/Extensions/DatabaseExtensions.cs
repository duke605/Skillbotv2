using System;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SkillBotv2.Extensions
{
    static class DatabaseExtensions
    {
        /// <summary>
        /// Creates a transaction and exectures the passed code then commits or rolls back automatically
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cb">The code to execute</param>
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
