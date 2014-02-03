using System;
using System.Collections.Generic;
using EclProvider.Wufoo.Api;
using Tridion.ExternalContentLibrary.V2;

namespace EclProvider.Wufoo
{
    /// <summary>
    /// Represents a Wufoo form with all details loaded. 
    /// </summary>
    public class Form : ListItem, IContentLibraryMultimediaItem
    {
        private const string NamespaceUri = "http://wufoo.com/api";
        private const string RootElementName = "Metadata";
        private const string FormEmbedHtml =
            "<div id=\"wufoo-{1}\"><a href=\"http://{0}.wufoo.com/forms/{1}\">Wufoo form</a></div>" +
            "<script type=\"text/javascript\">var {1};(function(e,t){{var n=e.createElement(t),r={{userName:\"{0}\",formHash:\"{1}\",autoResize:true,async:true,header:\"show\"}};" +
            "n.src=(\"https:\"==e.location.protocol?\"https://\":\"http://\")+\"wufoo.com/scripts/embed/form.js\";" +
            "n.onload=n.onreadystatechange=function(){{var e=this.readyState;if(e)if(e!=\"complete\")if(e!=\"loaded\")return;" +
            "try{{{1}=new WufooForm;{1}.initialize(r);{1}.display()}}catch(t){{}}}};" +
            "var i=e.getElementsByTagName(t)[0],s=i.parentNode;s.insertBefore(n,i)}})(document,\"script\")</script>";

        private readonly string _fields;
        private readonly string _entries;

        public Form(int publicationId, WufooData data) : base(publicationId, data)
        {
            // ToDo: consider using async calls for loading fields and entries
            _fields = Provider.Wufoo.GetFields(data.LinkFields, data.User);
            _entries = Provider.Wufoo.GetEntries(data.LinkEntries, data.User);
        }

        public string Filename
        {
            get { return Data.Name; }
        }

        public IContentResult GetContent(IList<ITemplateAttribute> attributes)
        {
            // Wufoo forms are already published, so we have to return null here
            return null;
        }

        public string GetDirectLinkToPublished(IList<ITemplateAttribute> attributes)
        {
            // Wufoo forms are already published, so simply return their url
            return Data.Url;
        }

        public string GetTemplateFragment(IList<ITemplateAttribute> attributes)
        {
            // Wufoo forms are already published, so we can provide a template fragment
            return string.Format(FormEmbedHtml, Data.User, Data.Hash);
        }

        public int? Height
        {
            get { return null; }
        }

        public string MimeType
        {
            get { return "text/html"; }
        }

        public int? Width
        {
            get { return null; }
        }

        public bool CanGetViewItemUrl
        {
            get { return true; }
        }

        public bool CanUpdateMetadataXml
        {
            get { return false; }
        }

        public bool CanUpdateTitle
        {
            get { return false; }
        }

        public DateTime? Created
        {
            get { return Data.Created; }
        }

        public string CreatedBy
        {
            get { return Data.User; }
        }

        public string MetadataXml
        {
            get
            {
                return
                    string.Format(
                        "<{0} xmlns=\"{1}\"><Description>{2}</Description><RedirectMessage>{3}</RedirectMessage><Email>{4}</Email><IsPublic>{5}</IsPublic><Language>{6}</Language><StartDate>{7}</StartDate><EndDate>{8}</EndDate><EntryLimit>{9}</EntryLimit>{10}{11}</{0}>",
                        RootElementName, NamespaceUri, Data.Description, Data.RedirectMessage, Data.Email, Data.IsPublic,
                        Data.Language, Data.StartDate, Data.EndDate, Data.EntryLimit, _fields, _entries);
            }
            set { throw new NotSupportedException(); }
        }

