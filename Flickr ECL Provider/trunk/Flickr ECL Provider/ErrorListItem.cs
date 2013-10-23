using System;
using Example.EclProvider.Api;
using Example.EclProvider.Extensions;
using Tridion.ExternalContentLibrary.V2;

namespace Example.EclProvider
{
    /// <summary>
    /// Represents the basic properties of a Flickr (Photo) Set. 
    /// </summary>
    public class ErrorListItem : IContentLibraryListItem
    {
        private readonly IEclUri _id;
        private readonly string _errorMessage;

        public ErrorListItem(int publicationId, string errorMessage)
        {
            _id = Provider.HostServices.CreateEclUri(publicationId, Provider.MountPointId, "error", DisplayTypeId, EclItemTypes.Folder);
            _errorMessage = errorMessage;
        }

        // for folders only
        public bool CanGetUploadMultimediaItemsUrl
        {
            get { return false; }
        }

        // for folders only
        public bool CanSearch
        {
            get { return false; }
        }

        public string DisplayTypeId
        {
            get { return "error"; }
        }

        public string IconIdentifier
        {
            //todo return a error icon
            get { return "error"; }
        }

        public IEclUri Id
        {
            get { return _id; }
        }

        public bool IsThumbnailAvailable
        {
            get { return false; }
        }

        public DateTime? Modified
        {
            get { return null; }
        }

        public string ThumbnailETag
        {
            get { return null; }
        }

        public string Title
        {
            get { return _errorMessage; }
            set { throw new NotSupportedException(); }
        }

        // allow override of dispatch
        public virtual string Dispatch(string command, string payloadVersion, string payload, out string responseVersion)
        {
            throw new NotSupportedException();
        }
    }
}
