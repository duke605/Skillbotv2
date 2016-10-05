using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SkillBotv2
{
    partial class item
    {
        /// <summary>
        /// Parses a lua object into an item
        /// </summary>
        /// <param name="s">The lua object represented as a string</param>
        /// <returns>the item parse from the lua object</returns>
        public static item ValueOf(string s)
        {
            string namePattern = @"item\s+=\s+'(.+?)(?<!\\)'";
            string pricePattern = @"price\s+=\s+(\d+?),";
            string idPattern = @"itemId\s+=\s+(\d+?),";

            return new item
            {
                Name = Regex.Match(s, namePattern).Groups[1].Value,
                Price = int.Parse(Regex.Match(s, pricePattern).Groups[1].Value),
                Id = ulong.Parse(Regex.Match(s, idPattern).Groups[1].Value),
                UpdatedAt = DateTime.Now
            };
        }
    }
}
