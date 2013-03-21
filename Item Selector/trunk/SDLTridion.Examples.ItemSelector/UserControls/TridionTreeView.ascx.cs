using System;
using System.ServiceModel;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Tridion.ContentManager;
using System.Configuration;
using Tridion.ContentManager.CoreService.Client;
using ItemType = Tridion.ContentManager.ItemType;

namespace SDLTridion.Examples.ItemSelector.UserControls
{
    public partial class TridionTreeView : System.Web.UI.UserControl
    {
        #region Constants
        // 2011 SP1 CME paths
        const string IconPath = "/WebUI/Editors/CME/Themes/Carbon/icon_v6.1.0.55920.1_.png";
        const string CollapseImage = "/WebUI/Editors/CME/Themes/Carbon/Images/Controls/Tree/TreeMinus1_v6.1.0.55920.1_.png";
        const string ExpandImage = "/WebUI/Editors/CME/Themes/Carbon/Images/Controls/Tree/TreePlus1_v6.1.0.55920.1_.png";

        #endregion

        #region Public Members
        /// <summary>
        /// Get or set the Windows account that the user is logged into.  
        /// </summary>
        public string LogonUser { get; set; }

        /// <summary>
        /// Get or set the URI to start the navigation tree from. 
        /// </summary>
        public string StartFromUri { get; set; }

        /// <summary>
        /// Get or set the Item Type which can be selected.
        /// </summary>
        /// <remarks>
        /// Item Types which are not part of the SelectTypes value will not be shown unless it is a organizational item
        /// Use 0 for all Item Types or combine multiple Item Types by their number, for example to select Folders and Components, use 2 + 16 = 18.
        /// Accepted Item Types are:
        /// Publication = 1
        /// Folder = 2
        /// StructureGroup = 4
        /// Schema = 8
        /// Component = 16
        /// ComponentTemplate = 32
        /// Page = 64
        /// PageTemplate = 128
        /// TargetGroup = 256
        /// Category = 512
        /// Keyword = 1024
        /// TemplateBuildingBlock = 2048
        /// VirtualFolder = 8192
        /// </remarks>
        public int SelectTypes { get; set; }

        /// <summary>
        /// Create an instance of the TridionTreeView object
        /// </summary>
        public TridionTreeView()
        {
            _nsMgr = new XmlNamespaceManager(new NameTable());
            _nsMgr.AddNamespace(Tridion.Constants.TcmPrefix, Tridion.Constants.TcmNamespace);
            _nsMgr.AddNamespace(Tridion.Constants.XlinkPrefix, Tridion.Constants.XlinkNamespace);
        }

        /// <summary>
        /// TreeNodePopulate event handler
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        public void PopulateNode(Object sender, TreeNodeEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            TcmUri uri = new TcmUri(e.Node.Value);
            if (uri.IsUriNull) // the root node
            {
                PublicationsFilterData filter = new PublicationsFilterData();
                XElement publications = _client.GetSystemWideListXml(filter);
                doc.Load(publications.CreateReader());
            }
            else
            {
                switch (uri.ItemType)
                {
                    case ItemType.Publication:
                        PublicationData pub = (PublicationData)_client.Read(uri.ToString(), new ReadOptions());
                        TreeNode tnF = CreateTreeNode(pub.RootFolder.Title, pub.RootFolder.IdRef);
                        TreeNode tnS = CreateTreeNode(pub.RootStructureGroup.Title, pub.RootStructureGroup.IdRef);
                        TreeNode tnC = CreateTreeNode("Categories and Keywords", string.Format("tcm:{0}-0-131200", uri.ItemId));
                        if (tnF != null)
                        {
                            e.Node.ChildNodes.Add(tnF);
                        }
                        if (tnS != null)
                        {
                            e.Node.ChildNodes.Add(tnS);
                        }
                        if (tnC != null)
                        {
                            e.Node.ChildNodes.Add(tnC);
                        }
                        break;
                    case ItemType.Folder:
                        XElement items = _client.GetListXml(uri.ToString(), new OrganizationalItemItemsFilterData { Recursive = false });
                        doc.Load(items.CreateReader());
                        break;
                    case ItemType.WorkItem: // fake type id, representing the categories & keywords node!
                        items = _client.GetListXml(string.Format("tcm:0-{0}-1", uri.PublicationId), new TaxonomiesFilterData());
                        doc.Load(items.CreateReader());
                        break;
                    case ItemType.StructureGroup:
                        items = _client.GetListXml(uri.ToString(), new OrganizationalItemItemsFilterData { Recursive = false });
                        doc.Load(items.CreateReader());
                        break;
                    case ItemType.Category:
                        items = _client.GetListXml(uri.ToString(), new KeywordsFilterData { IsRoot = true });
                        doc.Load(items.CreateReader());
                        break;
                    case ItemType.Keyword:
                        items = _client.GetListXml(uri.ToString(), new ChildKeywordsFilterData());
                        doc.Load(items.CreateReader());
                        break;
                }
            }

            foreach (XmlElement item in doc.SelectNodes("//tcm:Item", _nsMgr))
            {
                TreeNode tNode = CreateTreeNode(item.GetAttribute("Title"), item.GetAttribute("ID"));
                if (tNode != null)
                {
                    e.Node.ChildNodes.Add(tNode);
                }
            }
        }

