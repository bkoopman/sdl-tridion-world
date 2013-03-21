using System;
using System.Globalization;

namespace Example.EclProvider.Extensions
{
    /// <summary>
    /// DateTime Extension Methods
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Create ETag value from timestamp.
        /// </summary>
        /// <param name="dateTime">the DateTime object</param>
        /// <returns>ETag "yyyyMMddHHmmss"</returns>
        public static string ETag(this DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }
    }
}
