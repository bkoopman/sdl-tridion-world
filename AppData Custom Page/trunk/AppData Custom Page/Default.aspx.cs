using System;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Tridion.ContentManager.CoreService.Client;

namespace Example.CustomPage
{
    public partial class Default : Page
    {
        #region constants
        // services endpoint and binding
        private readonly NetTcpBinding _binding = new NetTcpBinding
        {
            MaxReceivedMessageSize = 2147483647,
            ReaderQuotas = new XmlDictionaryReaderQuotas
            {
                MaxStringContentLength = 2147483647,
                MaxArrayLength = 2147483647
            }
        };
        #endregion

        private EndpointAddress _coreServiceEndpoint;
        private string _vocabulariesAppId;
        private string _typeOfAppId;
        private string _schemaPublicationUri;

        protected void Page_Load(object sender, EventArgs e)
        {
            _coreServiceEndpoint = new EndpointAddress(WebConfigurationManager.AppSettings["EndpointAddress"]);
            _vocabulariesAppId = WebConfigurationManager.AppSettings["VocabulariesAppId"];
            _typeOfAppId = WebConfigurationManager.AppSettings["TypeOfAppId"];
            _schemaPublicationUri = WebConfigurationManager.AppSettings["SchemaPublicationUri"];

            if (!IsPostBack)
            {
                var client = new SessionAwareCoreServiceClient(_binding, _coreServiceEndpoint);
                client.Impersonate(HttpContext.Current.User.Identity.Name);

                // load vocabularies appdata
                ApplicationData appData = client.ReadApplicationData(null, _vocabulariesAppId);
                if (appData != null)
                {
                    AppData.Text = Encoding.Unicode.GetString(appData.Data);
                }
                else
                {
                    // load default xml
                    AppData.Text = "<vocabularies>\n  <vocabulary prefix=\"s\" name=\"http://schema.org\"/>\n</vocabularies>";
                }

                // load schemas
                var filter = new RepositoryItemsFilterData { ItemTypes = new[] { ItemType.Schema }, Recursive = true };
                var schemas = client.GetList(_schemaPublicationUri, filter);
                foreach (var schema in schemas)
                {
                    SchemaList.Items.Add(new ListItem(schema.Title, schema.Id));
                }

                // load appdata for first schema in the list
                appData = client.ReadApplicationData(schemas[0].Id, _typeOfAppId);
                if (appData != null)
                {
                    TypeOf.Text = Encoding.Unicode.GetString(appData.Data);
                }

                Close(client);
            }
        }

        protected void SchemaListChanged(object sender, EventArgs e)
        {
            var client = new SessionAwareCoreServiceClient(_binding, _coreServiceEndpoint);
            client.Impersonate(HttpContext.Current.User.Identity.Name);

            ApplicationData appData = client.ReadApplicationData(SchemaList.SelectedItem.Value, _typeOfAppId);
            if (appData != null)
            {
                TypeOf.Text = Encoding.Unicode.GetString(appData.Data);
            }
            else
            {
                // reset textbox
                TypeOf.Text = String.Empty;
            }

            Close(client);

            SchemaLabel.Text = SchemaList.SelectedItem.Value;
        }

        protected void UpdateSchemaClick(object sender, EventArgs e)
        {
            var client = new SessionAwareCoreServiceClient(_binding, _coreServiceEndpoint);
            client.Impersonate(HttpContext.Current.User.Identity.Name);

            var appData = new ApplicationData
            {
                ApplicationId = _typeOfAppId,
                Data = Encoding.Unicode.GetBytes(TypeOf.Text)
            };
            client.SaveApplicationData(SchemaList.SelectedItem.Value, new[] { appData });

            Close(client);

            SchemaLabel.Text = SchemaList.SelectedItem.Value + " saved...";
        }

        protected void UpdateVocabsClick(object sender, EventArgs e)
        {
            var client = new SessionAwareCoreServiceClient(_binding, _coreServiceEndpoint);
            client.Impersonate(HttpContext.Current.User.Identity.Name);

            var appData = new ApplicationData
            {
                ApplicationId = _vocabulariesAppId,
                Data = Encoding.Unicode.GetBytes(AppData.Text)
            };
            client.SaveApplicationData(null, new[] { appData });

            Close(client);

            VocabsLabel.Text = "saved...";
        }

        private static void Close(ICommunicationObject client)
        {
            try
            {
                if (client.State != CommunicationState.Faulted)
                {
                    client.Close();
                }
            }
            finally
            {
                if (client.State != CommunicationState.Closed)
                {
                    client.Abort();
                }
            }
        }
    }
}