using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

using Social.Model.CoreService;
using Social.Model.Response;
using Social.Model.UrlShortening;
using Tridion.ContentManager.CoreService.Client;

namespace Social.Model.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceContract(Namespace = "Social.Model.Services")]
    public class GetPageData
    {
        private const string SHORT_URL_APP_ID = "ShortUrl";

        [OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]
        public SocialPageData Execute(string pageUri)
        {
            SessionAwareCoreServiceClient client = null;
            SocialPageData socialPageData = new SocialPageData();

            try
            {
                client = Client.GetCoreService();

                string liveTargetUri = Configuration.GetConfigString("livetargeturi");
                string liveUrl = Configuration.GetConfigString("liveurl");

                PageData pageData = (PageData)client.Read(pageUri, new ReadOptions());
                PublishLocationInfo pubInfo = (PublishLocationInfo)pageData.LocationInfo;

                socialPageData.Title = pageData.Title;
                socialPageData.Uri = pageUri;
                socialPageData.Url = liveUrl + pubInfo.PublishLocationUrl;
                socialPageData.IsPublished = client.IsPublished(pageUri, liveTargetUri, true);
                socialPageData.UseShortUrl = bool.Parse(Configuration.GetConfigString("shorturl"));

                string shortUrl = string.Empty;

                if (socialPageData.UseShortUrl)
                {
                    ApplicationData appData = client.ReadApplicationData(pageUri, SHORT_URL_APP_ID);

                    if (appData != null)
                    {
                        Byte[] data = appData.Data;
                        shortUrl = Encoding.Unicode.GetString(data);
                    }

                    if (shortUrl.Equals(string.Empty))
                    {
                        Bitly service = new Bitly(socialPageData.Url);
                        string shorter = service.ShortenUrl(service.UrlToShorten, service.BitlyLogin, service.BitlyAPIKey);
                        shortUrl = Bitly.ParseXmlResponse(shorter, false);

                        Byte[] byteData = Encoding.Unicode.GetBytes(shortUrl);
                        appData = new ApplicationData
                        {
                            ApplicationId = SHORT_URL_APP_ID,
                            Data = byteData,
                            TypeId = shortUrl.GetType().ToString()
                        };

                        client.SaveApplicationData(pageUri, new[] { appData });
                    }
                }

                socialPageData.ShortUrl = shortUrl;
            }
            catch (Exception ex)
            {
                socialPageData.HasError = true;
                socialPageData.ErrorInfo = ex;
            }
            finally
            {
                if (client != null)
                {
                    if (client.State == CommunicationState.Faulted)
                    {
                        client.Abort();
                    }
                    else
                    {
                        client.Close();
                    }
                }
            }

            return socialPageData;
        }
    }
}
