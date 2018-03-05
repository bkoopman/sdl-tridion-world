using System;
using System.Collections.Generic;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.Logging;
using System.Diagnostics;
using Tridion.ContentManager.Publishing;

namespace SDLTridion.Examples.EventSystem
{
    public class ReiConfig
    {
        #region Constants
        /// <summary>
        /// Event system name
        /// </summary>
        public const string Name = "Rapid Editorial Interface";

        /// <summary>
        /// Publish priority (set to high)
        /// </summary>
        public const PublishPriority Priority = PublishPriority.High;

        #endregion

        #region Metadata XML Field names
        /// <summary>
        /// Metadata XML Field name for the Schema on which Components should be based to trigger the action
        /// </summary>
        public const string SchemaUriField = "schemaUri";

        /// <summary>
        /// Metadata XML Field name for the Structure Group in which the Page should be created 
        /// </summary>
        public const string StructureGroupUriField = "structureGroupUri";

        /// <summary>
        /// Metadata XML Field name for the Page Template to be used on the Page 
        /// </summary>
        public const string PageTemplateUriField = "pageTemplateUri";

        /// <summary>
        /// Metadata XML Field name for the Component Template to be used on the Page 
        /// </summary>
        public const string ComponentTemplateUriField = "componentTemplateUri";

        /// <summary>
        /// Metadata XML Field name for the URI of the index Page 
        /// </summary>
        public const string IndexPageUriField = "indexPageUri";

        /// <summary>
        /// Metadata XML Field name for the Component Template to be used on the index Page 
        /// </summary>
        public const string IndexCtUriField = "indexCtUri";

        /// <summary>
        /// Metadata XML Field name for the Target Type where the Pages should be published to 
        /// </summary>
        public const string TargetTypeUriField = "targetTypeUri";

        #endregion

        #region Properties
        /// <summary>
        /// Indicator whether TcmUris are correctly filled (i.e. not containing null or UriNull) 
        /// </summary>
        public bool IsValid { get; protected set; }
        /// <summary>
        /// The Schema URI on which Components should be based to trigger the action
        /// </summary>
        /// <remarks>
        /// Future enhancement can be an IList of Schema URIs  
        /// </remarks>
        public TcmUri SchemaUri { get; protected set; }
        /// <summary>
        /// The Structure Group URI in which the Page should be created 
        /// </summary>
        public TcmUri StructureGroupUri { get; protected set; }
        /// <summary>
        /// The Component Template URI to be used on the Page (transformed into Context Publication of Page)
        /// </summary>
        public TcmUri PageTemplateUri { get; protected set; }
        /// <summary>
        /// The Publication URI in which the Page should be created 
        /// </summary>
        public TcmUri ComponentTemplateUri { get; protected set; }
        /// <summary>
        /// URI of the index Page
        /// </summary>
        public TcmUri IndexPageUri { get; protected set; }
        /// <summary>
        /// The Component Template URI to be used on the index Page  (transformed into Context Publication of Index Page)
        /// </summary>
        public TcmUri IndexComponentTemplateUri { get; protected set; }
        /// <summary>
        /// Target Type URI where the Pages should be published to 
        /// </summary>
        /// <remarks>
        /// Future enhancement can be an IList of Target Type URIs  
        /// </remarks>
        public TcmUri TargetTypeUri { get; protected set; }

        #endregion

        /// <summary>
        /// Creates a new configuration instance
        /// </summary>
        /// <param name="metadata">ItemFields collection with configuration metadata</param>
        public ReiConfig(ItemFields metadata)
        {
            IsValid = true;
            try
            {
                SchemaUri = GetTcmUri(SchemaUriField, metadata);
                
                StructureGroupUri = GetTcmUri(StructureGroupUriField, metadata);
                
                // make Page Template URI local to Structure Group
                PageTemplateUri = GetTcmUri(PageTemplateUriField, metadata, StructureGroupUri);
                
                // make Component Template URI local to Structure Group
                ComponentTemplateUri = GetTcmUri(ComponentTemplateUriField, metadata, StructureGroupUri);
                
                IndexPageUri = GetTcmUri(IndexPageUriField, metadata);
                
                // make Component Template URI local to index Page
                IndexComponentTemplateUri = GetTcmUri(IndexCtUriField, metadata, IndexPageUri);

                TargetTypeUri = GetTcmUri(TargetTypeUriField, metadata);
            }
            catch (Exception ex)
            {
                Logger.Write(ex, Name, LoggingCategory.General, TraceEventType.Error);
                IsValid = false;
            }
        }

