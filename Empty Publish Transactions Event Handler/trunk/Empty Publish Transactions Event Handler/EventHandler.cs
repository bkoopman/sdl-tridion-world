using System.Configuration;
using System.Xml;
using Tridion;
using Tridion.ContentManager;
using Tridion.ContentManager.Extensibility;
using Tridion.ContentManager.Extensibility.Events;
using Tridion.ContentManager.Publishing;

namespace Example
{
    /// <summary>
    /// Change the state Success of an empty Publish Transaction to Warning
    /// </summary>
    [TcmExtension("EmptyPublishTransactionsEventHandlerExtension")]
    public class EventHandler : TcmExtension
    {
        /// <summary>
        /// R6 Delta XML PublishTransactionData
        /// </summary>
        private const string DeltaXml = "<tcm:PublishTransactionData xmlns:tcm=\"{0}\"><tcm:Id>{1}</tcm:Id><tcm:Information>{2}</tcm:Information><tcm:State>{3}</tcm:State></tcm:PublishTransactionData>";

        /// <summary>
        /// Warning message for empty Publish Transaction
        /// </summary>
        private const string Message = "This Publish Transaction contains 0 (zero) items.";

        /// <summary>
        /// Child Publications Only Resolver Config filename
        /// </summary>
        private const string ConfigFile = "ChildPublicationsOnlyResolver.config";

        /// <summary>
        /// Website Structure Publication TCMURI in config file
        /// </summary>
        private const string WebsiteStructurePublicationFieldName = "WebsiteStructurePublication";

        private readonly TcmUri _websiteStructurePublicationUri;

        /// <summary>
        /// Subscribe to PublishTransaction Save (Initiated) event 
        /// </summary>
        public EventHandler()
        {
            _websiteStructurePublicationUri = GetWebsiteStructurePublicationUri();

            EventSystem.Subscribe<PublishTransaction, SaveEventArgs>(PublishTransactionSaveAction, EventPhases.Initiated);
        }

        private void PublishTransactionSaveAction(PublishTransaction subject, SaveEventArgs args, EventPhases phases)
        {
            // ignore items in website structure publication (they are always empty because of the child publications only resolver)
            if (subject.PublishContexts[0].Publication.Id.ItemId == _websiteStructurePublicationUri.ItemId)
            {
                return;
            }
            
            // after the publisher has used the Resolve Engine to figure out what to publish/render or unpublish, 
            // it reflects the resolved results in PublishTransaction.PublishContexts[0].ProcessedItems
            // currently there will always be exactly one PublishContext per PublishTransaction
            if (subject.PublishContexts[0].ProcessedItems.Count == 0 && subject.State.Equals(PublishTransactionState.Success))
            {
                // change state to warning and inform that this publish transaction had 0 items 
                // could change state by setting property PublishTransaction.State, 
                // but the property PublishTransaction.Information is readonly, so let's use IdentifiableObject.Update 
                XmlDocument delta = new XmlDocument();
                delta.LoadXml(string.Format(DeltaXml, Constants.TcmR6Namespace, subject.Id, Message, PublishTransactionState.Warning));
                subject.Update(delta.DocumentElement);
            }
        }

        /// <summary>
        /// Read website structure Publication URI from config file (located in ..\Tridion\config\ directory)
        /// </summary>
        /// <returns>Website structure Publication URI</returns>
        private static TcmUri GetWebsiteStructurePublicationUri()
        {
            // read website structure publication uri from config file (located in ..\Tridion\config\ directory)
            string configPath = Tridion.ContentManager.ConfigurationSettings.GetTcmHomeDirectory();
            if (configPath.EndsWith("\\"))
            {
                configPath += "config\\";
            }
            else
            {
                configPath += "\\config\\";
            }
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = configPath + ConfigFile };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            return new TcmUri(config.AppSettings.Settings[WebsiteStructurePublicationFieldName].Value);
        }
    }
}
