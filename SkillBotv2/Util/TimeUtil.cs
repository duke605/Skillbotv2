using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkillBotv2.Util
{
    class TimeUtil
    {
        public static readonly long Epoch = 1475401590704;
        public static byte Increment = 0;
        public static string Last = "";

        public static DateTime UnixEpoch()
            => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <returns>The current time in miliseconds</returns>
        public static long GetCurrentTimeMillis()
            => (long) DateTime.UtcNow.Subtract(UnixEpoch()).TotalMilliseconds;

        /// <returns>Time in milliseconds past epoch</returns>
        public static long MillisSinceEpoch()
            => GetCurrentTimeMillis() - Epoch;

        /// <summary>
        /// Gets the created time of the snowflake
        /// </summary>
        /// <param name="snowflake">The snowflake to get the created date from</param>
        /// <returns>The creation date of the snowflake</returns>
        public static DateTime FromSnowflake(long snowflake)
        {
            string binarySnowflake = Convert.ToString(snowflake, 2).PadLeft(63, '0');
            string binaryTime = binarySnowflake.Substring(0, 41);
            ulong time = Convert.ToUInt64(binaryTime, 2);
            return UnixEpoch()
                .AddMilliseconds(Epoch)
                .AddMilliseconds(time)
                .ToUniversalTime();
        }

        /// <summary>
        /// Generates a snowflake
        /// </summary>
        /// <param name="v1">5 or less bits as a number</param>
        /// <param name="v2">5 or less bits as a number</param>
        /// <returns>The snow flake</returns>
        public static ulong GenerateSnowflake(byte v1, ushort v2)
        {
            string time = Convert.ToString(GetCurrentTimeMillis() - Epoch, 2);
            string tableId = Convert.ToString(v1, 2);
            string uid = Convert.ToString(v2, 2);
            string id;

            lock (Last)
            {
                // Getting or putting and getting the id
                if (Last != time)
                {
                    Increment = 0;
                    Last = time;
                }

                id = Convert.ToString(Increment++, 2);
            }

            uid = uid.PadLeft(12, '0');
            id = id.PadLeft(5, '0');
            tableId = id.PadLeft(5, '0');

            return Convert.ToUInt64(time + id + tableId + uid, 2);
        }

        public static DateTime FromUnixTime(ulong timestamp)
            => UnixEpoch().AddSeconds(timestamp);
    }
}
