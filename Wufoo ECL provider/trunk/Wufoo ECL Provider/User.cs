using System;
using EclProvider.Wufoo.Api;
using Tridion.ExternalContentLibrary.V2;

namespace EclProvider.Wufoo
{
    /// <summary>
    /// Represents a Wufoo user with all details loaded. 
    /// </summary>
    public class User : ListItem, IContentLibraryItem
    {
        public User(int publicationId, WufooData data) : base(publicationId, data)
        {
            // if data needs to be fully loaded, do so here
        }

        public bool CanGetViewItemUrl
        {
            get { return false; }
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
            get { return Data.AdminUserName; }
        }

        public string MetadataXml
        {
            get { return null; }
            set { throw new NotSupportedException(); }
        }

        public ISchemaDefinition MetadataXmlSchema
        {
            get { return null; }
        }

        public string ModifiedBy
        {
            get { return CreatedBy; }
        }

        public IEclUri ParentId
        {
            get
            {
                // return mountpoint uri (we only have folders in the top level)
                return Provider.HostServices.CreateEclUri(Id.PublicationId, Id.MountPointId);
            }
        }

        public IContentLibraryItem Save(bool readback)
        {
            // as saving isn't supported, the result of saving is always the item itself
            return readback ? this : null;
        }
    }
}
