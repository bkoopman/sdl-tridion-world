using System;
using EclProvider.Wufoo.Extensions;
using EclProvider.Wufoo.Api;
using Tridion.ExternalContentLibrary.V2;

namespace EclProvider.Wufoo
{
    /// <summary>
    /// Represents the basic properties of a Wufoo item. 
    /// </summary>
    public class ListItem : IContentLibraryListItem
    {
        internal readonly WufooData Data;
        private readonly IEclUri _id;

        public ListItem(int publicationId, WufooData data)
        {
            Data = data;
            if (data.IsForm)
            {
                // format of form itemId: [form user]_[form hash]
                string itemId = string.Format("{0}_{1}", data.User, data.Hash);
                _id = Provider.HostServices.CreateEclUri(publicationId, Provider.MountPointId, itemId, DisplayTypeId, EclItemTypes.File);
            }
            else
            {
                // use username as itemId (also available in form itemId) 
                _id = Provider.HostServices.CreateEclUri(publicationId, Provider.MountPointId, data.Title, DisplayTypeId, EclItemTypes.Folder);
            }
        }

        // for folders only
        public bool CanGetUploadMultimediaItemsUrl
        {
            get { return Data.CreateForms; }
        }

        // for folders only
        public bool CanSearch
        {
            get { return false; }
        }

        public string DisplayTypeId
        {
            get { return Data.IsForm ? "form" : "user"; }
        }

        public string IconIdentifier
        {
            get { return DisplayTypeId; }
        }

        public IEclUri Id
        {
            get { return _id; }
        }

        public bool IsThumbnailAvailable
        {
            get { return Data.IsForm; }
        }

        public DateTime? Modified
        {
            get { return Data.Modified; }
        }

        public string ThumbnailETag
        {
            get { return Modified != null ? Modified.Value.ETag() : Data.Created.Value.ETag(); }
        }

        public string Title
        {
            get { return Data.Title; }
            set { throw new NotSupportedException(); }
        }

        // allow override of dispatch
        public virtual string Dispatch(string command, string payloadVersion, string payload, out string responseVersion)
        {
            throw new NotSupportedException();
        }
    }
}
