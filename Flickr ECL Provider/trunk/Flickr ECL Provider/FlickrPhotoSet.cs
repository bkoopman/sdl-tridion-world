using System;
using Example.EclProvider.Api;
using Tridion.ExternalContentLibrary.V2;

namespace Example.EclProvider
{
    /// <summary>
    /// Represents a Flickr (Photo) Set with all details loaded. 
    /// </summary>
    public class FlickrPhotoSet : ListItem, IContentLibraryItem
    {
        public FlickrPhotoSet(int publicationId, FlickrInfo info) : base(publicationId, info)
        {
            // if info needs to be fully loaded, do so here
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
            get { return Info.Created; }
        }

        public string CreatedBy
        {
            get { return Provider.Flickr.UserId; }
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
