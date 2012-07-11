using System.ServiceModel;
using Tridion.ContentManager.CoreService.Client;

namespace Social.Common.CoreService
{
    public class Client
    {
        public static SessionAwareCoreServiceClient GetCoreService()
        {
            var result = new SessionAwareCoreServiceClient();
            result.Impersonate(Tridion.Web.UI.Core.Utils.GetUserName());
            return result;
        }

        public static SessionAwareCoreServiceClient GetConfiglessCoreService()
        {
            string userName = Tridion.Web.UI.Core.Utils.GetUserName();

            var quotas = new System.Xml.XmlDictionaryReaderQuotas
            {
                MaxStringContentLength = 10485760,
                MaxArrayLength = 10485760,
                MaxBytesPerRead = 10485760
            };

            var httpBinding = new WSHttpBinding
            {
                MaxReceivedMessageSize = 10485760,
                ReaderQuotas = quotas,
                Security = { Mode = SecurityMode.Message, Transport = { ClientCredentialType = HttpClientCredentialType.Windows } }
            };

            var endpoint = new EndpointAddress("http://localhost/WebServices/CoreService2011.svc/wsHttp");
            var result = new SessionAwareCoreServiceClient(httpBinding, endpoint);
            result.Impersonate(userName);

            return result;
        }
    }
}
