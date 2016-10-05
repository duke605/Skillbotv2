using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkillBotv2.Extensions
{
    static class PrimitveExtentions
    {
        public static bool Matches(this string s, string pattern, RegexOptions options)
            => Regex.IsMatch(s, pattern, options);

        /// <summary>
        /// Transforms a string into sentence case
        /// </summary>
        /// <param name="s">The string to be transformed</param>
        /// <returns>the string in sentence case</returns>
        public static string ToSentenceCase(this string s)
            => char.ToUpper(s[0]) + s.Substring(1).ToLower();

        /// <summary>
        /// Loops through a list and passes them the index
        /// </summary>
        /// <param name="s">The source</param>
        /// <param name="a">The action called for each element in the list</param>
        public static void ForEachWithIndex<T>(this IEnumerable<T> s, Action<int, T> a)
        {
            for (var i = 0; i < s.Count(); i++)
                a(i, s.ElementAt(i));
        }

        /// <summary>
        /// Centers the string
        /// </summary>
        /// <param name="s">The source</param>
        /// <param name="width">The width to center relative to</param>
        /// <param name="padChar">The character to pad the string with</param>
        /// <returns>The centered string</returns>
        public static string Center(this string s, int width, char padChar = ' ')
        {
            if (s.Length >= width)
            {
                return s;
            }

            int leftPadding = (width - s.Length) / 2;
            int rightPadding = width - s.Length - leftPadding;

            return new string(padChar, leftPadding) + s + new string(padChar, rightPadding);
        }

        /// <summary>
        /// Formats hours into readable string
        /// </summary>
        /// <param name="time">The number of hours as a double</param>
        /// <returns>The readable time string</returns>
        public static string ToFormattedTime(this double time)
        {
            var days  = time/24; 
            var hours = days%1*24;
            var mins  = hours%1*60;
            var secs  = mins%1*60;

            return $"{days.Floor().ToString("#,##0")}:" +
                   $"{hours.Floor().ToString("00")}:" +
                   $"{mins.Floor().ToString("00")}:" +
                   $"{secs.Round().ToString("00")}";
        }

        /// <summary>
        /// Floors the input
        /// </summary>
        /// <param name="s">The input to floor</param>
        /// <returns>The floored input</returns>
        public static double Floor(this double s)
            => Math.Floor(s);

        /// <summary>
        /// Rounds the input to the nearest whole number
        /// </summary>
        /// <param name="s">The number to round</param>
        /// <returns>The rounded number</returns>
        public static double Round(this double s)
            => Math.Round(s);
    }
}