        public ISchemaDefinition MetadataXmlSchema
        {
            get
            {
                // define schema
                ISchemaDefinition schema = Provider.HostServices.CreateSchemaDefinition(RootElementName, NamespaceUri);

                // add schema fields
                schema.Fields.Add(Provider.HostServices.CreateMultiLineTextFieldDefinition("Description", "Description", 0, 1, 2));
                schema.Fields.Add(Provider.HostServices.CreateMultiLineTextFieldDefinition("RedirectMessage", "Redirect message", 0, 1, 2));
                schema.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("Email", "Email", 0, 1));
                schema.Fields.Add(Provider.HostServices.CreateNumberFieldDefinition("IsPublic", "Public", 0, 1));
                schema.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("Language", "Language", 0, 1));
                schema.Fields.Add(Provider.HostServices.CreateDateFieldDefinition("StartDate", "Start", 0, 1));
                schema.Fields.Add(Provider.HostServices.CreateDateFieldDefinition("EndDate", "End", 0, 1));
                schema.Fields.Add(Provider.HostServices.CreateNumberFieldDefinition("EntryLimit", "Entry limit", 0, 1));

                // fields embeddable schema
                IFieldGroupDefinition fields = Provider.HostServices.CreateFieldGroupDefinition("Field", "Fields", 0, null);
                {
                    fields.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("ID", "ID", 0, 1));
                    fields.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("Title", "Title", 0, 1));
                    fields.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("Type", "Type", 0, 1));
                    fields.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("Instructions", "Instructions", 0, 1));
                    fields.Fields.Add(Provider.HostServices.CreateNumberFieldDefinition("IsRequired", "Required", 0, 1));
                    fields.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("ClassNames", "Class names", 0, 1));
                    fields.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("DefaultVal", "Default value", 0, 1));
                    fields.Fields.Add(Provider.HostServices.CreateNumberFieldDefinition("Page", "Page", 0, 1));

                    // subfields embeddable schema
                    IFieldGroupDefinition subfields = Provider.HostServices.CreateFieldGroupDefinition("Subfield", "Subfields", 0, null);
                    {
                        subfields.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("DefaultVal", "Default value", 0, 1));
                        subfields.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("ID", "ID", 0, 1));
                        subfields.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("Label", "Label", 0, 1));
                    }
                    fields.Fields.Add(subfields);

                    // choises embeddable schema
                    IFieldGroupDefinition choices = Provider.HostServices.CreateFieldGroupDefinition("Choice", "Choices", 0, null);
                    {
                        choices.Fields.Add(Provider.HostServices.CreateNumberFieldDefinition("Score", "Score", 0, 1));
                        choices.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("Label", "Label", 0, 1));
                    }
                    fields.Fields.Add(choices);
                    fields.Fields.Add(Provider.HostServices.CreateNumberFieldDefinition("HasOtherField", "Has other field", 0, 1));
                }
                schema.Fields.Add(fields);

                // entries embeddable schema
                IFieldGroupDefinition entries = Provider.HostServices.CreateFieldGroupDefinition("Entry", "Entries", 0, null);
                {
                    entries.Fields.Add(Provider.HostServices.CreateNumberFieldDefinition("EntryId", "ID", 0, 1));
                    entries.Fields.Add(Provider.HostServices.CreateDateFieldDefinition("DateCreated", "Created", 0, 1));
                    entries.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("CreatedBy", "Creator", 0, 1));
                    entries.Fields.Add(Provider.HostServices.CreateDateFieldDefinition("DateUpdated", "Updated", 0, 1));
                    entries.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("UpdatedBy", "Updater", 0, 1));

                    // data embeddable schema
                    IFieldGroupDefinition data = Provider.HostServices.CreateFieldGroupDefinition("Data", "Data", 0, null);
                    {
                        data.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("FieldId", "Field", 0, 1));
                        data.Fields.Add(Provider.HostServices.CreateSingleLineTextFieldDefinition("Value", "Value", 0, 1));
                    }
                    entries.Fields.Add(data);
                }
                schema.Fields.Add(entries);

                return schema;
            }
        }

        public string ModifiedBy
        {
            get { return CreatedBy; }
        }

        public IEclUri ParentId
        {
            get
            {
                // return parent uri (user/folder)
                return Provider.HostServices.CreateEclUri(Id.PublicationId, Id.MountPointId, Data.User, "user", EclItemTypes.Folder);
            }
        }

        public IContentLibraryItem Save(bool readback)
        {
            // as saving isn't supported, the result of saving is always the item itself
            return readback ? this : null;
        }
    }
}
