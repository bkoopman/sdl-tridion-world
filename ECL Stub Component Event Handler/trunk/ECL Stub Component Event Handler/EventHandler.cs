using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Extensibility;
using Tridion.ContentManager.Extensibility.Events;
using Tridion.ExternalContentLibrary.V2;
using Tridion.Logging;

namespace Example
{
    [TcmExtension("EclStubComponentEventHandlerExtension")]
    public class EventHandler : TcmExtension
    {
        // location and name of ECL config file
        private const string EclConfigFile = @"{0}\config\ExternalContentLibrary.xml";

        // ECL configuration namespace
        private static readonly XNamespace EclNs = "http://www.sdltridion.com/ExternalContentLibrary/Configuration";

        private readonly string _metadataXmlFieldName;
        private readonly string _mountpointType;
        private readonly bool _asynchronous;
        private readonly bool _update;
        private List<string> MountPointIds { get; set; }

        public EventHandler()
        {
            // read app.config
            var appConfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _metadataXmlFieldName = appConfig.AppSettings.Settings["MetadataXmlFieldName"].Value;
            _mountpointType = appConfig.AppSettings.Settings["MountpointType"].Value;
            _asynchronous = Convert.ToBoolean(appConfig.AppSettings.Settings["Asynchronous"].Value);
            _update = Convert.ToBoolean(appConfig.AppSettings.Settings["Update"].Value);

            // build list of media manager mountpoint ids
            MountPointIds = new List<string>();

            // read ExternalContentLibrary.xml from ..\Tridion\config folder
            XDocument eclConfig = XDocument.Load(string.Format(EclConfigFile, ConfigurationSettings.GetTcmHomeDirectory()));

            // loop over all mountpoints with configured type (could be multiple mountpoints) and add its id to our list
            foreach (XElement mountpoint in eclConfig.Descendants(EclNs + "MountPoint").Where(mountpoint => mountpoint.Attribute("type").Value.Equals(_mountpointType)))
            {
                MountPointIds.Add(mountpoint.Attribute("id").Value);
            }

            if (_asynchronous)
            {
                // subscribe to component checkin for asynchronous event (creates an extra version of the component, but has no performance impact on save)
                EventSystem.SubscribeAsync<Component, CheckInEventArgs>(SetOrUpdateMetadata, EventPhases.TransactionCommitted);
            }
            else
            {
                // subscribe to component save for synchronous event (has performance impact on save, only use if custom code is fast)
                EventSystem.Subscribe<Component, SaveEventArgs>(SetOrUpdateMetadata, EventPhases.Processed);
            }
        }

        private void SetOrUpdateMetadata(Component subject, EventArgs args, EventPhases phase)
        {
            // quick first test for ECL stub Component
            if (!subject.Title.StartsWith("ecl:") || subject.ComponentType != ComponentType.Multimedia) return;

            using (IEclSession eclSession = SessionFactory.CreateEclSession(subject.Session))
            {
                // determine if subject is an ECL stub Component from the list of available mountpoints
                IEclUri eclUri = eclSession.TryGetEclUriFromTcmUri(subject.Id);
                if (eclUri != null && MountPointIds.Contains(eclUri.MountPointId))
                {
                    // check if metadata field exists
                    ItemFields metadataFields = new ItemFields(subject.Metadata, subject.MetadataSchema);
                    if (metadataFields.Contains(_metadataXmlFieldName))
                    {
                        // only set value when update is true or metadata is not set
                        string metadata = ((SingleLineTextField)metadataFields[_metadataXmlFieldName]).Value;
                        if (_update || string.IsNullOrEmpty(metadata))
                        {
                            using (IContentLibraryContext context = eclSession.GetContentLibrary(eclUri))
                            {
                                // load actual ECL item so you can access its properties and metadata
                                IContentLibraryMultimediaItem eclItem = (IContentLibraryMultimediaItem) context.GetItem(eclUri);
                                try
                                {
                                    // implement your custom code here to set the metadata value
                                    // currently this reads the configured ECL metadata field and sets its value in the stub metadata
                                    if (!string.IsNullOrEmpty(eclItem.MetadataXml))
                                    {
                                        XNamespace ns = GetNamespace(eclItem.MetadataXml);
                                        XDocument eclMetadata = XDocument.Parse(eclItem.MetadataXml);

                                        XElement field = (from xml in eclMetadata.Descendants(ns + _metadataXmlFieldName) select xml).FirstOrDefault();
                                        if (field != null)
                                        {
                                            string value = field.Value;

                                            // only save value when metadata is empty or update is true and value differs
                                            if (string.IsNullOrEmpty(metadata) || (_update && !metadata.Equals(value)))
                                            {
                                                // update metadata
                                                if (_asynchronous)
                                                {
                                                    subject.CheckOut();
                                                }
                                                ((SingleLineTextField) metadataFields[_metadataXmlFieldName]).Value = value;
                                                subject.Metadata = metadataFields.ToXml();
                                                subject.Save();
                                                if (_asynchronous)
                                                {
                                                    subject.CheckIn();
                                                }

                                                Logger.Write(string.Format("added {0} to metadata of {1}", value, eclUri), "EclStubComponentEventHandlerExtension", LoggingCategory.General, TraceEventType.Information);
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Write(e, "EclStubComponentEventHandlerExtension", LoggingCategory.General);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string GetNamespace(string xml)
        {
            // ecl metadata string looks like <Metadata xmlns="..">..</Metadata>
            const string nsAttribute = "xmlns=\"";
            int start = xml.IndexOf(nsAttribute, StringComparison.Ordinal);
            int end = xml.IndexOf('\"', start + nsAttribute.Length);
            return xml.Substring(start + nsAttribute.Length, end - start - nsAttribute.Length);
        }
    }
}
