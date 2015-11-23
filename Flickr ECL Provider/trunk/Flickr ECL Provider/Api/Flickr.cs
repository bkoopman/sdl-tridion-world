using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Example.EclProvider.Extensions;

namespace Example.EclProvider.Api
{
    /// <summary>
    /// A light weight class to communicate with Flickr's API basic features.
    /// Based on the Flickr Library by Hector Correa.
    /// http://hectorcorrea.com/blog/Flickr-Library-in-C-Sharp
    /// </summary>
    /// <example>
    /// This sample shows how to use this Flickr libary:
    /// <code>
    /// // Instantiate the Flickr class
    /// Flickr flickr = new Flickr("YourApiKey", "YourUserId");
    /// 
    /// // Get a list of your photosets
    /// List&lt;PhotoSetInfo&gt; photosets = flickr.GetPhotoSets(); 
    /// 
    /// // Get a list of the photos inside a photoset
    /// List&lt;PhotoInfo&gt; photos = flickr.GetPhotosInSet(photosets[0].PhotoSetId);
    /// 
    /// // Get the complete URL for a particular photo's thumbnail 
    /// string photoUrl = Flickr.GetPhotoUrl(photos[0], PhotoSizeNum.Thumbnail);
    /// </code>
    /// </example>
    public class Flickr
    {
        private const string DefaultUrl = "https://api.flickr.com/services/rest/";
        private const string PhotoPage = "https://www.flickr.com/photos/{0}/{1}/";

        /// <summary>
        /// Publicly available maximum photo width in pixels.
        /// </summary>
        public const int MaxWidth = 1024;

        /// <summary>
        /// Publicly available maximum photo height in pixels.
        /// </summary>
        public const int MaxHeight = 768;

        #region properties
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public string UserId { get; set; }
        #endregion

        #region constructors
        /// <summary>
        /// Creates an instance of the Flickr class. 
        /// </summary>
        /// <param name="url">Flickr's API URL</param>
        /// <param name="apiKey">Your API key with Flickr. 
        /// See Flickr's API documentation at http://www.flickr.com/services/ if you don't know what this is.</param>
        /// <param name="userId">Your Flickr user ID.</param>
        public Flickr(string url, string apiKey, string userId)
        {
            ApiUrl = url;
            ApiKey = apiKey;
            UserId = userId;
        }

        /// <summary>
        /// Creates an instance of the Flickr class. 
        /// This method has the URL to Flick hardcoded to http://api.flickr.com/services/rest/
        /// </summary>
        /// <param name="apiKey">Your API key with Flickr. 
        /// See Flickr's API documentation at http://www.flickr.com/services/ if you don't know what this is.</param>
        /// <param name="userId">Your Flickr user ID.</param>
        public Flickr(string apiKey, string userId)
            : this(DefaultUrl, apiKey, userId)
        {
        }
        #endregion

        #region public members
        /// <summary>
        /// Returns the photo info.
        /// </summary>
        /// <param name="id">Id of the photo</param>
        /// <param name="secret">Secret of the photo</param>
        /// <param name="photoSetId">The ID of the photoset this photo belongs to</param>
        /// <returns></returns>
        public FlickrInfo GetPhotoInfo(string id, string secret, string photoSetId)
        {
            const string getInfoMethod = "flickr.photos.getInfo";
            string url = string.Format("{0}?method={1}&api_key={2}&photo_id={3}&secret={4}", ApiUrl, getInfoMethod, ApiKey, id, secret);
            XElement responseXml = SubmitRequest(url);

            XElement photoElement = responseXml.Element("photo");
            if (photoElement != null)
            {
                FlickrInfo photo = new FlickrInfo(photoElement, photoSetId);
                photo.Url = GetPhotoUrl(photo, PhotoSizeEnum.Large);
                return photo;
            }
            return null;
        }

