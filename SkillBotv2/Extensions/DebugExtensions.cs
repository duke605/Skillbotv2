using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillBotv2.Extensions
{
    static class DebugExtensions
    {
        /// <summary>
        /// Times how long something takes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="timer"></param>
        /// <param name="task">The task to be completed</param>
        /// <returns>Whatever the task returns</returns>
        public static T TimeTask<T>(this Stopwatch timer,Func<T> task)
        {
            timer.Start();
            var ret = task();
            timer.Stop();
            return ret;
        }

        /// <summary>
        /// See TimeTask
        /// </summary>
        public static async Task<T> TimeTaskAsync<T>(this Stopwatch timer, Func<Task<T>> task)
        {
            timer.Start();
            var ret = await task();
            timer.Stop();
            return ret;
        }
    }
}
