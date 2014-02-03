using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EclProvider.Wufoo.Api;
using Tridion.ExternalContentLibrary.V2;

namespace EclProvider.Wufoo
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

            // ToDo: consider using async calls for getting users and forms
            if (parentFolderUri.ItemType == EclItemTypes.MountPoint && itemTypes.HasFlag(EclItemTypes.Folder))
            {
                // get users
                foreach (WufooData data in Provider.Wufoo.GetUsers())
                {
                    items.Add(new ListItem(parentFolderUri.PublicationId, data));
                }
            }

            // only return files if they are requested (itemTypes is EclItemTypes.File))
            else if (parentFolderUri.ItemType == EclItemTypes.Folder && parentFolderUri.SubType == "user" && itemTypes.HasFlag(EclItemTypes.File))
            {
                // get forms
                foreach (WufooData data in Provider.Wufoo.GetForms(parentFolderUri.ItemId))
                {
                    items.Add(new ListItem(parentFolderUri.PublicationId, data));
                }
            }

            return Provider.HostServices.CreateFolderContent(parentFolderUri, items, CanGetUploadMultimediaItemsUrl(parentFolderUri.PublicationId), CanSearch(parentFolderUri.PublicationId));
        }

        public IContentLibraryItem GetItem(IEclUri eclUri)
        {
            if (eclUri.ItemType == EclItemTypes.File && eclUri.SubType == "form")
            {
                // format of form itemId: [form user]_[form hash]
                string[] ids = eclUri.ItemId.Split('_');
                return new Form(eclUri.PublicationId, Provider.Wufoo.GetForm(ids[0], ids[1]));
            }

            if (eclUri.ItemType == EclItemTypes.Folder && eclUri.SubType == "user")
            {
                return new User(eclUri.PublicationId, Provider.Wufoo.GetUser(eclUri.ItemId));
            }

            throw new NotSupportedException();
        }

        public IList<IContentLibraryItem> GetItems(IList<IEclUri> eclUris)
        {
            List<IContentLibraryItem> items = new List<IContentLibraryItem>();
            IEnumerable<string> uniqueFormIds = (from uri in eclUris
                                                 where uri.ItemType == EclItemTypes.File && uri.SubType == "form"
                                                 select uri.ItemId).Distinct();
            foreach (string id in uniqueFormIds)
            {
                string itemId = id;
                var urisForForm = from uri in eclUris
                                  where uri.ItemType == EclItemTypes.File && uri.SubType == "form" && uri.ItemId == itemId
                                  select uri;

                foreach (IEclUri eclUri in urisForForm)
                {
                    items.Add(GetItem(eclUri));
                }
            }
            return items;
        }

        public byte[] GetThumbnailImage(IEclUri eclUri, int maxWidth, int maxHeight)
        {
            // no thumbnails are availabe so use largest icon
            if (eclUri.ItemType == EclItemTypes.File && eclUri.SubType == "form")
            {
                byte[] thumbnailData = Provider.GetIconImage("form", 48);
                using (MemoryStream ms = new MemoryStream(thumbnailData, false))
                {
                    return Provider.HostServices.CreateThumbnailImage(maxWidth, maxHeight, ms, 48, 48, null);
                }
            }
            return null;
        }

        public string GetUploadMultimediaItemsUrl(IEclUri parentFolderUri)
        {
            if (parentFolderUri.ItemType == EclItemTypes.MountPoint)
            {
                return string.Format(WufooWrapper.UserManagerUrl, Provider.Wufoo.UserName);
            }

            if (parentFolderUri.ItemType == EclItemTypes.Folder && parentFolderUri.SubType == "user")
            {
                return string.Format(WufooWrapper.FormManagerUrl, Provider.Wufoo.UserName);
            }

            throw new NotSupportedException();
        }

        public string GetViewItemUrl(IEclUri eclUri)
        {
            if (eclUri.ItemType == EclItemTypes.File && eclUri.SubType == "form")
            {
                // format of form itemId: [form user]_[form hash]
                string[] ids = eclUri.ItemId.Split('_');

                // load form data for getting form name
                WufooData formData = Provider.Wufoo.GetForm(ids[0], ids[1]);

                return string.Format(WufooWrapper.FormEditUrl, ids[0], formData.Name);
            }

            throw new NotSupportedException();
        }

        public string IconIdentifier
        {
            get { return "wufoo"; }
        }

        public IFolderContent Search(IEclUri contextUri, string searchTerm, int pageIndex, int numberOfItems)
        {
            throw new NotSupportedException();
        }

        public string Dispatch(string command, string payloadVersion, string payload, out string responseVersion)
        {
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
