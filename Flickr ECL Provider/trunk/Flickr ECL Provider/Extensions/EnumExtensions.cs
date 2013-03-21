using System;
using System.ComponentModel;
using System.Reflection;

namespace Example.EclProvider.Extensions
{
    /// <summary>
    /// Enum Extension Methods
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieve the description based on an Enum.
        /// </summary>
        /// <param name="enumeration">The Enum</param>
        /// <returns>Description or Enum.ToString()</returns>
        public static string Description(this Enum enumeration)
        {
            FieldInfo fi = enumeration.GetType().GetField(enumeration.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            return enumeration.ToString();
        }
    }
}
