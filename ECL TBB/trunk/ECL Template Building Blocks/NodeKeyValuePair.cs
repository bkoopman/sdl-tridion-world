using System.Collections.Generic;
using System.Xml;
using Tridion.ExternalContentLibrary.V2;

namespace Example.Ecl.TBBs
{
    internal class NodeKeyValuePair : ITemplateAttribute
    {
        private readonly string _name;
        private readonly string _namespaceUri;
        private readonly string _value;

        public NodeKeyValuePair(XmlNode xmlNode)
        {
            _name = xmlNode.LocalName;
            _namespaceUri = xmlNode.NamespaceURI;

            if (xmlNode is XmlAttribute)
            {
                _value = xmlNode.Value;
            }
            if (xmlNode is XmlElement)
            {
                _value = xmlNode.InnerText;
            }
        }

        public NodeKeyValuePair(KeyValuePair<string, string> pair)
        {
            _name = pair.Key;
            _value = pair.Value;
        }

        public string Name
        {
            get { return _name; }
        }

        public string NamespaceUri
        {
            get { return _namespaceUri; }
        }

        public string Value
        {
            get { return _value; }
        }
    }
}
