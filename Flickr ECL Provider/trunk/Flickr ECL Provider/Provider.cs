using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Example.EclProvider.Api;
using Tridion.ExternalContentLibrary.V2;
using System.AddIn;

namespace Example.EclProvider
{
    [AddIn("FlickrProvider", Version = "1.0.0.0")]
    public class Provider : IContentLibrary
    {
        private static readonly XNamespace FlickrNs = "http://flickr.com/services/api";
        private static readonly string IconBasePath = Path.Combine(AddInFolder, "Themes");

        internal static string MountPointId { get; private set; }
        internal static Flickr Flickr { get; private set; }
        internal static IHostServices HostServices { get; private set; }

        // This should probably be more generally available - maybe as an extension to IContentLibraryContext in addinbase?
        internal static string AddInFolder
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        internal static byte[] GetIconImage(string iconIdentifier, int iconSize)
        {
            int actualSize;
            // get icon directly from default theme folder
            return HostServices.GetIcon(IconBasePath, "_Default", iconIdentifier, iconSize, out actualSize);
        }

        public void Initialize(string mountPointId, string configurationXmlElement, IHostServices hostServices)
        {
            MountPointId = mountPointId;
            HostServices = hostServices;
            
            // read ExtenalContentLibrary.xml for this mountpoint
            XElement config = XElement.Parse(configurationXmlElement);
            Flickr = new Flickr(config.Element(FlickrNs + "FlickrApiKey").Value, config.Element(FlickrNs + "FlickrNSID").Value);
        }

        public IContentLibraryContext CreateContext(IEclSession tridionUser)
        {
            return new Mountpoint();
        }

        public IList<IDisplayType> DisplayTypes
        {
            get
            {
                // we currently support Flickr (Photo) Sets as folders and Flickr Photos in them
                return new List<IDisplayType>
                           {
                               HostServices.CreateDisplayType("set", "Flickr Set", EclItemTypes.Folder), 
                               HostServices.CreateDisplayType("img", "Flickr Photo", EclItemTypes.File)
                           };
            }
        }

        public byte[] GetIconImage(string theme, string iconIdentifier, int iconSize)
        {
            // use static implementation
            return GetIconImage(iconIdentifier, iconSize);
        }

        public void Dispose()
        {
        }
    }
}
