using System;
using System.Globalization;

namespace Example.EclProvider.Extensions
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
        public static DateTime? DateTimeFromUnixTimeStamp(this string unixTimeStamp)
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
        public static DateTime? ExactDateTime(this string timestamp)
        {
            if (string.IsNullOrEmpty(timestamp))
            {
                return null;
            }

            return DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Determines whether this version string and a specified version string have the same major, minor and build numbers.
        /// </summary>
        /// <param name="instance">This version string instance.</param>
        /// <param name="version">The version string to compare to this instance.</param>
        /// <returns>true if the value of the version parameter has the same major, minor and build numbers as this version string, false otherwise.</returns>
        public static bool EqualsIgnoringRevision(this string instance, string version)
        {
            try
            {
                Version v1 = Version.Parse(instance);
                Version v2 = Version.Parse(version);
                return v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Build == v2.Build;
            }
            catch
            {
                return false;
            }
        }
    }
}