        #region Private Members
        private TcmUri GetTcmUri(string fieldName, ItemFields fields, TcmUri contextPublicationUri)
        {
            TcmUri uri = GetTcmUriFromField(fieldName, fields);
            
            if (!TcmUri.IsNullOrUriNull(contextPublicationUri))
            {
                uri = TransformTcmUri(uri, contextPublicationUri);
            }

            if (TcmUri.IsNullOrUriNull(uri))
            {
                IsValid = false;
            }

            return uri.GetVersionlessUri();
        }

        private TcmUri GetTcmUri(string fieldName, ItemFields fields)
        {
            return GetTcmUri(fieldName, fields, TcmUri.UriNull);
        }

        #endregion

        #region Static Members
        /// <summary>
        /// Transform a TCM URI to a Context Publication (i.e. change the Publication ID to the one of the supplied Context Publication)
        /// </summary>
        /// <param name="itemUri">Original TCM URI</param>
        /// <param name="contextPublicationUri">TCM URI of Context Publication (or any item in it)</param>
        /// <returns>TCM URI local to Context Publication or UriNull if not parsable</returns>
        public static TcmUri TransformTcmUri(TcmUri itemUri, TcmUri contextPublicationUri)
        {
            TcmUri uri;

            if (contextPublicationUri.ItemType == ItemType.Publication)
            {
                uri = new TcmUri(itemUri.ItemId, itemUri.ItemType, contextPublicationUri.ItemId);
            }
            else
            {
                uri = new TcmUri(itemUri.ItemId, itemUri.ItemType, contextPublicationUri.PublicationId);
            }

            return uri.GetVersionlessUri();
        }

        /// <summary>
        /// Get TCM URI from item (text) field using context Publication id
        /// </summary>
        /// <param name="fieldName">the XML field name</param>
        /// <param name="fields">ItemFields collection</param>
        /// <param name="contextPublicationId">ID for the context Publication</param>
        /// <returns>TcmUri object from field value using context Publication or UriNull if not parsable or empty</returns>
        public static TcmUri GetTcmUriFromField(string fieldName, ItemFields fields, int contextPublicationId)
        {
            TcmUri uri = GetTcmUriFromField(fieldName, fields);

            if (!TcmUri.IsNullOrUriNull(uri))
            {
                try
                {
                    uri = new TcmUri(uri.ItemId, uri.ItemType, contextPublicationId);
                }
                catch (InvalidTcmUriException ex)
                {
                    Logger.Write(ex, Name, LoggingCategory.General, TraceEventType.Error);
                }
            }

            return uri.GetVersionlessUri();
        }

        /// <summary>
        /// Get TCM URI from item (text) field
        /// </summary>
        /// <param name="fieldName">the XML field name</param>
        /// <param name="fields">ItemFields collection</param>
        /// <returns>TcmUri object from field value or UriNull if not parsable or empty</returns>
        public static TcmUri GetTcmUriFromField(string fieldName, ItemFields fields)
        {
            TcmUri uri = TcmUri.UriNull;

            string value = GetSingleStringValue(fieldName, fields);

            if (value != null)
            {
                try
                {
                    uri = new TcmUri(value);
                }
                catch (InvalidTcmUriException ex)
                {
                    Logger.Write(ex, Name, LoggingCategory.General, TraceEventType.Error);
                }
            }

            return uri.GetVersionlessUri();
        }

        /// <summary>
        /// Get a list of strings from a item (text) field. 
        /// </summary>
        /// <param name="fieldName">the XML field name</param>
        /// <param name="fields">ItemFields collection</param>
        /// <returns>IList with string values from the field or null of the field has no values</returns>
        public static IList<string> GetStringValues(string fieldName, ItemFields fields)
        {
            if (!fields.Contains(fieldName))
            {
                return null;
            }
            TextField field = (TextField)fields[fieldName];
            return (field.Values.Count > 0) ? field.Values : null;
        }

        /// <summary>
        /// Get a single (string) value from a item (text) field
        /// </summary>
        /// <param name="fieldName">the XML field name</param>
        /// <param name="fields">ItemFields collection</param>
        /// <returns>string value from the field or null if the field is empty</returns>
        public static String GetSingleStringValue(string fieldName, ItemFields fields)
        {
            if (fields.Contains(fieldName))
            {
                TextField field = fields[fieldName] as TextField;

                if (field != null) return field.Value;
            }

            return null;
        }

        #endregion
    }
}
