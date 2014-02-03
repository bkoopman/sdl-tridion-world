using System;
using System.Globalization;

namespace EclProvider.Wufoo.Extensions
{
    /// <summary>
    /// String Extension Methods
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Convert a Unix timestamp string into a DateTime object.
        /// </summary>
        /// <param name="unixTimeStamp">Seconds past epoch as a string</param>
        /// <returns>DateTime or null if timestamp was null or empty string</returns>
        public static DateTime? UnixDateTime(this string unixTimeStamp)
        {
            if (string.IsNullOrEmpty(unixTimeStamp))
            {
                return null;
            }

            // Unix timestamp is seconds past epoch
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dt = dt.AddSeconds(Convert.ToDouble(unixTimeStamp)).ToLocalTime();
            return dt;
        }

        /// <summary>
        /// Convert a timestamp string into a DateTime object.
        /// Expected format is "yyyy-MM-dd HH:mm:ss".
        /// </summary>
        /// <param name="timestamp">timestamp string with format "yyyy-MM-dd HH:mm:ss"</param>
        /// <returns>DateTime or null if timestamp was null or empty string</returns>
        public static DateTime? UniversalDateTime(this string timestamp)
        {
            if (string.IsNullOrEmpty(timestamp))
            {
                return null;
            }

            return DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
