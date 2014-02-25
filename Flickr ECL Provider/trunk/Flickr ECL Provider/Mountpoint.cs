using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Example.EclProvider.Api;
using Example.EclProvider.Extensions;
using Tridion.ExternalContentLibrary.V2;

namespace Example.EclProvider
{
    public class Mountpoint : IContentLibraryContext
    {
        public bool CanGetUploadMultimediaItemsUrl(int publicationId)
        {
            return true;
        }

        public bool CanSearch(int publicationId)
        {
            return false;
        }

        public IList<IContentLibraryListItem> FindItem(IEclUri eclUri)
        {
            // return null so we force it to call GetItem(IEclUri)
            return null;
        }

        public IFolderContent GetFolderContent(IEclUri parentFolderUri, int pageIndex, EclItemTypes itemTypes)
        {
            List<IContentLibraryListItem> items = new List<IContentLibraryListItem>();

            if (parentFolderUri.ItemType == EclItemTypes.MountPoint && itemTypes.HasFlag(EclItemTypes.Folder))
            {
                // get photosets
                foreach (FlickrInfo info in Provider.Flickr.GetPhotoSets())
                {
                    items.Add(new ListItem(parentFolderUri.PublicationId, info));
                }

                // oops, no photosets
                if(items.Count == 0)
                {
                    items.Add(new ErrorListItem(parentFolderUri.PublicationId, "There are no public photo sets in this Flickr account."));
                }
            }
            // only return files if they are requested (itemTypes is EclItemTypes.File))
            else if (parentFolderUri.ItemType == EclItemTypes.Folder && parentFolderUri.SubType == "set" && itemTypes.HasFlag(EclItemTypes.File))
            {
                // get photos
                foreach (FlickrInfo info in Provider.Flickr.GetPhotosInSet(parentFolderUri.ItemId))
                {
                    //items.Add(new FlickrPhoto(parentFolderUri.PublicationId, info));
                    items.Add(new ListItem(parentFolderUri.PublicationId, info));
                }
            }

            return Provider.HostServices.CreateFolderContent(parentFolderUri, items, CanGetUploadMultimediaItemsUrl(parentFolderUri.PublicationId), CanSearch(parentFolderUri.PublicationId));
        }

        public IContentLibraryItem GetItem(IEclUri eclUri)
        {
            if (eclUri.ItemType == EclItemTypes.File && eclUri.SubType == "img")
            {
                // format of photo item id: [flickr photo id]_[flickr photo secret]_[flickr photo set id]
                string[] ids = eclUri.ItemId.Split('_');
                return new FlickrPhoto(eclUri.PublicationId, Provider.Flickr.GetPhotoInfo(ids[0], ids[1], ids[2]));
            }

            if (eclUri.ItemType == EclItemTypes.Folder && eclUri.SubType == "set")
            {
                return new FlickrPhotoSet(eclUri.PublicationId, Provider.Flickr.GetPhotoSetInfo(eclUri.ItemId));
            }

            throw new NotSupportedException();
        }

        public IList<IContentLibraryItem> GetItems(IList<IEclUri> eclUris)
        {
            List<IContentLibraryItem> items = new List<IContentLibraryItem>();
            IEnumerable<string> uniquePhotoIds = (from uri in eclUris
                                                  where uri.ItemType == EclItemTypes.File && uri.SubType == "img"
                                                  select uri.ItemId).Distinct();
            foreach (string id in uniquePhotoIds)
            {
                string itemId = id;
                var urisForPhoto = from uri in eclUris
                                   where uri.ItemType == EclItemTypes.File && uri.SubType == "img" && uri.ItemId == itemId
                                   select uri;

                foreach (IEclUri eclUri in urisForPhoto)
                {
                    items.Add(GetItem(eclUri));
                }
            }
            return items;
        }

        public byte[] GetThumbnailImage(IEclUri eclUri, int maxWidth, int maxHeight)
        {
            // only return thumbnails for the actual photos
            if (eclUri.ItemType == EclItemTypes.File && eclUri.SubType == "img")
            {
                // format of photo item id: [flickr photo id]_[flickr photo secret]_[flickr photo set id]
                string[] ids = eclUri.ItemId.Split('_');
                FlickrInfo photo = Provider.Flickr.GetPhotoInfo(ids[0], ids[1], ids[2]);

                WebClient webClient = new WebClient();
                byte[] thumbnailData = webClient.DownloadData(Flickr.GetPhotoUrl(photo, maxWidth));
                using (MemoryStream ms = new MemoryStream(thumbnailData, false))
                {
                    return Provider.HostServices.CreateThumbnailImage(maxWidth, maxHeight, ms, Flickr.MaxWidth, Flickr.MaxHeight, null);
                }
            }
            return null;
        }

        public string GetUploadMultimediaItemsUrl(IEclUri parentFolderUri)
        {
            if (parentFolderUri.ItemType == EclItemTypes.MountPoint)
            {
                return Provider.Flickr.GetPhotoSetPageUrl(null);
            }

            if (parentFolderUri.ItemType == EclItemTypes.Folder && parentFolderUri.SubType == "set")
            {
                return Provider.Flickr.GetPhotoSetPageUrl(parentFolderUri.ItemId);
            }

            throw new NotSupportedException();
        }

        public string GetViewItemUrl(IEclUri eclUri)
        {
            if (eclUri.ItemType == EclItemTypes.File && eclUri.SubType == "img")
            {
                // format of photo item id: [flickr photo id]_[flickr photo secret]_[flickr photo set id]
                string[] ids = eclUri.ItemId.Split('_');
                return Provider.Flickr.GetPhotoPageUrl(ids[0]);
            }

            throw new NotSupportedException();
        }

        public string IconIdentifier
        {
            get { return "flickr"; }
        }

        public IFolderContent Search(IEclUri contextUri, string searchTerm, int pageIndex, int numberOfItems)
        {
            throw new NotSupportedException();
        }

        public string Dispatch(string command, string payloadVersion, string payload, out string responseVersion)
        {
            responseVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (payloadVersion.EqualsIgnoringRevision(responseVersion) && command.Equals("GetConfigurationXmlElement"))
            {
                return Provider.ConfigurationXmlElement;
            }

            throw new NotSupportedException();
        }

        public void StubComponentCreated(IEclUri eclUri, string tcmUri)
        {
        }

        public void StubComponentDeleted(IEclUri eclUri, string tcmUri)
        {
        }

        public void Dispose()
        {
        }
    }
}
