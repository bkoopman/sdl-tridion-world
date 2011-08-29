using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace Tridion.ContentManager.Templating.Expression
{
    /// <summary>
    /// Functions available to expression evaluation for Netbiscuits compatibility.
    /// </summary>
    public class NetbiscuitsFunctions : IFunctionSource
    {
        // Context of the evaluation
        private Package _package;
        private Engine _engine;

        #region IFunctionSource Members

        /// <summary>
        /// Initializes the function source with context data.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="package">The package.</param>
        public void Initialize(Engine engine, Package package)
        {
            _package = package;
            _engine = engine;
        }

        #endregion

        /// <summary>
        /// Renders a ComponentField using the <see cref="BuiltInFunctions.RenderComponentField(string, int)" /> function and converts that to Netbiscuits BBCode.
        /// </summary>
        /// <param name="fieldExpression">Reference to a field relative to the context component. For example Fields.MyEmbeddedSchema.MyField</param>
        /// <param name="fieldIndex">Index of this value for multi-valued fields starting at 1. Single-value fields simply use 1</param>        
        /// <returns>
        /// The value of a field on the context component converted to Netbiscuits BBCode surrounded with tcdl:ComponentField where image tags will be removed from the output.
        /// </returns>
        /// <exception cref="TemplatingException">When there is no context component or the expression cannot be resolved on it</exception>
        [TemplateCallable]
        public string RenderComponentFieldAsBBCode(string fieldExpression, int fieldIndex)
        {
            return RenderComponentFieldAsBBCode(fieldExpression, fieldIndex, false);
        }

        /// <summary>
        /// Renders a ComponentField using the <see cref="BuiltInFunctions.RenderComponentField(string, int)" /> function and converts that to Netbiscuits BBCode.
        /// </summary>
        /// <param name="fieldExpression">Reference to a field relative to the context component. For example Fields.MyEmbeddedSchema.MyField</param>
        /// <param name="fieldIndex">Index of this value for multi-valued fields starting at 1. Single-value fields simply use 1</param>        
        /// <param name="outputImages">If set to <c>true</c> image tags will be converted. If set to <c>false</c> image tags will be removed from the output.</param>
        /// <returns>
        /// The value of a field on the context component converted to Netbiscuits BBCode surrounded with tcdl:ComponentField
        /// </returns>
        /// <exception cref="TemplatingException">When there is no context component or the expression cannot be resolved on it</exception>
        [TemplateCallable]
        public string RenderComponentFieldAsBBCode(string fieldExpression, int fieldIndex, bool outputImages)
        {
            BuiltInFunctions functions = new BuiltInFunctions(_engine, _package);
            string output = functions.RenderComponentField(fieldExpression, fieldIndex);

            StringReader sr = new StringReader(output);
            NameTable nt = new NameTable();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
            nsmgr.AddNamespace("tcdl", Tridion.ContentManager.Templating.TcdlTags.TcdlNamespace);
            XmlParserContext parserContext = new XmlParserContext(null, nsmgr, null, XmlSpace.None);

            XmlReader xmlReader = XmlReader.Create(sr, new XmlReaderSettings(), parserContext);

            XslCompiledTransform transform = new XslCompiledTransform(true);

            using (Stream xsltStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tridion.ContentManager.Templating.Expression.NetbiscuitsFunctions.XhtmlToBBCode.xslt"))
            {
                using (XmlReader xsltReader = XmlReader.Create(xsltStream))
                {
                    transform.Load(xsltReader);
                }
            }

            StringWriter resultWriter = new StringWriter();
            XsltArgumentList argumentList = new XsltArgumentList();
            argumentList.AddParam("IncludeImages", String.Empty, outputImages);

            transform.Transform(xmlReader, argumentList, resultWriter);

            return resultWriter.ToString();   
        }
    }
}
