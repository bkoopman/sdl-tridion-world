using System;
using System.Xml.Linq;
using Example.EclProvider.Extensions;

namespace Example.EclProvider.Api
{
    /// <summary>
    /// Basic data for a Flickr item (photo or photoset).
    /// </summary>
    public class FlickrInfo
    {
        #region basic properties
        public string Id { get; private set; }
        public string Title { get; private set; }
        public DateTime? Created { get; internal set; }
        public DateTime? Modified { get; internal set; }
        public bool IsPhoto { get; private set; }
        #endregion

        #region photo specific properties
        public string Server { get; private set; }
        public string Secret { get; private set; }
        public string Farm { get; private set; }
        public string Description { get; set; }

        /// <summary>
        /// Direct URL to the Flickr photo
        /// </summary>
        public string Url { get; set; }
        //public string UrlThumb { get; set; }

        /// <summary>
        /// Flickr Photo page url
        /// </summary>
        public string EditUrl { get; private set; }

        /// <summary>
        /// The Flickr Set id this photo belongs to
        /// </summary>
        public string PhotoSetId { get; private set; }
        #endregion

        public FlickrInfo(XElement xml, string photoSetId = null, string photoPage = null)
        {
            // set basic properties (for photos and sets)
            Id = xml.AttributeValueOrDefault("id");
            Title = xml.Element("title").ElementValueNull();
            if (string.IsNullOrEmpty(Title))
            {
                // try getting from attribute value
                Title = xml.AttributeValueOrDefault("title");
            }
            Created = xml.AttributeValueOrDefault("date_create").DateTimeFromUnixTimeStamp();
            Modified = xml.AttributeValueOrDefault("date_update").DateTimeFromUnixTimeStamp();
            IsPhoto = false;

            // set photo specific properties
            if (xml.Name.LocalName.Equals("photo"))
            {
                IsPhoto = true;
                Secret = xml.AttributeValueOrDefault("secret");
                Server = xml.AttributeValueOrDefault("server");
                Farm = xml.AttributeValueOrDefault("farm");
                Description = xml.Element("description").ElementValueNull();
                PhotoSetId = photoSetId;

                if (string.IsNullOrEmpty(photoPage))
                {
                    EditUrl = xml.Element("urls").Element("url").Value;
                    Created = xml.Element("dates").AttributeValueOrDefault("taken").ExactDateTime();
                    Modified = xml.Element("dates").AttributeValueOrDefault("lastupdate").DateTimeFromUnixTimeStamp();
                }
                else
                {
                    EditUrl = string.Format(photoPage, xml.AttributeValueOrDefault("pathalias"), Id);
                    Created = xml.AttributeValueOrDefault("datetaken").ExactDateTime();
                    Modified = xml.AttributeValueOrDefault("lastupdate").DateTimeFromUnixTimeStamp();
                }
            }
        }
    }
}
