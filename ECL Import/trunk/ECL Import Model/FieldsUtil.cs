using System;
using System.Collections.Generic;
using System.Linq;
using Tridion.ContentManager.CoreService.Client;
using System.Xml;
using System.Collections;

namespace EclImport.Model
{
    /// <summary>
    /// A wrapper around the content or metadata fields of a Tridion item.
    /// </summary>
    public class Fields
    {
        private readonly ItemFieldDefinitionData[] _definitions;
        private readonly XmlNamespaceManager _namespaceManager;
        private readonly XmlElement _root; // the root element under which these fields live

        // at any point EITHER data OR parent has a value
        private readonly SchemaFieldsData _data; // the schema fields data as retrieved from the core service
        private readonly Fields _parent; // the parent fields (so we're an embedded schema), where we can find the data

        public Fields(SchemaFieldsData data, ItemFieldDefinitionData[] definitions, string content = null, string rootElementName = null)
        {
            _data = data;
            _definitions = definitions;
            var doc = new XmlDocument();
            if (!string.IsNullOrEmpty(content))
            {
                doc.LoadXml(content);
            }
            else
            {
                doc.AppendChild(doc.CreateElement(string.IsNullOrEmpty(rootElementName) ? data.RootElementName : rootElementName, data.NamespaceUri));
            }
            _root = doc.DocumentElement;
            _namespaceManager = new XmlNamespaceManager(doc.NameTable);
            _namespaceManager.AddNamespace("custom", data.NamespaceUri);
        }

        public Fields(Fields parent, ItemFieldDefinitionData[] definitions, XmlElement root)
        {
            _definitions = definitions;
            _parent = parent;
            _root = root;
        }

        public static Fields ForContentOf(SchemaFieldsData data)
        {
            return new Fields(data, data.Fields);
        }

        public static Fields ForContentOf(SchemaFieldsData data, ComponentData component)
        {
            return new Fields(data, data.Fields, component.Content);
        }

        public static Fields ForMetadataOf(SchemaFieldsData data, RepositoryLocalObjectData item)
        {
            return new Fields(data, data.MetadataFields, item.Metadata, "Metadata");
        }

        public string NamespaceUri
        {
            get { return _data != null ? _data.NamespaceUri : _parent.NamespaceUri; }
        }
        public XmlNamespaceManager NamespaceManager
        {
            get { return _parent != null ? _parent._namespaceManager : _namespaceManager; }
        }

        internal IEnumerable<XmlElement> GetFieldElements(ItemFieldDefinitionData definition)
        {
            return _root.SelectNodes("custom:" + definition.Name, NamespaceManager).OfType<XmlElement>();
        }

        internal XmlElement AddFieldElement(ItemFieldDefinitionData definition)
        {
            var newElement = _root.OwnerDocument.CreateElement(definition.Name, NamespaceUri);

            XmlNodeList nodes = _root.SelectNodes("custom:" + definition.Name, NamespaceManager);
            XmlElement referenceElement = null;
            if (nodes.Count > 0)
            {
                referenceElement = (XmlElement)nodes[nodes.Count - 1];
            }
            else
            {
                // this is the first value for this field, find its position in the XML based on the field order in the schema
                bool foundUs = false;
                for (int i = _definitions.Length - 1; i >= 0; i--)
                {
                    if (!foundUs)
                    {
                        if (_definitions[i].Name == definition.Name)
                        {
                            foundUs = true;
                        }
                    }
                    else
                    {
                        var values = GetFieldElements(_definitions[i]);
                        if (values.Any())
                        {
                            referenceElement = values.Last();
                            break; // from for loop
                        }
                    }
                } // for every definition in reverse order
            } // no existing values found
            _root.InsertAfter(newElement, referenceElement); // if referenceElement is null, will insert as first child
            return newElement;
        }

        public IEnumerator<Field> GetEnumerator()
        {
            return new FieldEnumerator(this, _definitions);
        }

        public Field this[string name]
        {
            get
            {
                var definition = _definitions.First(ifdd => ifdd.Name == name);
                if (definition == null) throw new ArgumentOutOfRangeException("Unknown field '" + name + "'");
                return new Field(this, definition);
            }
        }

        public override string ToString()
        {
            return _root.OuterXml;
        }

    }

    public class FieldEnumerator : IEnumerator<Field>
    {
        private readonly Fields _fields;
        private readonly ItemFieldDefinitionData[] _definitions;

        // Enumerators are positioned before the first element until the first MoveNext() call
        int _position = -1;

        public FieldEnumerator(Fields fields, ItemFieldDefinitionData[] definitions)
        {
            _fields = fields;
            _definitions = definitions;
        }

        public bool MoveNext()
        {
            _position++;
            return (_position < _definitions.Length);
        }

