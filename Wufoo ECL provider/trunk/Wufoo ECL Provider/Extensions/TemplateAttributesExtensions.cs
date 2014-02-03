using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tridion.ExternalContentLibrary.V2;

namespace EclProvider.Wufoo.Extensions
{
    /// <summary>
    /// IEnumerable&lt;ITemplateAttribute&gt; Extension methods
    /// </summary>
    public static class TemplateAttributesExtensions
    {
        /// <summary>
        /// Get attribute value from template attribute collection using its local name.
        /// </summary>
        /// <param name="attributes">Template attributes collection</param>
        /// <param name="localName">local name of attribute</param>
        /// <returns>Template attribute value</returns>
        public static string Attribute(this IList<ITemplateAttribute> attributes, string localName)
        {
            if (attributes != null)
            {
                localName = localName.ToLowerInvariant();

                // ignoring the namespace will make this easier to use from non xhtml compliant templates
                return (from att in attributes
                        let attName = att.Name.ToLowerInvariant()
                        where attName == localName || attName.EndsWith(":" + localName)
                        select att.Value).FirstOrDefault();
            }
            return string.Empty;
        }

        /// <summary>
        /// Extract supported attributes from template attribute collection based on a collection of supported attributes names.
        /// </summary>
        /// <param name="attributes">Template attributes collection</param>
        /// <param name="supportedAttributeNames">collection of supported attribute names</param>
        /// <returns>The supported attributes with their values as a string</returns>
        public static string SupportedAttributes(this IList<ITemplateAttribute> attributes, IEnumerable<string> supportedAttributeNames)
        {
            if (attributes != null)
            {
                IEnumerable<string> attributeStrings = from supportedAttributeName in supportedAttributeNames
                                                       let value = attributes.Attribute(supportedAttributeName)
                                                       where !string.IsNullOrWhiteSpace(value)
                                                       select string.Format("{0}=\"{1}\"", supportedAttributeName, HttpUtility.HtmlEncode(value));
                return string.Join(" ", attributeStrings);
            }
            return string.Empty;
        }
    }
}
