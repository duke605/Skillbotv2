using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace SkillBotv2.Extensions
{
    static class JsonExtensions
    {
        /// <summary>
        /// Gets a dynamic type from a json object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o">The json object to pull from</param>
        /// <param name="key">The key to fetch</param>
        /// <returns>The value attached to the key</returns>
        public static T Get<T>(this JObject o, string key)
            => o.Value<T>(key);

        /// <summary>
        /// Gets the last member in a JsonObject and converts it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o">The json object to pull from</param>
        /// <returns>The last member converted to T</returns>
        public static T Last<T>(this JObject o)
            => o.Last.Value<T>();
    }
}