        public void Reset()
        {
            _position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public Field Current
        {
            get
            {
                try
                {
                    return new Field(_fields, _definitions[_position]);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public void Dispose()
        {
        }
    }

    public class Field
    {
        private readonly Fields _fields;
        private readonly ItemFieldDefinitionData _definition;

        public Field(Fields fields, ItemFieldDefinitionData definition)
        {
            _fields = fields;
            _definition = definition;
        }

        public string Name
        {
            get { return _definition.Name; }
        }
        public Type Type
        {
            get { return _definition.GetType(); }
        }
        public string Value
        {
            get
            {
                return Values.Count > 0 ? Values[0] : null;
            }
            set
            {
                if (Values.Count == 0) _fields.AddFieldElement(_definition);
                Values[0] = value;
            }
        }
        public ValueCollection Values
        {
            get
            {
                return new ValueCollection(_fields, _definition);
            }
        }

        public void AddValue(string value = null)
        {
            XmlElement newElement = _fields.AddFieldElement(_definition);
            if (value != null) newElement.InnerText = value;
        }

        public void RemoveValue(string value)
        {
            var elements = _fields.GetFieldElements(_definition);
            foreach (var element in elements)
            {
                if (element.InnerText == value)
                {
                    element.ParentNode.RemoveChild(element);
                }
            }
        }

        public void RemoveValue(int i)
        {
            var elements = _fields.GetFieldElements(_definition).ToArray();
            elements[i].ParentNode.RemoveChild(elements[i]);
        }

        public IEnumerable<Fields> SubFields
        {
            get
            {
                var embeddedFieldDefinition = _definition as EmbeddedSchemaFieldDefinitionData;
                if (embeddedFieldDefinition != null)
                {
                    var elements = _fields.GetFieldElements(_definition);
                    foreach (var element in elements)
                    {
                        yield return new Fields(_fields, embeddedFieldDefinition.EmbeddedFields, element);
                    }
                }
            }
        }

        public Fields GetSubFields(int i = 0)
        {
            var embeddedFieldDefinition = _definition as EmbeddedSchemaFieldDefinitionData;
            if (embeddedFieldDefinition != null)
            {
                var elements = _fields.GetFieldElements(_definition);
                if (i == 0 && !elements.Any())
                {
                    // you can always set the first value of any field without calling AddValue, so same applies to embedded fields
                    AddValue();
                    elements = _fields.GetFieldElements(_definition);
                }
                return new Fields(_fields, embeddedFieldDefinition.EmbeddedFields, elements.ToArray()[i]);
            }

            throw new InvalidOperationException("You can only GetSubField on an EmbeddedSchemaField");
        }

        // The subfield with the given name of this field
        public Field this[string name]
        {
            get { return GetSubFields()[name]; }
        }

        // The subfields of the given value of this field
        public Fields this[int i]
        {
            get { return GetSubFields(i); }
        }

    }

    public class ValueCollection
    {
        private readonly Fields _fields;
        private readonly ItemFieldDefinitionData _definition;

        public ValueCollection(Fields fields, ItemFieldDefinitionData definition)
        {
            _fields = fields;
            _definition = definition;
        }

        public int Count
        {
            get { return _fields.GetFieldElements(_definition).Count(); }
        }

        public bool IsLinkField
        {
            get { return _definition is ComponentLinkFieldDefinitionData || _definition is ExternalLinkFieldDefinitionData || _definition is MultimediaLinkFieldDefinitionData; }
        }

        public bool IsRichTextField
        {
            get { return _definition is XhtmlFieldDefinitionData; }
        }

        public string this[int i]
        {
            get
            {
                XmlElement[] elements = _fields.GetFieldElements(_definition).ToArray();
                if (i >= elements.Length)
                {
                    throw new IndexOutOfRangeException();
                }
                return IsLinkField ? elements[i].Attributes["xlink:href"].Value : elements[i].InnerXml;
            }
            set
            {
                XmlElement[] elements = _fields.GetFieldElements(_definition).ToArray();
                if (i >= elements.Length) throw new IndexOutOfRangeException();
                if (IsLinkField)
                {
                    elements[i].SetAttribute("href", "http://www.w3.org/1999/xlink", value);
                    elements[i].SetAttribute("type", "http://www.w3.org/1999/xlink", "simple");
                    // TODO: should we clear the title for MMCLink and CLink fields? They will automatically be updated when we save the xlink:href.
                }
                else
                {
                    if (IsRichTextField)
                    {
                        elements[i].InnerXml = value;
                    }
                    else
                    {
                        elements[i].InnerText = value;
                    }
                }
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _fields.GetFieldElements(_definition).Select(elm => IsLinkField ? elm.Attributes["xlink:href"].Value : elm.InnerXml.ToString()).GetEnumerator();
        }
    }
}
