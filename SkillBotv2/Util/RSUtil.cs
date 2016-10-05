using System;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fclp.Internals.Extensions;
using RestSharp.Extensions;
using SkillBotv2.Command.Recipe;
using SkillBotv2.Entities;
using SkillBotv2.Extensions;
using unirest_net.http;

namespace SkillBotv2.Util
{
    class RSUtil
    {

        /// <summary>
        /// Gets item data
        /// </summary>
        /// <param name="itemName">The name of the item</param>
        /// <returns>the item data</returns>
        public static async Task<item> GetItemForName(string itemName)
        {
            // Replacing spaces with _ and casing properly
            itemName = itemName.ToSentenceCase().Replace(" ", "_");

            // Getting item data
            HttpResponse<string> data = await Unirest.get($"http://runescape.wikia.com/wiki/Module:Exchange/{itemName}?action=raw")
                .asStringAsync();

            // Checking if response was okay
            if (data.Code < 200 || data.Code > 299)
                throw new HttpRequestException($"Request returned {data.Code}.");

            return item.ValueOf(data.Body);
        }

        /// <summary>
        /// Gets item data for the given item id
        /// </summary>
        /// <param name="itemId">The id of the item to get the data for</param>
        /// <returns>the item data</returns>
        public static async Task<item> GetItemForId(int itemId)
        {
            // Getting item data
            HttpResponse<string> data = await Unirest.get($"http://rscript.org/lookup.php?type=ge&search={itemId}&exact=1")
                .asStringAsync();

            // Checking if response was okay
            if (data.Code < 200 || data.Code > 299)
                throw new HttpRequestException($"Request returned {data.Code}.");

            // Finding item name
            Match match = Regex.Match(data.Body, @"\nITEM: \d+?\s(.+?)\s");

            // Checking if the item name was found
            if (!match.Groups[1].Success)
                throw new HttpRequestException($"Item with id {itemId} not found.");

            return await GetItemForName(match.Groups[1].Value);
        }

        /// <summary>
        /// Gets the item data for item name or id
        /// </summary>
        /// <param name="item">The id or name of the item to get the data for</param>
        /// <returns>the item data</returns>
        public static async Task<item> GetItemForDynamic(string item)
        {
            int itemId;

            if (int.TryParse(item, out itemId))
                return await GetItemForId(itemId);
            
            return await GetItemForName(item);
        }

        public static async Task<Stats> GetStatsForUser(string username)
        {
            HttpResponse<string> r = await Unirest.get($"http://services.runescape.com/m=hiscore/index_lite.ws?player={username.UrlEncode()}")
                .asStringAsync();

            // Checking response
            if (r.Code < 200 || r.Code > 299)
                throw new Exception($"Request returned {r.Code}");

            var stats = new Stats();
            var lines = r.Body.Split('\n');
            var props = stats.GetType().GetProperties();

            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var parts = lines[i].Split(',');

                prop.SetMethod.Invoke(stats, new object[] {Stats.Stat.CreateFromCSV(parts)});
            }

            return stats;
        }

        /// <summary>
        /// Calculates the remaining exp between current exp and desired level
        /// </summary>
        /// <param name="stat">The user's stat</param>
        /// <param name="level">The desired level</param>
        /// <returns>The exp between current exp and the desired level</returns>
        public static int ExpBetweenLevels(Stats.Stat stat, double level)
        {
            int exp = 0;

            for (double i = 1; i < level; i++)
            {
                exp += (int)Math.Floor(i + 300 * Math.Pow(2, i / 7));
            }

            return (int)(Math.Floor(exp / 4.0) - stat.Exp);
        }
    }
}
