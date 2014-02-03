using System;
using System.Xml.Linq;
using EclProvider.Wufoo.Extensions;

namespace EclProvider.Wufoo.Api
{
    /// <summary>
    /// Basic data for a Wufoo item (form and possibly user).
    /// </summary>
    public class WufooData
    {
        #region properties
        public string Hash { get; private set; }
        public string Title { get; private set; }
        public string Name { get; private set; }
        public string User { get; private set; }
        public string ApiKey { get; private set; }
        public DateTime? Created { get; internal set; }
        public DateTime? Modified { get; internal set; }
        public bool IsForm { get; private set; }
        public string Description { get; private set; }
        public string Language { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public string IsPublic { get; private set; }
        public string FormsUrl { get; private set; }
        public bool CreateForms { get; private set; }
        public string RedirectMessage { get; private set; }
        public string Email { get; private set; }
        public string EntryLimit { get; private set; }
        public string LinkFields { get; private set; }
        public string LinkEntries { get; private set; }

        /// <summary>
        /// Direct URL to the Wufoo form
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Wufoo form edit url
        /// </summary>
        public string EditUrl { get; private set; }

        /// <summary>
        /// The Wufoo admin username
        /// </summary>
        public string AdminUserName { get; private set; }
        #endregion

        /// <summary>
        /// Constructor for user data 
        /// </summary>
        /// <param name="xml">XContainer containing user data</param>
        /// <param name="adminUser">Admin username</param>
        public WufooData(XContainer xml, string adminUser)
        {
            // set user properties
            AdminUserName = adminUser;
            Hash = xml.Element("Hash").ElementValueNull();
            Title = xml.Element("User").ElementValueNull();
            ApiKey = xml.Element("ApiKey").ElementValueNull();
            FormsUrl = xml.Element("LinkForms").ElementValueNull();
            CreateForms = xml.Element("CreateForms").ElementValueNull().Equals("1");
            Created = DateTime.MinValue;
            Modified = DateTime.MinValue;
            IsForm = false;
        }

        /// <summary>
        /// Constructor for form data
        /// </summary>
        /// <param name="xml">XContainer containing form data</param>
        /// <param name="user">User the form belongs to</param>
        /// <param name="adminUser">Admin username</param>
        public WufooData(XContainer xml, string user, string adminUser)
        {
            // set form properties
            AdminUserName = adminUser;
            User = user;
            IsForm = true;
            Hash = xml.Element("Hash").ElementValueNull();
            Title = xml.Element("Name").ElementValueNull();
            Name = xml.Element("Url").ElementValueNull();
            Created = xml.Element("DateCreated").ElementValueNull().UniversalDateTime();
            Modified = xml.Element("DateUpdated").ElementValueNull().UniversalDateTime();
            Description = xml.Element("Description").ElementValueNull();
            Language = xml.Element("Language").ElementValueNull();
            IsPublic = xml.Element("IsPublic").ElementValueNull();
            StartDate = xml.Element("StartDate").ElementValueNull().UniversalDateTime();
            EndDate = xml.Element("EndDate").ElementValueNull().UniversalDateTime();
            RedirectMessage = xml.Element("RedirectMessage").ElementValueNull();
            Email = xml.Element("Email").ElementValueNull();
            EntryLimit = xml.Element("EntryLimit").ElementValueNull();
            LinkFields = xml.Element("LinkFields").ElementValueNull();
            LinkEntries = xml.Element("LinkEntries").ElementValueNull();

            // we can use the friendly url or the hash (let's use the friendly url for now)
            Url = string.Format(WufooWrapper.FormUrl, adminUser, Name);
            EditUrl = string.Format(WufooWrapper.FormEditUrl, adminUser, Name);
        }
    }
}
