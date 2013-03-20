using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing.Rendering;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ExternalContentLibrary.V2;

// set assembly template name
[assembly: TcmTemplateTitle("ECL assembly")]

namespace Example.Ecl.TBBs
{
    /// <summary>
    /// Resolve and publish (if required) ECL items in the package
    /// </summary>
    [TcmDefaultTemplate]
    [TcmTemplateTitle("Resolve ECL items")]
    [TcmTemplateParameterSchema("resource:Example.Ecl.TBBs.Resources.ResolveEclItemsParameters.xsd")]
    public class ResolveEclItems : ITemplate
    {
        // ECL content type
        private const string EclContentType = "application/externalcontentlibrary";

        // configuration parameters
        private const string ParameterNameTargetStructureGroup = "sg_TargetStructureGroup";
        private const string ParameterNameItemName = "ItemName";

        private static readonly TemplatingLogger Log = TemplatingLogger.GetLogger(typeof(ResolveEclItems));

        #region attributes regular expressions
        //  - <img|a ... tridion:href="..." ...>
        private static readonly Regex TridionHrefExpression = new Regex(@"(?<tagwithlink><(?<tagname>[^\s>]+)[^>]+tridion:href[^>]+?>)", RegexOptions.Multiline | RegexOptions.Compiled);

        //  - tridion:*="*" expression
        private static readonly Regex QuotedTridionAttributeExpression = new Regex(@"[\s]+tridion:(?<name>[^\s=]+)\s*=\s*((\""(?<value>[^""]+)\"")|('(?<value>[^']+)'))", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex QuotedAttributeExpression = new Regex(@"[\s]+(?<name>[^\s=]+)\s*=\s*((\""(?<value>[^""]+)\"")|('(?<value>[^']+)'))", RegexOptions.Multiline | RegexOptions.Compiled);

