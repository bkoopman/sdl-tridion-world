using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Extensibility;
using Tridion.ContentManager.Extensibility.Events;
using Tridion.ContentManager.Publishing;
using Tridion.Localization;
using Tridion.Logging;

namespace Example
{
    [TcmExtension("ExampleEventHandlerExtension")]
    public class EventHandler : TcmExtension
    {
        public EventHandler()
        {
            EventSystem.Subscribe<Page, PublishOrUnPublishEventArgs>(PagePublishOrUnpublishAction, EventPhases.Initiated);
            EventSystem.Subscribe<ComponentTemplate, LoadEventArgs>(ComponentTemplateCreate, EventPhases.Initiated);
            EventSystem.Subscribe<Page, LoadEventArgs>(OnPageCreate, EventPhases.Processed);
            EventSystem.Subscribe<Component, LoadEventArgs>(ComponentLoadAction, EventPhases.TransactionCommitted);
        }

        public static void ComponentLoadAction(Component subject, LoadEventArgs args, EventPhases phases)
        {
            try
            {
                // the TCMURI of the Publication from this item
                TcmUri contextRepository = subject.ContextRepository.Id;

                // the TCMURI of the Publication where this item was localized
                TcmUri owningRepository = subject.OwningRepository.Id;

                Logger.Write(string.Format("Component: {0}, Context: {1}, Owning: {2}", subject.Id, contextRepository, owningRepository), "ExampleEventHandlerExtension", LoggingCategory.General, TraceEventType.Information);

                if (subject.IsLocalized)
                {
                    BluePrintChainFilter filter = new BluePrintChainFilter(subject.Session)
                    {
                        Direction = BluePrintChainDirection.Up
                    };

                    // the TCMURI of the Publication where this item was created
                    owningRepository = new TcmUri(subject.GetBluePrintChain(filter).Last().Id.ContextRepositoryId, ItemType.Publication);
                }

                // build TcmUri of parent Component
                // (note using ItemId of owningRepositiory as a PublicationId, 
                //  since that is a Publication TcmUri)
                TcmUri uri = new TcmUri(subject.Id.ItemId, subject.Id.ItemType, owningRepository.ItemId);

                // load parent Component
                Component parent = new Component(uri, subject.Session);

                Logger.Write(string.Format("Component: {0}, Context: {1}, Owning: {2}, Parent: {3}", subject.Id, contextRepository, owningRepository, parent.Id), "ExampleEventHandlerExtension", LoggingCategory.General, TraceEventType.Information);

                Component check = GetParentComponent(subject);
                Logger.Write(string.Format("GetParentComponent: {0}", check.Id), "ExampleEventHandlerExtension", LoggingCategory.General, TraceEventType.Information);
            }
            catch (Exception e)
            {
                Logger.Write(e, "ExampleEventHandlerExtension", LoggingCategory.General, TraceEventType.Error);
            }
        }

        // dodgy code from TRex, doesn't really give me the parent in all cases...
        private static Component GetParentComponent(IdentifiableObject component)
        {
            UsedItemsFilter usedItemFilter = new UsedItemsFilter(component.Session)
            {
                IncludeBlueprintParentItem = true,
                ItemTypes = new[] { ItemType.Component },
                BaseColumns = ListBaseColumns.Id
            };
            IEnumerable<IdentifiableObject> usedItems = component.GetUsedItems(usedItemFilter);
            if (usedItems == null)
            {
                return null;
            }

            usedItems = usedItems.Where(usedItem => usedItem.Id.ItemId.Equals(component.Id.ItemId));
            if (usedItems == null)
            {
                return null;
            }

            return (Component)usedItems.First();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OnPageCreate(Page page, LoadEventArgs args, EventPhases phases)
        {
            // only react on new Pages
            if (page.Id == TcmUri.UriNull)
            {
                StackTrace stackTrace = new StackTrace();
                foreach (var frame in stackTrace.GetFrames())
                {
                    // build Class.Method
                    MethodBase method = frame.GetMethod();
                    string name = method.ReflectedType.Name + "." + method.Name;

                    // only trigger on CoreServiceBase.GetDefaultData, not on CoreServiceBase.Create (which is the second call)
                    if (name.Equals("CoreServiceBase.GetDefaultData")) break;
                    if (name.Equals("CoreServiceBase.Create")) return;
                }

                const string metadataFieldName = "relatedContent";
                // bad example of hardcoding TCMURIs, but we at least use the context Publication
                string pageMetadataSchemaUri = string.Format("tcm:{0}-209-8", page.OrganizationalItem.Id.PublicationId);
                string seoSchemaUri = string.Format("tcm:{0}-520-8", page.OrganizationalItem.Id.PublicationId);
                string seoFolderUri = string.Format("tcm:{0}-1-2", page.OrganizationalItem.Id.PublicationId);

                if (page.MetadataSchema == null)
                {
                    // set Page metadata Schema
                    Schema meta = (Schema)page.Session.GetObject(pageMetadataSchemaUri);
                    page.MetadataSchema = meta;
                    page.Metadata = meta.GetInstanceData(page.OrganizationalItem, LoadFlags.Expanded).Metadata;

                    Logger.Write(string.Format("Added metadata to Page {0} ({1})", page.Title, page.Id), "ExampleEventHandlerExtension", LoggingCategory.General, TraceEventType.Information);
                }

                // check if Component hasn't already been set
                ItemFields pageMeta = new ItemFields(page.Metadata, page.MetadataSchema);
                if (pageMeta.Contains(metadataFieldName))
                {
                    ComponentLinkField field = (ComponentLinkField)pageMeta[metadataFieldName];
                    if (field != null && field.Value == null)
                    {
                        // create a new SEO Component
                        Folder folder = (Folder)page.Session.GetObject(seoFolderUri);
                        Schema seoSchema = (Schema)page.Session.GetObject(seoSchemaUri);
                        Component comp = folder.GetNewObject<Component>();
                        comp.Title = string.Format("Auto SEO Component {0}", Guid.NewGuid().ToString("N"));
                        comp.Schema = seoSchema;
                        comp.Content = seoSchema.GetInstanceData(folder, LoadFlags.Expanded).Content;
                        comp.Save(true);
                        Logger.Write(string.Format("Created Component {0} ({1})", comp.Title, comp.Id), "ExampleEventHandlerExtension", LoggingCategory.General, TraceEventType.Information);

                        // set link in Page metadata
                        field.Value = comp;
                        page.Metadata = pageMeta.ToXml();

                        Logger.Write(string.Format("Added Component Link to metadata of Page {0} ({1})", page.Title, page.Id), "ExampleEventHandlerExtension", LoggingCategory.General, TraceEventType.Information);
                    }
                }
            }
        }

        private static void ComponentTemplateCreate(ComponentTemplate ct, LoadEventArgs args, EventPhases phase)
        {
            if (ct.Version != 0) { return; }

            // add your TBBs here

        }

        private static void PagePublishOrUnpublishAction(Page page, PublishOrUnPublishEventArgs args, EventPhases phase)
        {
            if (page.Title.ToLower().Contains("do not publish"))
            {
                throw new PublisherException(new LocalizableMessage(Properties.Resources.ResourceManager, "DoNotPublishError", new object[] { page }));
            }
        }
    }
}
