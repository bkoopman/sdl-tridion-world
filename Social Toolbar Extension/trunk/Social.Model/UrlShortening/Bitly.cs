using System;
using System.Net;
using System.IO;
using System.Web;
using System.Xml;

namespace Social.Model.UrlShortening
{
    public class Bitly
    {
        private const string shortenUrl = "http://api.bit.ly/v3/shorten?login=";
        private const string restoreUrl = "http://api.bit.ly/v3/expand?login=";
        private const string clicksUrl = "http://api.bit.ly/v3/clicks?login=";
        private const string verifyUrl = "http://api.bit.ly/v3/validate?x_login=";
        private string BitResponse = "";

        public string BitlyAPIKey { get; set; }
        public string BitlyLogin { get; set; }
        public string UrlToShorten { get; set; }

        public Bitly(string url)
        {
            /*if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key", "A valid Bit.ly API key is required");

            if (string.IsNullOrEmpty(login) || string.IsNullOrWhiteSpace(login))
                throw new ArgumentNullException("login", "A valid Bit.ly login is requied");

            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("url", "A valid url must be provided");

            this.BitlyAPIKey = key;
            this.BitlyLogin = login;
            this.UrlToShorten = url;*/

            this.BitlyLogin = Configuration.GetConfigString("bitlyuser");
            this.BitlyAPIKey = Configuration.GetConfigString("bitlykey");
            this.UrlToShorten = url;
        }

        public enum ResponseFormat : int
        {
            Text = 0,
            XML = 1,
            Json = 2
        }

        /// <summary>
        /// method for shortening a URL utilizing the Bitly API
        /// </summary>
        /// <param name="longURL">URL to shorten</param>
        /// <param name="login">Login for our account</param>
        /// <param name="key">our Bitly API Key</param>
        /// <param name="format">Thef format we wish to have returned to us</param>
        /// <returns></returns>
        public string ShortenUrl(string longURL, string login, string key, ResponseFormat format = ResponseFormat.XML)
        {
            //this will hold the shortened URL
            string output;          
 
            //build the full URL for shortening the URL
            string url = string.Format(shortenUrl + @"{0}&apiKey={1}&longUrl={2}&format={3}", 
                login, key, HttpUtility.UrlEncode(longURL), format.ToString().ToLower());
 
            //get the finaly shortened URL (this is an external method that you will see later
            return getFinalValue(url, out output);
        }
 

        private static string getFinalValue(string URL, out string output)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(URL);
 
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    output = reader.ReadToEnd();
                }
            }
 
            return output;
        }

        /// <summary>
        /// method for parsing the XML that is sent from the api call
        /// </summary>
        /// <param name="xml">the XML string tht was returned</param>
        /// <param name="expand">whether we are expanding a url or shortning one</param>
        /// <returns></returns>
        public static string ParseXmlResponse(string xml, bool expand)
        {
            //create an XML document
            XmlDocument xmlDoc = new XmlDocument();

            //load the XML string
            xmlDoc.LoadXml(xml);
            string url = string.Empty;

            //populate an XmlNodeList with SelectNodes of the XmlDocument
            XmlNodeList nodeList = expand ? xmlDoc.SelectNodes("/response/data/entry") : xmlDoc.SelectNodes("/response/data");

            //now find the shortened url (or extended is expand is set to true
            foreach (XmlNode node in nodeList)
                url = expand ? node["long_url"].InnerText : node["url"].InnerText;

            return url;
        }

        /// <summary>
        /// method for parsing Json and retrieving the shortened link.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        /*public static string ParseJsonResponse(string json)
        {
            var obj = JObject.Parse(json);

            if (obj["data"]["expand"] == null)
                return (string)obj["data"]["url"];
            else
                return (string)obj["data"]["expand"][0]["long_url"];
        }*/
    }
}