        //  - attrName='*' expression, case insensitive, formatstring, actually attribute name must be filled out
        private const string FixedAttributeExpressionString = @"[\s]+(?<name>{0})\s*=\s*((\""(?<value>[^\""]+)\"")|('(?<value>[^']+)')|(?<url>[^\s\>]+))";

        //  - attrName='href' expression, case insensitive, using FixedAttributeExpressionString with href filled out
        private static readonly Regex FixedHrefAttributeExpression = new Regex(String.Format(FixedAttributeExpressionString, "href"), RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        // This expression currently is run both on full HTML documents as well as on just script parts
        // (CSS url reference does not fully check whether reference is in a CSS fragement, performs replacement on all { ... url(["|']ecl:...["|']);...} fragments
        private static readonly Regex EclUriCssUrlExpression = new Regex(@"{[^}]+url\s*\([^""']*[""|'](?<ecluri>ecl:[^""']+)[""|']\s*\)\s*;?.*}", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        #endregion
        #region tag attributes
        /// <summary>
        /// The href attribute (used in created tridion hyperlinks)
        /// </summary>
        private const string AttributeNameTridionHref = "href";

        /// <summary>
        /// The type attribute, used to indicate the type of tridion hyperlink
        /// </summary>
        private const string AttributeNameTridionType = "type";

        /// <summary>
        /// type attribute value for component-links
        /// </summary>
        private const string AttributeTridionTypeComponent = "Component";

        /// <summary>
        /// type attribute value for page-links
        /// </summary>
        private const string AttributeTridionTypePage = "Page";

        /// <summary>
        /// type attribute value for binary-links
        /// </summary>
        private const string AttributeTridionTypeMultimedia = "Multimedia";

        /// <summary>
        /// Attribute name for the attribute the eventual link value should be placed in
        /// </summary>
        private const string AttributeNameTargetAttribute = "targetattribute";

        /// <summary>
        /// Attribute name for the attribute with the anchor to use in a link.
        /// </summary>
        private const string AttributeNameTridionAnchor = "anchor";
        #endregion

        // keep some member variables to prevent passing around too many arguments between methods
        private Package _package;
        private Engine _engine;
        private int _publicationId;
        private StructureGroup _targetStructureGroup;
        private Dictionary<TcmUri, EclItemInPackage> _eclItemsInPackage;

        // Because methods alter this text-variable as it is processed, keep it outside of the method. 
        // The text is changed in the variable as the processing is executed.
        private string _text;

        public void Transform(Engine engine, Package package)
        {
            _engine = engine;
            _package = package;

            // determine context publication id
            if (_package.GetByType(ContentType.Page) != null)
            {
                // assuming page in package is current item
                _publicationId = _engine.GetObject(_package.GetByType(ContentType.Page).GetAsSource().GetValue("ID")).Id.PublicationId;
            }
            else
            {
                // assuming component in package is current item
                _publicationId = _engine.GetObject(_package.GetByType(ContentType.Component).GetAsSource().GetValue("ID")).Id.PublicationId;
            }

            // determine (optional) structure group parameter
            Item targetStructureGroupItem = _package.GetByName(ParameterNameTargetStructureGroup);
            if (targetStructureGroupItem != null)
            {
                if (!TcmUri.IsValid(targetStructureGroupItem.GetAsString()))
                {
                    throw new InvalidDataException(string.Format("Target location {0} is not a structure group", targetStructureGroupItem));
                }
                TcmUri uri = new TcmUri(targetStructureGroupItem.GetAsString());
                // get target structure group in context publication
                _targetStructureGroup = (StructureGroup)_engine.GetSession().GetObject(new TcmUri(uri.ItemId, uri.ItemType, _publicationId));
                Log.Debug(string.Format("Target Structure Group {0} found", _targetStructureGroup.Id));
            }

            // make sure eclSession is disposed at the end
            using (IEclSession eclSession = SessionFactory.CreateEclSession(engine.GetSession()))
            {
                // loop over all ecl items and store them in a dictionary
                _eclItemsInPackage = new Dictionary<TcmUri, EclItemInPackage>();
                foreach (Item item in _package.GetAllByType(new ContentType(EclContentType)))
                {
                    if (item.Properties.ContainsKey(Item.ItemPropertyTcmUri) && item.Properties.ContainsKey(Item.ItemPropertyFileName))
                    {
                        // has the item already been published?
                        if (!item.Properties.ContainsKey(Item.ItemPropertyPublishedPath))
                        {
                            // find ecl items
                            string uri;
                            if (item.Properties.TryGetValue(Item.ItemPropertyTcmUri, out uri))
                            {
                                TcmUri stubUri = new TcmUri(uri);
                                IEclUri eclUri = eclSession.TryGetEclUriFromTcmUri(stubUri);
                                if (eclUri != null)
                                {
                                    // we have a valid ecl item, lets store it in the dictionary
                                    IContentLibraryMultimediaItem eclItem = (IContentLibraryMultimediaItem)eclSession.GetContentLibrary(eclUri).GetItem(eclUri);
                                    EclItemInPackage eclItemInPackage = new EclItemInPackage(item, eclItem);
                                    _eclItemsInPackage.Add(stubUri, eclItemInPackage);
                                }
                            }
                        }
                        else
                        {
                            // items have been published, we are probabaly called too late...
                            Log.Warning("{0} has already been published, \"Resolve ECL items\" should be added before \"Publish Binaries in Package\"");
                        }
                    }
                }

                // end processing if there are no ecl items found in the package
                if (_eclItemsInPackage.Count == 0) return;

                // determine item name to operate on from a parameter or use default
                string itemName = _package.GetValue(ParameterNameItemName);
                if (string.IsNullOrEmpty(itemName))
                {
                    itemName = Package.OutputName;
                }
                Item selectedItem = _package.GetByName(itemName);
                // continue on the output by matching the found ecl items in there against the list of ecl items in the package
                if ((selectedItem.Type == PackageItemType.String) || (selectedItem.Type == PackageItemType.Stream))
                {
                    // Assume text content
                    string inputToConvert = selectedItem.GetAsString();
                    string convertedOutput = ResolveEclLinks(inputToConvert);
                    if (convertedOutput != null)
                    {
                        Log.Debug("Changed Output item");
                        selectedItem.SetAsString(convertedOutput);
                    }
                }
                else
                {
                    // XML
                    XmlDocument outputDocument = selectedItem.GetAsXmlDocument();
                    ResolveEclLinks(outputDocument);
                    selectedItem.SetAsXmlDocument(outputDocument);
                }
            }
        }

        /// <summary>
        /// Resolve and possibly publish an ECL item
        /// </summary>
        /// <param name="eclItemInPackage">EclItemInPackage object</param>
        /// <param name="attributes">template atrributes</param>
        /// <param name="variantId">variant id</param>
        /// <param name="targetStructureGroup">target Structure Group to publish too (when null, Image Structure Group from Publication will be used</param>
        /// <param name="targetAttribute">name of target attribute in tag (usually src or href)</param>
        /// <param name="publishPath">return template fragment or publish path</param>
        /// <returns>template fragment for (published) ECL item or publish path when publishPath = true</returns>
        private string ResolveEclItem(EclItemInPackage eclItemInPackage, IList<ITemplateAttribute> attributes, string variantId, StructureGroup targetStructureGroup, string targetAttribute, bool publishPath = false)
        {
            // determine if item is already published or if we should get the content for it and let tridion publish it
            string publishedPath = eclItemInPackage.EclItem.GetDirectLinkToPublished(attributes);

            if (string.IsNullOrEmpty(publishedPath))
            {
                // tridion must publish this ecl item as a variant 
                Component component = (Component)_engine.GetSession().GetObject(eclItemInPackage.StubUri);
                // get the content and create a filename with size and a proper extension
                string filename = ConstructFileName(eclItemInPackage.EclItem.Filename, variantId, eclItemInPackage.EclUri.ToString());
                IContentResult content = eclItemInPackage.EclItem.GetContent(attributes);
                Binary publishedEclItem;

                if (targetStructureGroup == null)
                {
                    Log.Info(string.Format("publishing ECL item of type {0}", content.ContentType));
                    if (!content.Stream.CanSeek)
                    {
                        // create a seekable copy of the content stream
                        using (MemoryStream stream = new MemoryStream())
                        {
                            content.Stream.CopyTo(stream);
                            publishedEclItem = _engine.PublishingContext.RenderedItem.AddBinary(stream, filename, variantId, component, content.ContentType);
                        }
                    }
                    else
                    {
                        publishedEclItem = _engine.PublishingContext.RenderedItem.AddBinary(content.Stream, filename, variantId, component, content.ContentType);
                    }
                }
                else
                {
                    Log.Info(string.Format("publishing ECL item of type {0} to Structure Group: {1}", content.ContentType, targetStructureGroup.Id));
                    if (!content.Stream.CanSeek)
                    {
                        // create a seekable copy of the content stream
                        using (MemoryStream stream = new MemoryStream())
                        {
                            content.Stream.CopyTo(stream);
                            publishedEclItem = _engine.PublishingContext.RenderedItem.AddBinary(stream, filename, targetStructureGroup, variantId, component, content.ContentType);
                        }
                    }
                    else
                    {
                        publishedEclItem = _engine.PublishingContext.RenderedItem.AddBinary(content.Stream, filename, targetStructureGroup, variantId, component, content.ContentType);
                    }
                }
                // we could consider updating the package item with the content and the file extension, but to change the content type, we have to recreate it
                // this won't be of much use other than getting the content stream twice (performance impact) as the package item will (or should) not be used after this anymore
                //eclItemInPackage.PackageItem.SetAsStream(content.Stream);
                //eclItemInPackage.PackageItem.Properties[Item.ItemPropertyFileNameExtension] = DetermineExtension(eclItemInPackage.MimeType);

                // set published path in package item and add to attributes
                eclItemInPackage.PackageItem.Properties[Item.ItemPropertyPublishedPath] = publishedEclItem.Url;
                attributes.Add(new NodeKeyValuePair(new KeyValuePair<string, string>(targetAttribute, publishedEclItem.Url)));
            }
            else
            {
                // ecl item is already published, lets set the uri that can be used from a public website to link directly to the item on the external system
                eclItemInPackage.PackageItem.Properties[Item.ItemPropertyPublishedPath] = publishedPath;
                Log.Info(string.Format("resolved ECL item with path {0}", publishedPath));
            }

            if (publishPath)
            {
                return publishedPath;
            }

            // return the template fragment which should replace the original one in the Output
            return eclItemInPackage.EclItem.GetTemplateFragment(attributes);
        }

        /// <summary>
        /// Construct a filename based on input parameters.
        /// </summary>
        /// <param name="originalFilename">the original filename with extension</param>
        /// <param name="variantId">the variant id</param>
        /// <param name="eclUri">the ECLURI as a string</param>
        /// <returns>filename with extension extended with ECLURI and variant id</returns>
        private static string ConstructFileName(string originalFilename, string variantId, string eclUri)
        {
            // set defaults
            string extension = "bin";
            eclUri = eclUri.Replace("ecl:", string.Empty);

            // determine filename and extension
            if (!string.IsNullOrEmpty(originalFilename) && originalFilename.Contains("."))
            {
                if (!string.IsNullOrEmpty(Path.GetExtension(originalFilename)))
                {
                    extension = Path.GetExtension(originalFilename).Substring(1);
                }
                originalFilename = Path.GetFileNameWithoutExtension(originalFilename);
            }

            // build filename for empty variant
            if (string.IsNullOrEmpty(variantId) || variantId.Equals("x"))
            {
                if (string.IsNullOrEmpty(originalFilename))
                {
                    return string.Format("{0}.{1}", eclUri, extension);
                }
                return string.Format("{0}_{1}.{2}", originalFilename, eclUri, extension);
            }

            // build filename for empty original filename
            if (string.IsNullOrEmpty(originalFilename))
            {
                return string.Format("{0}_{1}.{2}", eclUri, variantId, extension);
            }

            // build full filename
            return string.Format("{0}_{1}_{2}.{3}", originalFilename, eclUri, variantId, extension);
        }

        /// <summary>
        /// Perform the actual function of this template, by iterating over all tags with tridion:href attributes in them and see if they belong to an ECL item.
        /// </summary>
        /// <param name="text">The text to process</param>
        /// <returns>The processed text (also stored in the member field)</returns>
        private string ResolveEclLinks(string text)
        {
            _text = text;
            string[] outputContainer = new[] { _text };
            foreach (Match matchingElement in TemplateUtilities.GetRegexMatches(outputContainer, TridionHrefExpression))
            {
                Log.Debug("Processing ECL link text");
                outputContainer[0] = ProcessEclLink(matchingElement);
            }

            // Process CSS references
            _text = ReplaceCssReferencesInText(_text);
            return _text;
        }

        /// <summary>
        /// Perform the actual function of this template, by iterating over all tags with tridion:href attributes in them and see if they belong to an ECL item.
        /// </summary>
        /// <param name="document">The template document to process</param>
        private void ResolveEclLinks(XmlNode document)
        {
            XmlNodeList foundLinkNodes = TemplateUtilities.SelectNodes(document, "//*[@tridion:href]");
            foreach (XmlNode foundLinkNode in foundLinkNodes)
            {
                Log.Debug(string.Format("Found link node: {0}", foundLinkNode.Name));
                ParsedLinkInfo linkInfo = ParseLinkInfo(foundLinkNode);

                // check if link belongs to ECL item in package
                EclItemInPackage item;
                if (_eclItemsInPackage.TryGetValue(linkInfo.TargetUri, out item))
                {
                    // parse remaining attributes to determine width and height 
                    IDictionary<string, string> remainingAttributes = new Dictionary<string, string>();
                    string width;
                    string height;
                    ParseRemainingAttributes(linkInfo.RemainingAttributes, remainingAttributes, out width, out height);

                    // convert remaining attributes into IList<ITemplateAttribute>
                    IList<ITemplateAttribute> attributes = new List<ITemplateAttribute>();
                    foreach (var attribute in remainingAttributes)
                    {
                        attributes.Add(new NodeKeyValuePair(new KeyValuePair<string, string>(attribute.Key, attribute.Value)));
                    }

                    // resolve (possibly publish) ECL item
                    string variantId = string.Format("{0}x{1}", width.Trim(), height.Trim());
                    string linkReplacementString = ResolveEclItem(item, attributes, variantId, _targetStructureGroup, linkInfo.TargetAttribute);

                    Log.Debug("Constructed link: " + linkReplacementString);
                    // replacing the link node with the constructed output text
                    ReplaceLinkNodeWithEclNode(foundLinkNode, linkReplacementString);
                }
            }

            // Process the content (can be text-nodes or comments) of style-elements, replacing ECL URI references with published paths
            XmlNodeList styleElements = TemplateUtilities.SelectNodes(document, "//style|//html:style");
            foreach (XmlNode styleElement in styleElements)
            {
                string styleContent = styleElement.InnerXml;
                styleContent = ReplaceCssReferencesInText(styleContent);
                styleElement.InnerXml = styleContent;
            }
        }

        /// <summary>
        /// Replace the link node with an ECL node (keeping children and attributes).
        /// Output will be based on the replacementString, assuming this is a valid XHTML fragment coming from IContentLibraryMultimediaItem.GetTemplateFragment() 
        /// </summary>
        /// <param name="foundLinkNode">The link-node to replace</param>
        /// <param name="replacementString">valid XHTML fragment coming from IContentLibraryMultimediaItem.GetTemplateFragment()</param>
        private static void ReplaceLinkNodeWithEclNode(XmlNode foundLinkNode, string replacementString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(replacementString);
            if (doc.DocumentElement != null)
            {
                XmlNode newElement = foundLinkNode.OwnerDocument.ImportNode(doc.DocumentElement, true);

                // Put the Ecl link node in the tree
                foundLinkNode.ParentNode.ReplaceChild(newElement, foundLinkNode);
            }
        }

        /// <summary>
        /// if tag belongs to an ECL item, replace it with the result from IContentLibraryMultimediaItem.GetTemplateFragment(attributes)
        /// The changes are made directly on the _text class member variable.
        /// </summary>
        /// <param name="matchingTag">A matching start-tag (so not the full tag) which contains at least a tridion:href attribute</param>
        /// <returns>The altered overall text</returns>
        private string ProcessEclLink(Match matchingTag)
        {
            Group foundMatch = matchingTag.Groups["tagwithlink"];
            string elementName = matchingTag.Groups["tagname"].ToString();

            int tagStartPosition;
            int tagLength;
            DetermineTagPositions(foundMatch, elementName, out tagStartPosition, out tagLength);

            // determine some more link positions
            string hyperLinkText = _text.Substring(tagStartPosition, tagLength);
            // ToDo: many text processing todo's here, but especially problematic is that no '>' is allowed in attribute values
            int endStartTag = hyperLinkText.IndexOf('>');
            if (endStartTag == -1)
            {
                throw new InvalidDataException(string.Format("Tag '{0}' is not formatted correctly", hyperLinkText));
            }

            // retrieve link information from the html tag fragment
            ParsedLinkInfo linkInfo = ParseLinkInfo(elementName, hyperLinkText, endStartTag);

            // check if link belongs to ECL item in package
            EclItemInPackage item;
            if (_eclItemsInPackage.TryGetValue(linkInfo.TargetUri, out item))
            {
                if (!string.IsNullOrEmpty(linkInfo.RemainingAttributes))
                {
                    // cleanup the xmlns:tridion attribute, if present
                    linkInfo.RemainingAttributes = linkInfo.RemainingAttributes.Replace("xmlns:tridion=&quot;http://www.tridion.com/ContentManager/5.0&quot;", string.Empty).Replace("xmlns:tridion=\"http://www.tridion.com/ContentManager/5.0\"", string.Empty);
                }

                // parse remaining attributes to determine width and height 
                IDictionary<string, string> remainingAttributes = new Dictionary<string, string>();
                string width;
                string height;
                ParseRemainingAttributes(linkInfo.RemainingAttributes, remainingAttributes, out width, out height);

                // convert remaining attributes into IList<ITemplateAttribute>
                IList<ITemplateAttribute> attributes = new List<ITemplateAttribute>();
                foreach (var attribute in remainingAttributes)
                {
                    attributes.Add(new NodeKeyValuePair(new KeyValuePair<string, string>(attribute.Key, attribute.Value)));
                }

                // resolve (possibly publish) ECL item
                string variantId = string.Format("{0}x{1}", width.Trim(), height.Trim());
                string linkReplacementString = ResolveEclItem(item, attributes, variantId, _targetStructureGroup, linkInfo.TargetAttribute);

                Log.Debug("Constructed link text: " + linkReplacementString);
                // alter the text output being processed, replacing the input link fragment by the constructed output text
                _text = _text.Substring(0, tagStartPosition) + linkReplacementString + _text.Substring(tagStartPosition + tagLength);
            }
            return _text;
        }

        /// <summary>
        /// Try to extract width and height from the supplied attributes string, returning the remaining attributes as a dictionary.
        /// Search for width and height attribute or a style attribute containing something like "width: 320px; height: 240px;".
        /// </summary>
        /// <param name="attributes">attributes</param>
        /// <param name="remainingAttributes">remaining attributes dictionary</param>
        /// <param name="width">width in pixels as a string, or string.Empty if not available</param>
        /// <param name="height">height in pixels as a string, or string.Empty if not available</param>
        private static void ParseRemainingAttributes(string attributes, IDictionary<string, string> remainingAttributes, out string width, out string height)
        {
            ParseTridionTagAttributes(QuotedAttributeExpression, attributes, remainingAttributes);
            width = string.Empty;
            height = string.Empty;
            string value;

            // try to set width from attributes
            if (remainingAttributes.TryGetValue("width", out value))
            {
                width = value.Trim();
            }
            // try to set height from attributes
            if (remainingAttributes.TryGetValue("height", out value))
            {
                height = value.Trim();
            }

            // assuming style contains something like "width: 320px; height: 240px;", we can extract the width and height from that also
            if (remainingAttributes.TryGetValue("style", out value))
            {
                // only set width if not already set
                if (string.IsNullOrEmpty(width) && !string.IsNullOrEmpty(value) && value.Contains("width"))
                {
                    width = value.Split(';').Select(t => t.Trim()).First(t => t.StartsWith("width")).Split(':')[1].Replace("px", string.Empty).Trim();
                }
                // only set height if not already set
                if (string.IsNullOrEmpty(height) && !string.IsNullOrEmpty(value) && value.Contains("height"))
                {
                    height = value.Split(';').Select(t => t.Trim()).First(t => t.StartsWith("height")).Split(':')[1].Replace("px", string.Empty).Trim();
                }
            }
        }

        /// <summary>
        /// Find all CSS URL references containing ECL URIs in an input text, and replace them with their string value.
        /// </summary>
        /// <param name="inputText">The text to process</param>
        /// <returns>The input, with CSS URL references containing ECL URIs replaced by their published paths (or error messages if they could not be resolved)</returns>
        private string ReplaceCssReferencesInText(string inputText)
        {
            string[] outputContainer = new[] { inputText };
            foreach (Match matchingUrl in TemplateUtilities.GetRegexMatches(outputContainer, EclUriCssUrlExpression))
            {
                Group eclUriGroup = matchingUrl.Groups["ecluri"];
                string eclUri = eclUriGroup.ToString(); // Regex ensures this has the right format
                Log.Debug("Processing CSS URL reference for " + eclUri);
                if (eclUri.StartsWith("ecl:"))
                {
                    // resolve into stub uri
                    using (IEclSession eclSession = SessionFactory.CreateEclSession(_engine.GetSession()))
                    {
                        eclUri = eclSession.GetOrCreateTcmUriFromEclUri(eclSession.HostServices.CreateEclUri(eclUri));
                    }
                }
                if (!TcmUri.IsValid(eclUri))
                {
                    throw new TemplatingException(string.Format("Could not process url value '{0}' into uri", eclUri));
                }
                string targetUrl = eclUri;
                EclItemInPackage item;
                if (_eclItemsInPackage.TryGetValue(new TcmUri(eclUri), out item))
                {
                    // resolve (possibly publish) ECL item
                    targetUrl = ResolveEclItem(item, new List<ITemplateAttribute>(), string.Empty, _targetStructureGroup, "url", true);
                }

                inputText = inputText.Substring(0, eclUriGroup.Index) + targetUrl + inputText.Substring(eclUriGroup.Index + eclUriGroup.Length);
                outputContainer[0] = inputText;
            }
            return inputText;
        }

        /// <summary>
        /// Determine the full tag positions of a tag. The found match only indicates
        /// the positions of the start-tag (done this way since the end-tag is optional).
        /// </summary>
        /// <param name="foundMatch">A regular expression group pointing to a start-tag within _text</param>
        /// <param name="elementName">The name of the element found</param>
        /// <param name="tagStartPosition">Result, the start of of the tag</param>
        /// <param name="tagLength">The length of the full tag, up until the (optional) matching closing tag</param>
        private void DetermineTagPositions(Group foundMatch, string elementName, out int tagStartPosition, out int tagLength)
        {
            string elementStartTagOpen = "<" + elementName;
            string elementEndTag = "</" + elementName + ">";

            tagStartPosition = foundMatch.Index;

            // Now determine the tag end position, including the close tag if there is one
            // (so maybe outside the regexp match that only matches the start)
            tagLength = foundMatch.Length;
            int nextEndTagPosition = _text.IndexOf(elementEndTag, tagStartPosition + 1, StringComparison.Ordinal);
            int nextOpenTagPosition = _text.IndexOf(elementStartTagOpen, tagStartPosition + 1, StringComparison.Ordinal);
            if (nextEndTagPosition > 0)
            {
                if ((nextOpenTagPosition == -1) || (nextOpenTagPosition > nextEndTagPosition))
                {
                    // Found a full tag
                    tagLength = nextEndTagPosition + elementEndTag.Length - tagStartPosition;
                }
            }
        }

        /// <summary>
        /// Process an expression into a value.
        /// </summary>
        /// <param name="expression">The expression/ literal to process</param>
        /// <returns>Returns a determined value. Never returns null, returns an empty string if nothing can be determined</returns>
        private string ProcessExpression(string expression)
        {
            Log.Debug("ProcessExpression: " + expression);
            const string expressionStart = "${";
            const string expressionEnd = "}";

            if (string.IsNullOrEmpty(expression))
            {
                return string.Empty;
            }

            if (expression.StartsWith(expressionStart) && expression.EndsWith(expressionEnd))
            {
                string expresssionSource = expression.Substring(expressionStart.Length, expression.Length - (expressionStart.Length + expressionEnd.Length));
                string sourceValue = _package.GetValue(expresssionSource);
                if (string.IsNullOrEmpty(sourceValue))
                {
                    sourceValue = string.Empty;
                }
                return sourceValue;
            }

            // No further parsing necessary, treat as literal
            return expression;
        }

        /// <summary>
        /// Parse a set of HTML attributes (as a string), treating attributes
        /// with a prefix 'tridion:' special. Those attributes are:
        ///  - Removed from the attributes string.
        ///  - Placed in a map.
        /// </summary>
        /// <param name="attributesString">The string to parse, should be of the format
        /// key="value" key2='value2' key3=value3 tridion:key4 = 'value 5'</param>
        /// <param name="tridionAttributes">The map in which to place tridion attributes
        /// as key value pairs. The tridion: prefix is removed from keys</param>
        /// <returns>The attributesString input, without the tridion attributes found</returns>
        private string ParseTridionTagAttributes(string attributesString, IDictionary<string, string> tridionAttributes)
        {
            attributesString = ParseTridionTagAttributes(QuotedTridionAttributeExpression, attributesString, tridionAttributes);

            // Get rid of href or other target attribute (since one will be inserted)
            Regex removeExpression = FixedHrefAttributeExpression;
            if (tridionAttributes.ContainsKey(AttributeNameTargetAttribute))
            {
                string attributeToRemove = tridionAttributes[AttributeNameTargetAttribute];
                string removeExpressionString = String.Format(FixedAttributeExpressionString, attributeToRemove);
                removeExpression = new Regex(removeExpressionString, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }
            IDictionary<string, string> dummyAttributes = new Dictionary<string, string>();
            attributesString = ParseTridionTagAttributes(removeExpression, attributesString, dummyAttributes);

            return attributesString;
        }

        /// <summary>
        /// Helper for ParseTridionTagAttributes, see its docs for more information
        /// </summary>
        /// <param name="regularExpression">The regular expression to use</param>
        /// <param name="attributesString">The string to parse, should be of the format
        /// key="value" key2='value2' key3=value3 tridion:key4 = 'value 5'</param>
        /// <param name="tridionAttributes">The map in which to place tridion attributes
        /// as key value pairs. The tridion: prefix is removed from keys</param>
        /// <returns>The attributesString input, without the tridion attributes found</returns>
        private static string ParseTridionTagAttributes(Regex regularExpression, string attributesString, IDictionary<string, string> tridionAttributes)
        {
            int currentPosition = 0;
            Match match;
            do
            {
                match = regularExpression.Match(attributesString, currentPosition);
                Log.Debug("match.Success " + match.Success + " for " + regularExpression);
                if (match.Success)
                {
                    string name = match.Groups["name"].ToString();
                    string value = match.Groups["value"].ToString();
                    Log.Debug("Found tridion attribute " + name + "='" + value + "'");
                    if (tridionAttributes.ContainsKey(name))
                    {
                        Log.Debug("Overriding duplicate attribute: " + name);
                        tridionAttributes.Remove(name);
                    }
                    tridionAttributes.Add(name, value);
                    // Remove attribute from the attributes string
                    Log.Debug("Attributes string before attribute removal: " + attributesString);
                    attributesString = attributesString.Substring(0, match.Index) + attributesString.Substring(match.Index + match.Length);
                    Log.Debug("Attributes string after attribute removal: " + attributesString);
                    currentPosition = match.Index;
                }
            }
            while (match.Success);

            return attributesString;
        }

        /// <summary>
        /// Distill all useful information of a hyperlink string, and put the found data in a ParsedLinkInfo structure
        /// </summary>
        /// <param name="elementName">The name of the element being processed</param>
        /// <param name="hyperLinkText">The full tag to parse.</param>
        /// <param name="endStartTag">Within the hyperLinkText, where end-tag ends</param>
        /// <returns>A newly created ParsedLinkInfo object with all the found information filled out</returns>
        private ParsedLinkInfo ParseLinkInfo(string elementName, string hyperLinkText, int endStartTag)
        {
            string elementStartTagOpen = "<" + elementName;
            ParsedLinkInfo linkInfo = new ParsedLinkInfo();

            // Determine hyperlink parts
            string startTag = hyperLinkText.Substring(0, endStartTag + 1);
            string startTagAttributes = hyperLinkText.Substring(elementStartTagOpen.Length, endStartTag - elementStartTagOpen.Length);

            // Process attributes
            IDictionary<string, string> tridionAttributes = new Dictionary<string, string>();
            linkInfo.RemainingAttributes = ParseTridionTagAttributes(startTagAttributes, tridionAttributes);

            // Determine target URI
            if (!tridionAttributes.ContainsKey(AttributeNameTridionHref))
            {
                throw new InvalidDataException(string.Format("Could not find attribute {1} in tag {0}", startTag, "tridion:href"));
            }
            string tridionHrefValue = tridionAttributes[AttributeNameTridionHref];
            string targetUriString = ProcessExpression(tridionHrefValue);

            // Check link URI and determine link-type
            if (targetUriString.StartsWith("ecl:"))
            {
                // resolve into stub uri
                using (IEclSession eclSession = SessionFactory.CreateEclSession(_engine.GetSession()))
                {
                    targetUriString = eclSession.GetOrCreateTcmUriFromEclUri(eclSession.HostServices.CreateEclUri(targetUriString));
                }
            }
            if (!TcmUri.IsValid(targetUriString))
            {
                throw new TemplatingException(string.Format("Could not process href value '{0}' into uri", tridionHrefValue));
            }
            linkInfo.TargetUri = new TcmUri(targetUriString);
            if (linkInfo.TargetUri.ItemType == ItemType.Page)
            {
                linkInfo.LinkType = AttributeTridionTypePage;
            }
            // Check for override of type in link itself
            if (tridionAttributes.ContainsKey(AttributeNameTridionType))
            {
                linkInfo.LinkType = tridionAttributes[AttributeNameTridionType];
            }

            // Check for anchor attribute
            if (tridionAttributes.ContainsKey(AttributeNameTridionAnchor))
            {
                string anchorValue = ProcessExpression(tridionAttributes[AttributeNameTridionAnchor]);
                if (!String.IsNullOrEmpty(anchorValue))
                {
                    linkInfo.Anchor = anchorValue;
                }
            }

            // Output attribute (only for multimedia)
            if (tridionAttributes.ContainsKey(AttributeNameTargetAttribute))
            {
                linkInfo.TargetAttribute = tridionAttributes[AttributeNameTargetAttribute];
            }

            if ((linkInfo.LinkType == AttributeTridionTypeComponent) && String.IsNullOrEmpty(linkInfo.Anchor))
            {
                // for component links where nothing is specified specifically do not add an anchor
                linkInfo.Anchor = "false";
            }

            return linkInfo;
        }

        /// <summary>
        /// Distill all useful information of a hyperlink node, and remove the existing tridion: attributes from the node
        /// </summary>
        /// <param name="linkNode">The element node containing a tridion:href attribute being processed</param>
        private ParsedLinkInfo ParseLinkInfo(XmlNode linkNode)
        {
            // Set up data not in attributes
            ParsedLinkInfo linkInfo = new ParsedLinkInfo();
            StringBuilder sb = new StringBuilder();
            XmlAttributeCollection linkAttributeNodes = linkNode.Attributes;
            IList<XmlAttribute> linkAttributes = linkAttributeNodes.Cast<XmlAttribute>().ToList();

            foreach (XmlAttribute linkAttribute in linkAttributes)
            {
                if (linkAttribute.NamespaceURI == TemplateUtilities.TridionNamespace)
                {
                    // Remove attribute from original node
                    linkAttributeNodes.Remove(linkAttribute);

                    // So what attribute is it?
                    string attName = linkAttribute.LocalName;
                    string value = linkAttribute.Value;
                    switch (attName)
                    {
                        case AttributeNameTridionType:
                            linkInfo.LinkType = value;
                            break;
                        case AttributeNameTridionHref:
                            {
                                string targetUriString = ProcessExpression(value);
                                if (!TcmUri.IsValid(targetUriString))
                                {
                                    throw new InvalidDataException(string.Format("Could not process href value '{0}' into uri", value));
                                }
                                linkInfo.TargetUri = new TcmUri(targetUriString);

                                if (linkInfo.TargetUri.ItemType == ItemType.Page)
                                {
                                    linkInfo.LinkType = AttributeTridionTypePage;
                                }
                            }
                            break;
                        case AttributeNameTargetAttribute:
                            linkInfo.TargetAttribute = value;
                            break;
                        case AttributeNameTridionAnchor:
                            linkInfo.Anchor = value;
                            break;
                        default:
                            sb.Append(string.Format("{0}=\"{1}\" ", attName, value));
                            break;
                    }
                }
            }
            if (sb.Length > 0)
            {
                linkInfo.RemainingAttributes = sb.ToString();
            }

            if (linkInfo.TargetUri == null)
            {
                throw new InvalidDataException(string.Format("Internal error: {0}", "Attribute 'href' must be present"));
            }

            if ((linkInfo.LinkType == AttributeTridionTypeComponent) && String.IsNullOrEmpty(linkInfo.Anchor))
            {
                // For component links where nothing is specified specifically do not add an anchor
                linkInfo.Anchor = "false";
            }

            return linkInfo;
        }

        /// <summary>
        /// Simple structure for data-sharing between methods in this class
        /// </summary>
        private class EclItemInPackage
        {
            /// <summary>
            /// Templating Package item
            /// </summary>
            public Item PackageItem { get; private set; }

            /// <summary>
            /// ECL Multimedia item
            /// </summary>
            public IContentLibraryMultimediaItem EclItem { get; private set; }

            /// <summary>
            /// ECLURI
            /// </summary>
            public IEclUri EclUri { get { return EclItem.Id; } }

            /// <summary>
            /// TCMURI of stub item in package
            /// </summary>
            public TcmUri StubUri
            {
                get
                {
                    string uri;
                    return PackageItem.Properties.TryGetValue(Item.ItemPropertyTcmUri, out uri) ? new TcmUri(uri) : TcmUri.UriNull;
                }
            }

            /// <summary>
            /// Creates a new instance of a Templating Package item together with its corresponding a ECL Multimedia item
            /// </summary>
            /// <param name="packageItem">Templating Package item</param>
            /// <param name="eclItem">ECL Multimedia item</param>
            public EclItemInPackage(Item packageItem, IContentLibraryMultimediaItem eclItem)
            {
                PackageItem = packageItem;
                EclItem = eclItem;
            }
        }

        /// <summary>
        /// Simple structure for data-sharing between the methods in this class
        /// </summary>
        private class ParsedLinkInfo
        {
            internal ParsedLinkInfo()
            {
                // Set up default data
                LinkType = AttributeTridionTypeComponent;
                TargetAttribute = AttributeNameTridionHref; // default is 'href'
                Anchor = string.Empty;
            }
            // Shared
            public string LinkType;
            public string RemainingAttributes;
            // For TCDL
            public TcmUri TargetUri;
            public string Anchor;
            public string TargetAttribute;
        }
    }
}