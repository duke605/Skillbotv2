using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Extensions;
using SkillBotv2.Entities;
using SkillBotv2.Exceptions;
using SkillBotv2.Extensions;
using Tweetinvi.Core.Extensions;
using unirest_net.http;

namespace SkillBotv2.Util
{
    static class RSUtil
    {
        public class NameId
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// Gets the last time the DE was updated
        /// </summary>
        public static async Task<uint> GetRuneday()
        {
            return (await GetFromJCA("http://services.runescape.com/m=itemdb_rs/api/info.json"))
                .Get<uint>("lastConfigUpdateRuneday");
        }

        /// <summary>
        /// Gets the item ID for a name that may not be exact
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <returns>The id of that belongs to the item name or null</returns>
        public static async Task<NameId> FuzzyMatchName(string name)
        {
            using (var db = new Database())
            {
                // Checking if name is in DB
                var correct = await db.Database.SqlQuery<NameId>(
                        "SELECT Id, Name " +
                        "FROM items " +
                        "WHERE LOWER(Name) = @p0 " +
                        "OR SOUNDEX(Name) = SOUNDEX(@p0) " +
                        "LIMIT 1", name)
                        .FirstOrDefaultAsync();

                // Found name
                if (correct != null)
                    return correct;
                
                // Getting item name from site
                var data = await Unirest.get($"http://rscript.org/lookup.php?type=ge&search={name.UrlEncode()}&exact=1")
                    .asStringAsync();

                // Getting item name from weird return data
                var matchId = Regex.Match(data.Body, @"\nIID:\s(\d+)(?:$|\n)").Groups[1];
                var matchName = Regex.Match(data.Body, @"\nITEM:\s\d+\s(.+?)\s").Groups[1];

                // Found name
                if (matchId.Success && matchName.Success)
                    return new NameId { Name = matchName.Value.Replace("_", " "), Id = matchId.Value.ToInt() };

                // Fuzzy matching the DB very slowly
                correct = await db.Database.SqlQuery<NameId>(
                    "SELECT Id, Name " +
                    "FROM items " +
                    "ORDER BY sys.jaro_winkler(Name, @p0) DESC " +
                    "LIMIT 1", name)
                    .FirstOrDefaultAsync();

                return correct;
            }
        }
        
        /// <summary>
        /// Gets item data
        /// </summary>
        /// <param name="itemName">The name of the item</param>
        /// <returns>the item data</returns>
        public static async Task<item> GetItemForName(string itemName)
        {
            var itemId = await FuzzyMatchName(itemName);

            if (itemId == null)
                throw new ControlledException($"Item with name **{itemName}** could not be found.");

            return await GetItemForId(itemId.Id, itemId.Name);
        }

        /// <summary>
        /// Gets the price history for an item
        /// </summary>
        /// <param name="itemName">The name of the item</param>
        /// <returns>The price history of the item</returns>
        public static async Task<IDictionary<DateTime, int>> GetPriceHistory(string itemName, bool exactName = false)
        {
            if (!exactName)
                itemName = (await FuzzyMatchName(itemName)).Name;

            var data = await Unirest.get($"http://runescape.wikia.com/wiki/Module:Exchange/{itemName.UrlEncode()}/Data?action=raw")
                .asStringAsync();

            // Checking if response was okay
            if (data.Code < 200 || data.Code > 299)
            { 
                if (data.Code == 404)
                    throw new ControlledException($"Price history for item with the name \"{itemName}\" could not be found.");
                
                throw new HttpRequestException($"Request returned {data.Code}.");
            }

            // Converting to json
            var jsonReadable = data.Body
                .Replace("'", "\"")
                .Replace("return {", "[")
                .Replace("}", "]");
            var json = JsonConvert.DeserializeObject<string[]>(jsonReadable);

            // Converting to price history
            var dic = new Dictionary<DateTime, int>();
            json.ForEach( h =>
            {
                var time = TimeUtil.FromUnixTime(h.Split(':')[0].ToUlong());
                var price = h.Split(':')[1].ToInt();

                dic[time] = price;
            });

            return dic;
        }

