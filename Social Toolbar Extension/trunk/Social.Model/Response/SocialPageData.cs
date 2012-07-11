using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Social.Model.Response
{
    public class SocialPageData
    {
        private string _title = string.Empty;
        private string _uri = string.Empty;
        private string _url = string.Empty;
        private string _shortUrl = string.Empty;
        private bool _isPublished = false;
        private bool _useShortUrl = false;
        private bool _hasError = false;
        private Exception _errorInfo = null;

        public SocialPageData() { }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public string ShortUrl
        {
            get { return _shortUrl; }
            set { _shortUrl = value; }
        }

        public bool IsPublished
        {
            get { return _isPublished; }
            set { _isPublished = value; }
        }

        public bool UseShortUrl
        {
            get { return _useShortUrl; }
            set { _useShortUrl = value; }
        }

        public bool HasError
        {
            get { return _hasError; }
            set { _hasError = value; }
        }

        public Exception ErrorInfo
        {
            get { return _errorInfo; }
            set { _errorInfo = value; }
        }
    }
}