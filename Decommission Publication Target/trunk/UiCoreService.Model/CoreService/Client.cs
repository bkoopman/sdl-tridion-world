using Tridion.ContentManager.CoreService.Client;

namespace Example.UiCoreService.Model.CoreService
{
    public class Client
    {
        public static SessionAwareCoreServiceClient GetCoreService()
        {
            var client = new SessionAwareCoreServiceClient("netTcp_2013");
            client.Impersonate(Tridion.Web.UI.Core.Utils.GetUserName());
            return client;
        }
    }
}