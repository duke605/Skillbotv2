using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp.Extensions;
using SkillBotv2.Entities;
using unirest_net.http;

namespace SkillBotv2.Util
{
    static class OSRSUtil
    {
        /// <summary>
        /// Gets the user's stats
        /// </summary>
        /// <param name="username">The user to get the stats for</param>
        /// <returns>The user's stats</returns>
        public static async Task<OSRSStats> GetStatsForUser(string username)
        {
            var r = await Unirest.get($"http://services.runescape.com/m=hiscore_oldschool/index_lite.ws?player={username.UrlEncode()}")
                .asStringAsync();

            // Checking response
            if (r.Code < 200 || r.Code > 299)
                throw new Exception($"Request returned {r.Code}");

            var stats = new OSRSStats();
            var lines = r.Body.Split('\n');
            var props = stats.GetType().GetProperties();

            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var parts = lines[i].Split(',');

                prop.SetMethod.Invoke(stats, new object[] { Stat.CreateFromCSV(parts) });
            }

            return stats;
        }
    }
}