        #endregion

        #region Protected Members
        protected override void OnLoad(EventArgs e)
        {
            Initialize();
            if (!IsPostBack)
            {
                base.OnLoad(e);
                if (!String.IsNullOrEmpty(StartFromUri))
                {
                    TcmUri uri = new TcmUri(StartFromUri);
                    switch (uri.ItemType)
                    {
                        case ItemType.Publication:
                            PublicationData pub = (PublicationData)_client.Read(StartFromUri, new ReadOptions());
                            EmbeddedTreeView.Nodes[0].Text = pub.Title;
                            break;
                        case ItemType.Folder:
                            FolderData fol = (FolderData)_client.Read(StartFromUri, new ReadOptions());
                            EmbeddedTreeView.Nodes[0].Text = fol.Title;
                            break;
                        case ItemType.StructureGroup:
                            StructureGroupData sg = (StructureGroupData)_client.Read(StartFromUri, new ReadOptions());
                            EmbeddedTreeView.Nodes[0].Text = sg.Title;
                            break;
                        case ItemType.Category:
                            CategoryData cat = (CategoryData)_client.Read(StartFromUri, new ReadOptions());
                            EmbeddedTreeView.Nodes[0].Text = cat.Title;
                            break;
                    }
                    // root node, disable select action (set to expand)
                    EmbeddedTreeView.Nodes[0].Value = StartFromUri;
                    EmbeddedTreeView.Nodes[0].ImageUrl = UriImage(uri);
                    EmbeddedTreeView.Nodes[0].SelectAction = TreeNodeSelectAction.Expand;
                    //EmbeddedTreeView.Nodes[0].Text += " (" + LogonUser + ")"; 

                    // translate the logical mask value for 'SelectItems' into a list of item types
                    _selectedItemTypes = new List<ItemType>();
                    _showItemTypes = new List<ItemType>();
                    foreach (int type in ItemTypeIds.Keys)
                    {
                        if (type == 0)
                        {
                            continue;
                        }

                        ItemType curType = ItemTypeIds[type];

                        // select types 0 means all (selecting none makes no sense)
                        if (SelectTypes == 0)
                        {
                            _selectedItemTypes.Add(curType);
                            _showItemTypes.Add(curType);
                            continue;
                        }
                        if ((SelectTypes & type) != type) continue;

                        _selectedItemTypes.Add(curType);
                        _showItemTypes.Add(curType);
                        switch (curType)
                        {
                            case ItemType.Publication:
                                break;
                            case ItemType.Folder:
                            case ItemType.StructureGroup:
                            case ItemType.Category:
                                if (!_showItemTypes.Contains(ItemType.Publication))
                                {
                                    _showItemTypes.Add(ItemType.Publication);
                                }
                                break;
                            case ItemType.Page:
                                if (!_showItemTypes.Contains(ItemType.StructureGroup))
                                {
                                    _showItemTypes.Add(ItemType.StructureGroup);
                                }
                                if (!_showItemTypes.Contains(ItemType.Publication))
                                {
                                    _showItemTypes.Add(ItemType.Publication);
                                }
                                break;
                            case ItemType.Keyword:
                                if (!_showItemTypes.Contains(ItemType.Category))
                                {
                                    _showItemTypes.Add(ItemType.Category);
                                }
                                if (!_showItemTypes.Contains(ItemType.Publication))
                                {
                                    _showItemTypes.Add(ItemType.Publication);
                                }
                                break;
                            default:
                                if (!_showItemTypes.Contains(ItemType.Folder))
                                {
                                    _showItemTypes.Add(ItemType.Folder);
                                }
                                if (!_showItemTypes.Contains(ItemType.Publication))
                                {
                                    _showItemTypes.Add(ItemType.Publication);
                                }
                                break;
                        }
                    }
                    // always add workitem type for categories and keyword section
                    if (SelectTypes != 0)
                    {
                        _showItemTypes.Add(ItemType.WorkItem);
                    }
                }
                Session["showitemtypes"] = _showItemTypes;
                Session["selecteditemtypes"] = _selectedItemTypes;
            }
            else
            {
                _showItemTypes = (List<ItemType>)Session["showitemtypes"];
                _selectedItemTypes = (List<ItemType>)Session["selecteditemtypes"];
            }
        }

        public override void Dispose()
        {
            if (_client.State == CommunicationState.Faulted)
            {
                _client.Abort();
            }
            else
            {
                _client.Close();
            }
            
            base.Dispose();
        }

        #endregion

        #region Private Members
        private SessionAwareCoreServiceClient _client;
        private List<ItemType> _selectedItemTypes;
        private List<ItemType> _showItemTypes;
        private string _tcmUrl;
        private readonly XmlNamespaceManager _nsMgr;

