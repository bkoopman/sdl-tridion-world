using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace EclProvider.Wufoo.Api
{
    /// <summary>
    /// A light weight class to communicate with Wufoo's API basic features.
    /// </summary>
    /// <example>
    /// This sample shows how to use this Wufoo libary:
    /// <code>
    /// // Instantiate the WufooWrapper class
    /// WufooWrapper wufoo = new WufooWrapper("YourApiKey", "YourUserName");
    /// 
    /// // Get a list of your forms
    /// List&lt;FormData&gt; forms = wufoo.GetForms(); 
    /// 
    /// // Get the URL for a particular form 
    /// string formUrl = forms[0].Url;
    /// </code>
    /// </example>
    public class WufooWrapper
    {
        private const string DefaultUrl = "https://{0}.wufoo.com/api/v3/";

        public const string UserManagerUrl = "https://{0}.wufoo.com/users/";
        public const string FormManagerUrl = "https://{0}.wufoo.com/admin/";
        public const string FormEditUrl = "https://{0}.wufoo.com/build/{1}/";
        public const string FormUrl = "https://{0}.wufoo.com/forms/{1}/";

        #region properties
        public string ApiUrl { get; private set; }
        public string ApiKey { get; private set; }
        public string UserName { get; private set; }
        #endregion

        private List<string> _users;
        private List<WufooData> _userList;

        #region constructors
        /// <summary>
        /// Creates an instance of the Wufoo class. 
        /// </summary>
        /// <param name="url">Wufoo's API URL</param>
        /// <param name="apiKey">Your API key with Wufoo. 
        /// See Wufoo's API documentation at http://help.wufoo.com/articles/en_US/SurveyMonkeyArticleType/Wufoo-REST-API-V3#Findingthekey </param>
        /// <param name="userName">Your Wufoo username.</param>
        public WufooWrapper(string url, string apiKey, string userName)
        {
            ApiUrl = url;
            ApiKey = apiKey;
            UserName = userName;

            // initialize user lists
            GetUsers();
        }

        /// <summary>
        /// Creates an instance of the Wufoo class. 
        /// This method has the URL to Wufoo hardcoded to https://UserName.wufoo.com/api/v3/
        /// </summary>
        /// <param name="apiKey">Your API key with Wufoo. 
        /// See Wufoo's API documentation at http://help.wufoo.com/articles/en_US/SurveyMonkeyArticleType/Wufoo-REST-API-V3#Findingthekey </param>
        /// <param name="userName">Your Wufoo username.</param>
        public WufooWrapper(string apiKey, string userName) : this(DefaultUrl, apiKey, userName)
        {
        }
        #endregion

        #region public members
        /// <summary>
        /// Returns the form data.
        /// </summary>
        /// <param name="user">Name of the user this form belongs to</param>
        /// <param name="hash">Hash of the form</param>
        /// <returns>Form data object</returns>
        public WufooData GetForm(string user, string hash)
        {
            if (_users.Contains(user))
            {
                WufooData userData = _userList[_users.IndexOf(user)];
                string url = string.Format("{0}forms/{1}.xml", ApiUrl, hash);
                string response = SubmitRequest(url, user, userData.ApiKey);

                XElement responseXml = XElement.Parse(response);
                XElement formElement = responseXml.Element("Form");
                if (formElement != null)
                {
                    WufooData form = new WufooData(formElement, user, UserName);
                    return form;
                }
            }
            return null;
        }

        /// <summary>
        /// Get fields XML for a form based on given LinkFields url.
        /// </summary>
        /// <param name="url">LinkFields url</param>
        /// <param name="user">Name of the user.</param>
        /// <returns>String containing fields XML</returns>
        public string GetFields(string url, string user)
        {
            StringBuilder fields = new StringBuilder();
            if (_users.Contains(user))
            {
                WufooData userData = _userList[_users.IndexOf(user)];
                string response = SubmitRequest(url, userData.ApiKey);
                XElement xml = XElement.Parse(response);
                IEnumerable<XElement> fieldsXml = xml.Elements("Field");
                foreach (var field in fieldsXml)
                {
                    // ignore EntryId, DateCreated, CreatedBy, LastUpdated and UpdatedBy fields (those are not real form fields, only part of an entry)
                    string id = field.Element("ID").Value;
                    if (!id.Equals("EntryId") && !id.Equals("DateCreated") && !id.Equals("CreatedBy") && !id.Equals("LastUpdated") && !id.Equals("UpdatedBy"))
                    {
                        fields.Append("<Field>");

                        // append ID, Title and Type (putting them in the desired order)
                        fields.Append(field.Element("ID"));
                        fields.Append(field.Element("Title"));
                        fields.Append(field.Element("Type"));
                        fields.Append(field.Element("Instructions"));
                        fields.Append(field.Element("IsRequired"));
                        fields.Append(field.Element("ClassNames"));
                        fields.Append(field.Element("DefaultVal"));
                        fields.Append(field.Element("Page"));

                        // append subfields
                        if (field.Element("SubFields") != null)
                        {
                            foreach (var child in field.Element("SubFields").Elements())
                            {
                                fields.Append(child);
                            }
                        }

                        // append choises
                        if (field.Element("Choices") != null)
                        {
                            foreach (var child in field.Element("Choices").Elements())
                            {
                                fields.Append(child);
                            }

                            fields.Append(field.Element("HasOtherField"));
                        }

                        fields.Append("</Field>");
                    }
                }
            }
            return fields.ToString();
        }

        /// <summary>
        /// Get entries XML for a form based on given LinkEntries url.
        /// </summary>
        /// <param name="url">LinkEntries url</param>
        /// <param name="user">Name of the user.</param>
        /// <returns>String containing entries XML</returns>
        public string GetEntries(string url, string user)
        {
            StringBuilder entries = new StringBuilder();
            if (_users.Contains(user))
            {
                WufooData userData = _userList[_users.IndexOf(user)];
                string response = SubmitRequest(url, userData.ApiKey);
                XElement xml = XElement.Parse(response);
                IEnumerable<XElement> entriesXml = xml.Elements("Entry");
                foreach (var entry in entriesXml)
                {
                    entries.Append("<Entry>");

                    // append EntryId, DateCreated, CreatedBy, DateUpdated and UpdatedBy (putting them in the desired order)
                    entries.Append(entry.Element("EntryId"));
                    entries.Append(entry.Element("DateCreated"));
                    entries.Append(entry.Element("CreatedBy"));
                    entries.Append(entry.Element("DateUpdated"));
                    entries.Append(entry.Element("UpdatedBy"));

                    foreach (var child in entry.Elements())
                    {
                        // append form field data
                        if (!child.Name.LocalName.Equals("EntryId") && !child.Name.LocalName.Equals("DateCreated") && !child.Name.LocalName.Equals("CreatedBy") &&
                            !child.Name.LocalName.Equals("DateUpdated") && !child.Name.LocalName.Equals("UpdatedBy"))
                        {
                            entries.AppendFormat("<Data><FieldId>{0}</FieldId><Value>{1}</Value></Data>", child.Name.LocalName, child.Value);
                        }
                    }

                    entries.Append("</Entry>");
                }
            }
            return entries.ToString();
        }

        /// <summary>
        /// Returns the user data.
        /// </summary>
        /// <param name="name">Name of the user</param>
        /// <returns>User data object</returns>
        public WufooData GetUser(string name)
        {
            if (_users.Contains(name))
            {
                return _userList[_users.IndexOf(name)];
            }

            return null;
        }

        /// <summary>
        /// Returns the list of forms for a user.
        /// </summary>
        /// <param name="user">Name of the user.</param>
        /// <returns>List of forms for a user</returns>
        public List<WufooData> GetForms(string user)
        {
            if (_users.Contains(user))
            {
                WufooData userData = _userList[_users.IndexOf(user)];
                string response = SubmitRequest(userData.FormsUrl, userData.ApiKey);
                XElement xml = XElement.Parse(response);

                IEnumerable<XElement> formsXml = xml.Elements("Form");
                List<WufooData> forms = (from f in formsXml select new WufooData(f, user, UserName)).ToList();

                return forms;
            }

            return null;
        }

        /// <summary>
        /// Returns a list of users and caches the results so we can reuse username and api key.
        /// </summary>
        /// <returns>List of users</returns>
        public List<WufooData> GetUsers()
        {
            // always clear lists so we have current data (new users could have been created)
            _users = new List<string>();
            _userList = new List<WufooData>();

            string url = string.Format("{0}users.xml", ApiUrl);

            string response = SubmitRequest(url, UserName, ApiKey);
            XElement xml = XElement.Parse(response);

            IEnumerable<XElement> usersXml = xml.Elements("User");
            foreach (XElement userXml in usersXml)
            {
                WufooData data = new WufooData(userXml, UserName);
                _userList.Add(data);
                _users.Add(data.Title);
            }

            return _userList;
        }
        #endregion

        #region private static members
        /// <summary>
        /// Submits a request to a URL and reads the response to a string.
        /// </summary>
        /// <param name="requestUrl">The URL to submit.</param>
        /// <param name="apiKey">The API key to use for this request.</param>
        /// <returns>The content of the response.</returns>
        private static string SubmitRequest(string requestUrl, string apiKey)
        {
            Uri address = new Uri(requestUrl);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
            request.UserAgent = "SDL Tridion ECL provider";
            request.Credentials = new NetworkCredential(apiKey, "nopass");
            using (Stream stream = request.GetResponse().GetResponseStream())
            {
                if (stream != null)
                {
                    string response;
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        response = sr.ReadToEnd();
                    }
                    return response;
                }
            }
            return null;
        }

        /// <summary>
        /// Submits a request to a URL (passing in the username) and reads the response to a string.
        /// </summary>
        /// <param name="requestUrl">The URL to submit.</param>
        /// <param name="user">The name of the user.</param>
        /// <param name="apiKey">The API key to use for this request.</param>
        /// <returns>The content of the response.</returns>
        private static string SubmitRequest(string requestUrl, string user, string apiKey)
        {
            return SubmitRequest(string.Format(requestUrl, user), apiKey);
        }
        #endregion
    }
}
