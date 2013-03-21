using System;
using Example.EclProvider.Api;
using Example.EclProvider.Extensions;
using Tridion.ExternalContentLibrary.V2;

namespace Example.EclProvider
{
    /// <summary>
    /// Represents the basic properties of a Flickr (Photo) Set. 
    /// </summary>
    public class ListItem : IContentLibraryListItem
    {
        internal readonly FlickrInfo Info;
        private readonly IEclUri _id;

        public ListItem(int publicationId, FlickrInfo info)
        {
            Info = info;
            if (info.IsPhoto)
            {
                // format of photo item id: [flickr photo id]_[flickr photo secret]_[flickr photo set id]
                string itemId = string.Format("{0}_{1}_{2}", info.Id, info.Secret, info.PhotoSetId);
                _id = Provider.HostServices.CreateEclUri(publicationId, Provider.MountPointId, itemId, DisplayTypeId, EclItemTypes.File);
            }
            else
            {
                _id = Provider.HostServices.CreateEclUri(publicationId, Provider.MountPointId, info.Id, DisplayTypeId, EclItemTypes.Folder);
            }
        }

        // for folders only
        public bool CanGetUploadMultimediaItemsUrl
        {
            get { return true; }
        }

        // for folders only
        public bool CanSearch
        {
            get { return false; }
        }

        public string DisplayTypeId
        {
            get { return Info.IsPhoto ? "img" : "set"; }
        }

        public string IconIdentifier
        {
            get { return null; }
        }

        public IEclUri Id
        {
            get { return _id; }
        }

        public bool IsThumbnailAvailable
        {
            get { return Info.IsPhoto; }
        }

        public DateTime? Modified
        {
            get { return Info.Modified; }
        }

        public string ThumbnailETag
        {
            get { return Modified != null ? Modified.Value.ETag() : Info.Created.Value.ETag(); }
        }

        public string Title
        {
            get { return Info.Title; }
            set { throw new NotSupportedException(); }
        }

        // allow override of dispatch
        public virtual string Dispatch(string command, string payloadVersion, string payload, out string responseVersion)
        {
            throw new NotSupportedException();
        }
    }
}
