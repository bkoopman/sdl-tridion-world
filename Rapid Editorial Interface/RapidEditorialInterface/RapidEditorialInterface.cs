using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Extensibility;
using Tridion.ContentManager.Extensibility.Events;
using Tridion.ContentManager.Publishing;
using Tridion.Logging;

namespace SDLTridion.Examples.EventSystem
{
    /// <summary>
    /// Rapid Editorial Interface event handler extension.
    /// On save (and check-in) of a Component, create a Page for that Component and update an index Page with the Component and publish both to a staging target.
    /// </summary>
    /// <remarks>
    /// This demo event system used to be known as the Rapid Editorial Interface.
    /// The metadata of the Folder the Component resides in, will be used as the configuration for the actions.
    /// </remarks>
    /// <author>
    ///     Bart Koopman (bkoopman@sdl.com), SDL - Web Content Management Solutions Division
    /// </author>
    /// <date>
    ///     created 23-August-2010
    ///     updated 25-August-2010
    /// </date>	
    [TcmExtension("RapidEditorialInterfaceEventHandlerExtension")]
    public class RapidEditorialInterface : TcmExtension
    {
        /// <summary>
        /// Subscribes to events
        /// </summary>
        public RapidEditorialInterface()
        {
            Subscribe();
        }

        /// <summary>
        /// Subscribe to Component, CheckIn, TransactionCommitted event
        /// </summary>
        private void Subscribe()
        {
            // TOM event was OnComponentSavePost, now we use the Component CheckIn TransactionCommited 
            // This ensures the Component is (saved and) checked in before we try to use it on a Page and publish it
            EventSystem.Subscribe<Component, CheckInEventArgs>(ComponentCheckInAction, EventPhases.TransactionCommitted);
        }

        /// <summary>
        /// On (Save, and) Check-in of a Component, create a Page for that Component and update an index Page with the Component and publish both to a staging target.
        /// </summary>
        /// <remarks>
        /// The metadata of the Folder the Component resides in, will be used as the configuration for the actions.
        /// </remarks>
        /// <param name="subject">checked in Component</param>
        /// <param name="args">check in event arguments</param>
        /// <param name="phase">event phase</param>
        private static void ComponentCheckInAction(Component subject, CheckInEventArgs args, EventPhases phase)
        {
            // get Folder from Component for configuration metadata
            Folder folder = (Folder)subject.OrganizationalItem;

            // proceed when Folder has metadata
            if (folder.Metadata == null) return;

            ItemFields metadata = new ItemFields(folder.Metadata, folder.MetadataSchema);
            ReiConfig config = new ReiConfig(metadata);

            // proceed when metadata contains valid URIs, and Schema of Component is recognised 
            if (!config.IsValid || subject.Schema.Id.ItemId != config.SchemaUri.ItemId) return;

            // create list of items to publish
            List<IdentifiableObject> items = new List<IdentifiableObject>();

            // if Component is already used on any Page then no need to create new Page and update index, just publish Component
            UsingItemsFilter pageFilter = new UsingItemsFilter(subject.Session) { ItemTypes = new List<ItemType> { ItemType.Page } };
            if (subject.HasUsingItems(pageFilter))
            {
                items.Add(subject);
            }
            else
            {
                // create Page and add Component Presentation (using Context Publication of Structure Group)
                TcmUri localUri = ReiConfig.TransformTcmUri(subject.Id, config.StructureGroupUri);
                Component localComponent = new Component(localUri, subject.Session);
                ComponentTemplate componentTemplate = new ComponentTemplate(config.ComponentTemplateUri, subject.Session);
                Page page = new Page(subject.Session, config.StructureGroupUri);
                try
                {
                    page.Title = subject.Title;
                    page.FileName = GetSafeFileName(subject.Title);
                    page.PageTemplate = new PageTemplate(config.PageTemplateUri, subject.Session);
                    page.ComponentPresentations.Add(new ComponentPresentation(localComponent, componentTemplate));
                    page.Save(true);

                    // add Page to publish items list
                    items.Add(page);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex, ReiConfig.Name, LoggingCategory.General, TraceEventType.Error);
                }

                // add Component to index Page (using Context Publication of index Page)
                localUri = ReiConfig.TransformTcmUri(subject.Id, config.IndexPageUri);
                localComponent = new Component(localUri, subject.Session);
                componentTemplate = new ComponentTemplate(config.IndexComponentTemplateUri, subject.Session);
                Page indexPage = new Page(config.IndexPageUri, subject.Session);
                try
                {
                    indexPage.CheckOut();
                    indexPage.ComponentPresentations.Add(new ComponentPresentation(localComponent, componentTemplate));
                    indexPage.Save(true);

                    // add index Page to publish items list
                    items.Add(indexPage);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex, ReiConfig.Name, LoggingCategory.General, TraceEventType.Error);
                }
            }

            // publish items
            if (items.Count > 0)
            {
                List<TargetType> targets = new List<TargetType> { new TargetType(config.TargetTypeUri, subject.Session) };
                PublishInstruction publishInstruction = new PublishInstruction(subject.Session);
                PublishEngine.Publish(items, publishInstruction, targets, ReiConfig.Priority);
            }
            else
            {
                Logger.Write("No items were published.", ReiConfig.Name, LoggingCategory.General, TraceEventType.Information);
            }
        }

        private static string GetSafeFileName(string fileName)
        {
            // replace all non standard characters with an underscore
            return System.Text.RegularExpressions.Regex.Replace(fileName, "[^a-zA-Z_]", "_");
        }
    }
}