        /// <summary>
        /// Gets item data for the given item id
        /// </summary>
        /// <param name="itemId">The id of the item to get the data for</param>
        /// <returns>the item data</returns>
        public static async Task<item> GetItemForId(int itemId, string name = null, uint? runedate = null)
        {
            var host = "http://services.runescape.com/m=itemdb_rs/api";

            // Getting name for item if not given
            if (name == null)
                name = (await GetFromJCA($"{host}/catalogue/detail.json?item={itemId}"))
                    ?.Get<JObject>("item")
                    .Get<string>("name");
            
            // Checking if item was found
            if (name == null)
                throw new ControlledException($"Item with the id **{itemId}** could not be found.");

            // Getting runedate for item
            if (runedate == null)
                runedate = await GetRuneday();

            // Checking if runedate could be retrieved
            if (runedate == null)
                throw new ControlledException("Runeday could not be determined.");

            var price = (await GetFromJCA($"{host}/graph/{itemId}.json"))
                ?.GetValue("daily")
                .Last
                .Last
                .Value<int>();

            // Checking if item was found
            if (price == null)
                throw new ControlledException($"Item with the id **{itemId}** could not be found.");

            return new item
            {
                Id = itemId,
                Name = name,
                Price = price.Value,
                UpdatedAt = DateTime.Now,
                UpdatedAtRD = runedate.Value
            };
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

        /// <summary>
        /// Gets the user's stats
        /// </summary>
        /// <param name="username">The user to get the stats for</param>
        /// <returns>The user's stats</returns>
        public static async Task<RS3Stats> GetStatsForUser(string username)
        {
            var r = await Unirest.get($"http://services.runescape.com/m=hiscore/index_lite.ws?player={username.UrlEncode()}")
                .asStringAsync();

            // Checking response
            if (r.Code < 200 || r.Code > 299)
                throw new Exception($"Request returned {r.Code}");

            var stats = new RS3Stats();
            var lines = r.Body.Split('\n');
            var props = stats.GetType().GetProperties();

            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var parts = lines[i].Split(',');

                prop.SetMethod.Invoke(stats, new object[] {Stat.CreateFromCSV(parts)});
            }

            return stats;
        }

        /// <summary>
        /// Calculates the remaining exp between current exp and desired level
        /// </summary>
        /// <param name="stat">The user's stat</param>
        /// <param name="level">The desired level</param>
        /// <returns>The exp between current exp and the desired level</returns>
        public static int ExpBetweenLevels(Stat stat, double level)
        {
            int exp = 0;

            for (double i = 1; i < level; i++)
            {
                exp += (int)Math.Floor(i + 300 * Math.Pow(2, i / 7));
            }

            return (int)(Math.Floor(exp / 4.0) - stat.Exp);
        }

        /// <summary>
        /// Calculates the remaining exp between current exp and desired level
        /// </summary>
        /// <param name="level1">The user's stat</param>
        /// <param name="level2">The desired level</param>
        /// <returns>The exp between current exp and the desired level</returns>
        public static int ExpBetweenLevels(double level1, double level2)
        {
            var exp1 = 0;
            var exp2 = 0;

            for (double i = 1; i < level2; i++)
            {
                if (i < level1)
                    exp1 += (int)Math.Floor(i + 300 * Math.Pow(2, i / 7));
                exp2 += (int)Math.Floor(i + 300 * Math.Pow(2, i / 7));
            }

            return (int) (Math.Floor(exp2 / 4.0) - Math.Floor(exp1 / 4.0));
        }

        /// <summary>
        /// Uses jagex's crappy API
        /// </summary>
        /// <param name="url">A endpoint of jagex's crappy API</param>
        /// <returns>Data returned from jagex's crappy API</returns>
        public static async Task<JObject> GetFromJCA(string url)
        {
            HttpResponse<string> data;

            do
            {
                data = await Unirest.get(url).asStringAsync();
                
                if (data.Code == 404)
                    return null;

                if (data.Code == 429 || data.Body == null || data.Body.Trim().IsNullOrEmpty())
                {
                    Console.WriteLine($"Waiting... {url}");
                    await Task.Delay(4000);
                }

            } while (data.Body == null || data.Body.Trim().IsNullOrEmpty());
        
            return JsonConvert.DeserializeObject<JObject>(data.Body);
        }
    }
}