        /// <summary>
        /// Returns the list of photos in a set.
        /// </summary>
        /// <param name="photoSetId">ID of the set. Typically a value returned by GetPhotoSets</param>
        /// <returns></returns>
        public List<FlickrInfo> GetPhotosInSet(string photoSetId)
        {
            const string getPhotosMethod = "flickr.photosets.getPhotos";
            string url = string.Format("{0}?method={1}&api_key={2}&photoset_id={3}&extras=path_alias,date_taken,last_update&media=photos", ApiUrl, getPhotosMethod, ApiKey, photoSetId);
            XElement responseXml = SubmitRequest(url);

            IEnumerable<XElement> photosetXml = responseXml.Elements("photoset");
            IEnumerable<XElement> photosXml = photosetXml.Elements("photo");

            List<FlickrInfo> photos = (from p in photosXml select new FlickrInfo(p, photoSetId, PhotoPage)).ToList();
            foreach (FlickrInfo photo in photos)
            {
                photo.Url = GetPhotoUrl(photo, PhotoSizeEnum.Large);
                //photo.UrlThumb = GetPhotoUrl(photo, PhotoSizeEnum.Thumbnail);
            }
            return photos;
        }

        /// <summary>
        /// Returns the photos set info.
        /// </summary>
        /// <param name="photoSetId">ID of the set.</param>
        /// <returns></returns>
        public FlickrInfo GetPhotoSetInfo(string photoSetId)
        {
            const string getInfoMethod = "flickr.photosets.getInfo";
            string url = string.Format("{0}?method={1}&api_key={2}&photoset_id={3}", ApiUrl, getInfoMethod, ApiKey, photoSetId);
            XElement xml = SubmitRequest(url);

            XElement xmlPhotoSet = xml.Element("photoset");
            if (xmlPhotoSet != null)
            {
                return new FlickrInfo(xmlPhotoSet);
            }

            return null;
        }

        /// <summary>
        /// Returns a list of photoset for the current user.
        /// </summary>
        public List<FlickrInfo> GetPhotoSets()
        {
            List<FlickrInfo> list = new List<FlickrInfo>();

            const string getListMethod = "flickr.photosets.getList";
            string url = string.Format("{0}?method={1}&api_key={2}&user_id={3}", ApiUrl, getListMethod, ApiKey, UserId);
            XElement xml = SubmitRequest(url);

            IEnumerable<XElement> photosetsRoot = xml.Elements("photosets");
            foreach (XElement xmlPhotoSet in photosetsRoot.Elements("photoset"))
            {
                list.Add(new FlickrInfo(xmlPhotoSet));
            }
            return list;
        }

        /// <summary>
        /// Fills the details about a picture. At this time the only value that
        /// it fills is the description. Typically you call GetPhotosInSet first
        /// and then this method to populate the description of an individual 
        /// picture. 
        /// </summary>
        /// <param name="photo">A FlickrInfo with information about the picture to fill.
        /// </param>
        public void FillPhotoDetails(ref FlickrInfo photo)
        {
            const string getInfoMethod = "flickr.photos.getInfo";
            string url = string.Format("{0}?method={1}&api_key={2}&photo_id={3}&secret={4}", ApiUrl, getInfoMethod, ApiKey, photo.Id, photo.Secret);
            XElement responseXml = SubmitRequest(url);

            XElement photoElement = responseXml.Element("photo");
            if (photoElement != null)
            {
                XElement descriptionElement = photoElement.Element("description");
                if (descriptionElement != null)
                {
                    photo.Description = descriptionElement.Value;
                }
            }
        }

        /// <summary>
        /// Return PhotoSet page url.
        /// </summary>
        /// <param name="id">ID of the photoset</param>
        /// <returns></returns>
        public string GetPhotoSetPageUrl(string id)
        {
            const string photoSetPage = "http://www.flickr.com/photos/{0}/sets/{1}/";
            if (string.IsNullOrEmpty(id))
            {
                // if we load the url without a photoset id, it goes to the set page
                return string.Format(photoSetPage, UserId, string.Empty);
            }
            return string.Format(photoSetPage, UserId, id);
        }

