using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using Tridion.ContentManager;
using Tridion.ContentManager.Publishing;
using Tridion.ContentManager.Publishing.Resolving;
using Tridion.Logging;

namespace Example
{
    /// <summary>
    /// Resolver to strip out items from website structure publication
    /// </summary>
    public class ChildPublicationsOnlyResolver : IResolver
    {
        /// <summary>
        /// Config filename
        /// </summary>
        private const string ConfigFile = "ChildPublicationsOnlyResolver.config";

        /// <summary>
        /// Website Structure Publication TCMURI in config file
        /// </summary>
        private const string WebsiteStructurePublicationFieldName = "WebsiteStructurePublication";

        private readonly TcmUri _websiteStructurePublicationUri;

        public ChildPublicationsOnlyResolver()
        {
            _websiteStructurePublicationUri = GetWebsiteStructurePublicationUri();
        }

        /// <summary>
        /// For Publish and UnPublish, remove all items from the website structure Publication from the list.
        /// Website structure Publication URI is read from the config file.
        /// </summary>
        /// <param name="item">Item to be resolved (e.g. a page, structure group, template)</param>
        /// <param name="instruction">Resolve instruction</param>
        /// <param name="context">Publish context</param>
        /// <param name="resolvedItems">List of items that are currently to be rendered and published (added by previous resolvers in the chain)</param>
        public void Resolve(IdentifiableObject item, ResolveInstruction instruction, PublishContext context, Tridion.Collections.ISet<ResolvedItem> resolvedItems)
        {
            List<ResolvedItem> itemsToRemove = new List<ResolvedItem>();
            StringBuilder infoMessage = new StringBuilder();
            infoMessage.AppendLine(string.Format("Removed the following items from a {0} Transaction to {1}:", instruction.Purpose, context.PublicationTarget.Title));

            // check for items from website structure publication (these do not need to be published or unpublished) 
            foreach (ResolvedItem resolvedItem in resolvedItems)
            {
                // mark all items from website structure publication for removal
                if (resolvedItem.Item.Id.PublicationId == _websiteStructurePublicationUri.ItemId)
                {
                    itemsToRemove.Add(resolvedItem);
                }
            }

            // remove all items that we need to discard
            foreach (ResolvedItem itemToRemove in itemsToRemove)
            {
                infoMessage.AppendLine(string.Format("{0}: {1} ({2})", itemToRemove.Item.Id.ItemType, itemToRemove.Item.Title, itemToRemove.Item.Id));
                resolvedItems.Remove(itemToRemove);
            }
            if (itemsToRemove.Count > 0)
            {
                // log info mesage about which items have been removed (optionally this can be logged as a warning to stand out in the logfile)
                Logger.Write(infoMessage.ToString(), "ChildPublicationsOnlyResolver", LoggingCategory.General, TraceEventType.Information);
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