        /// <summary>
        /// Dictionary with Tridion.ContentManager.ItemType identified by their int value
        /// </summary>
        private static readonly Dictionary<int, ItemType> ItemTypeIds = new Dictionary<int, ItemType>
        {
            {0, ItemType.None},
            {1, ItemType.Publication},
            {2, ItemType.Folder},
            {4, ItemType.StructureGroup},
            {8, ItemType.Schema},
            {16, ItemType.Component},
            {32, ItemType.ComponentTemplate},
            {64, ItemType.Page},
            {128, ItemType.PageTemplate},
            {256, ItemType.TargetGroup},
            {512, ItemType.Category},
            {1024, ItemType.Keyword},
            {2048, ItemType.TemplateBuildingBlock},
            {8192, ItemType.VirtualFolder},
            {131200, ItemType.WorkItem}
        };

        /// <summary>
        /// Dictionary with int values idientified by their Tridion.ContentManager.ItemType representation
        /// </summary>
        private static readonly Dictionary<ItemType, int> ItemTypeValues = new Dictionary<ItemType, int>
        {
            {ItemType.None, 0},
            {ItemType.Publication, 1},
            {ItemType.Folder, 2},
            {ItemType.StructureGroup, 4},
            {ItemType.Schema, 8},
            {ItemType.Component, 16},
            {ItemType.ComponentTemplate, 32},
            {ItemType.Page, 64},
            {ItemType.PageTemplate, 128},
            {ItemType.TargetGroup, 256},
            {ItemType.Category, 512},
            {ItemType.Keyword, 1024},
            {ItemType.TemplateBuildingBlock, 2048},
            {ItemType.VirtualFolder, 8192},
            {ItemType.WorkItem, 131200}
        };

        /// <summary>
        /// Initialize TreeView by setting image url attributes based on Tridion url
        /// </summary>
        private void Initialize()
        {
            // use net.tcp core service client as we are on the machine itself
            var endpoint = new EndpointAddress(ConfigurationManager.AppSettings["endpointAddress"]);
            var binding = new NetTcpBinding
            {
                MaxReceivedMessageSize = 2147483647,
                ReaderQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxStringContentLength = 2147483647,
                    MaxArrayLength = 2147483647
                }
            };
            _client = new SessionAwareCoreServiceClient(binding, endpoint);
            // impersonate core service call with currently logged on user
            if (!String.IsNullOrEmpty(LogonUser))
            {
                _client.Impersonate(LogonUser);
            }

            _tcmUrl = ConfigurationManager.AppSettings["sdlTridionCmsUrl"];
            EmbeddedTreeView.ExpandImageUrl = _tcmUrl + ExpandImage;
            EmbeddedTreeView.CollapseImageUrl = _tcmUrl + CollapseImage;
        }

        private TreeNode CreateTreeNode(string nodeText, string nodeValue)
        {
            TcmUri uri = new TcmUri(nodeValue);
            if (uri.ItemType == ItemType.WorkItem)
            {
                if (!_showItemTypes.Contains(ItemType.Category))
                {
                    return null;
                }
            }

            if (!_showItemTypes.Contains(uri.ItemType))
            {
                return null;
            }

            TreeNode tNode = new TreeNode(nodeText, nodeValue) { Expanded = false, ImageUrl = UriImage(uri) };

            switch (uri.ItemType)
            {
                case ItemType.Publication:
                case ItemType.Folder:
                case ItemType.StructureGroup:
                case ItemType.Category:
                case ItemType.Keyword:
                case ItemType.WorkItem:
                    tNode.PopulateOnDemand = true;
                    break;
                default:
                    tNode.PopulateOnDemand = false;
                    break;
            }

            if (!_selectedItemTypes.Contains(uri.ItemType))
            {
                // disable select action (set to expand)
                tNode.SelectAction = TreeNodeSelectAction.Expand;
            }
            else
            {
                // call javascript from navigate url to prevent post (post will loose window.dialogarguments)
                tNode.NavigateUrl = "javascript:setvalue('" + nodeValue + "');";
            }

            return tNode;
        }

        private string UriImage(TcmUri uri)
        {
            switch (uri.ItemType)
            {
                case ItemType.None:
                    return _tcmUrl + IconPath + "?name=T0&size=16";
                case ItemType.WorkItem:
                    // abuse workitem type for categories and keyword section
                    return _tcmUrl + IconPath + "?name=cme-catman&size=16";
                default:
                    return _tcmUrl + IconPath + "?name=T" + GetItemType(uri).ToString() + "L0P0S2&size=16";
            }
        }

        private static int GetPublicationId(TcmUri uri)
        {
            // for publication uris use the item id (this contains the publication id)
            return uri.ItemType == ItemType.Publication ? uri.ItemId : uri.PublicationId;
        }

        private static int GetItemType(TcmUri uri)
        {
            int value;
            return ItemTypeValues.TryGetValue(uri.ItemType, out value) ? value : 0;
        }

        #endregion
    }
}