        /// <summary>
        /// Returns Photo page url.
        /// </summary>
        /// <param name="id">ID of the photo</param>
        /// <returns></returns>
        public string GetPhotoPageUrl(string id)
        {
            const string photoPage = "http://www.flickr.com/photos/{0}/{1}/";
            return string.Format(photoPage, UserId, id);
        }
        #endregion

        #region public static members
        /// <summary>
        /// Returns the URL for a particular picture. 
        /// Typically you call GetPhotosInSet first and then this method to get 
        /// the complete URL for a picture in the set in a particular size.
        /// </summary>
        /// <param name="photo">Details of the photo</param>
        /// <param name="size">The size requested for the picture</param>
        /// <returns>URL as a string</returns>
        public static string GetPhotoUrl(FlickrInfo photo, PhotoSizeEnum size)
        {
            string baseUrl = string.Format("http://farm{0}.static.flickr.com", photo.Farm);
            string url = string.Format("{0}/{1}/{2}_{3}{4}.jpg", baseUrl, photo.Server, photo.Id, photo.Secret, size.Description());

            return url;
        }

        /// <summary>
        /// Returns the URL for a particular picture with a given width.
        /// The returned URL will be based on the specified width, if available the
        /// exact width will be returned, else the nearest smaller variant will be used.
        /// </summary>
        /// <param name="photo">Details of the photo</param>
        /// <param name="width">The width in pixels for the picture, defaults to <see cref="MaxWidth"/></param>
        /// <returns>URL as a string</returns>
        public static string GetPhotoUrl(FlickrInfo photo, int width = MaxWidth)
        {
            if (width >= MaxWidth)
            {
                // width 1024, height 768
                return GetPhotoUrl(photo, PhotoSizeEnum.Large);
            }
            if (width >= 800)
            {
                // width 800, height 600
                return GetPhotoUrl(photo, PhotoSizeEnum.Svga);
            }
            if (width >= 640)
            {
                // width 640, height 480
                return GetPhotoUrl(photo, PhotoSizeEnum.Vga);
            }
            if (width >= 500)
            {
                // width 500, height 375
                return GetPhotoUrl(photo, PhotoSizeEnum.Medium);
            }
            if (width >= 320)
            {
                // width 320, height 240
                return GetPhotoUrl(photo, PhotoSizeEnum.Qvga);
            }
            if (width >= 240)
            {
                // width 240, height 180
                return GetPhotoUrl(photo, PhotoSizeEnum.Small);
            }
            if (width >= 150)
            {
                // width 150, height 150
                return GetPhotoUrl(photo, PhotoSizeEnum.LargeSquare);
            }
            if (width >= 100)
            {
                // width 100, height 75
                return GetPhotoUrl(photo, PhotoSizeEnum.Thumbnail);
            }
            // width 75, height 75
            return GetPhotoUrl(photo, PhotoSizeEnum.Square);
        }
        #endregion

        #region private static members
        /// <summary>
        /// Submits a request to a URL and reads the response to a string.
        /// </summary>
        /// <param name="requestUrl">The URL to submit.</param>
        /// <returns>The content of the response.</returns>
        private static XElement SubmitRequest(string requestUrl)
        {
            string textResponse;
            XElement xmlResponse;
            WebRequest request = WebRequest.Create(requestUrl);
            request.Proxy = null;
            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        textResponse = reader.ReadToEnd();
                    }
                }
            }
            xmlResponse = XElement.Parse(textResponse);

            if (xmlResponse.AttributeValueOrDefault("stat") != "ok")
            {
                if (xmlResponse.AttributeValueOrDefault("stat") == "fail")
                {
                    XElement error = xmlResponse.Element("err");
                    if (error != null)
                        throw new Exception(String.Format("Error while retrieving from Flickr; Error code {0}: {1}", error.AttributeValueOrDefault("code"), error.AttributeValueOrDefault("msg")));
                }
                throw new Exception("Error while retrieving from Flickr");
            }

            return xmlResponse;
        }
        #endregion
    }
}
