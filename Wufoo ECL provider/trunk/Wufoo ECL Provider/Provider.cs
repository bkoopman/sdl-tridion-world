using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using EclProvider.Wufoo.Api;
using Tridion.ExternalContentLibrary.V2;
using System.AddIn;

namespace EclProvider.Wufoo
{
    [AddIn("WufooProvider", Version = "1.0.0.0")]
    public class Provider : IContentLibrary
    {
        private static readonly XNamespace WufooNs = "http://wufoo.com/api";
        private static readonly string IconBasePath = Path.Combine(AddInFolder, "Themes");

        internal static string MountPointId { get; private set; }
        internal static WufooWrapper Wufoo { get; private set; }
        internal static IHostServices HostServices { get; private set; }

        // this should probably be more generally available - maybe as an extension to IContentLibraryContext in addinbase?
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
            Wufoo = new WufooWrapper(config.Element(WufooNs + "WufooApiKey").Value, config.Element(WufooNs + "WufooUser").Value);
        }

        public IContentLibraryContext CreateContext(IEclSession tridionUser)
        {
            return new Mountpoint();
        }

        public IList<IDisplayType> DisplayTypes
        {
            get
            {
                // currently supporting Wufoo forms as files, and users as folders
                return new List<IDisplayType>
                           {
                               HostServices.CreateDisplayType("user", "Wufoo User", EclItemTypes.Folder), 
                               HostServices.CreateDisplayType("form", "Wufoo Form", EclItemTypes.File)
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
