using System.Xml.Linq;

namespace Example.EclProvider.Extensions
{
    /// <summary>
    /// Linq XML Extension Methods
    /// </summary>
    public static class LinqXmlExtensions
    {
        /// <summary>
        /// Allows a safe way to retrieve attribute values from an element
        /// </summary>
        /// <param name="element">A reference to the element object</param>
        /// <param name="attributeName">The name of the attribute</param>
        /// <returns>The attribute content or null</returns>
        public static string AttributeValueOrDefault(this XElement element, string attributeName)
        {
            XAttribute attr = null;
            if (element != null)
            {
                attr = element.Attribute(attributeName);
            }

            return attr == null ? null : attr.Value;
        }

        /// <summary>
        /// Allows a safe way to retrieve element data
        /// </summary>
        /// <param name="element">A reference to the element object</param>
        /// <returns>Element content or an empty string</returns>
        public static string ElementValueNull(this XElement element)
        {
            if (element != null)
            {
                return element.Value;
            }

            return string.Empty;
        }
    }
}
