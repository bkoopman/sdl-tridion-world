using System.Xml;
using Tridion.Web.UI;
using Tridion.Web.UI.Core;

namespace Social.Common
{
    public class Configuration
    {
        public static string GetConfigString(string configItem) {
            XmlDocument customConfiguration = ConfigurationManager.Models["Social.Model"].CustomXml;
            XmlNamespaceManager ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace("c", Constants.EDITOR_CONFIG_NAMESPACE);
            XmlNode node = customConfiguration.SelectSingleNode("/c:customconfiguration/c:clientconfiguration/c:" + configItem, ns);
            string configValue = node != null ? node.InnerText : "";

            return configValue;
        }
    }
}
