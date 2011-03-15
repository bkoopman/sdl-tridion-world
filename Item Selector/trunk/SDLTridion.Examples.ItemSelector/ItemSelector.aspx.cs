using System;
using System.Web;
using Tridion.ContentManager;

namespace SDLTridion.Examples.ItemSelector
{
    public partial class ItemSelector : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // get the current logged in user
            string user = HttpContext.Current.User.Identity.Name;
            if (!String.IsNullOrEmpty(user))
            {
                // set LogonUser
                TridionTreeView.LogonUser = user;
            }

            // process querystring parameters
            string start = Request.QueryString["start"];
            string types = Request.QueryString["types"];
            string pubid = Request.QueryString["pubid"];

            if (!String.IsNullOrEmpty(start))
            {
                // override StartFromUri
                TridionTreeView.StartFromUri = start;
            }
            else if (!String.IsNullOrEmpty(pubid))
            {
                // override StartFromUri with current publication id
                TcmUri uri = new TcmUri(pubid);
                TridionTreeView.StartFromUri = uri.ToString();
            }

            if (!String.IsNullOrEmpty(types))
            {
                // override SelectTypes
                TridionTreeView.SelectTypes = Convert.ToInt32(types);
            }
        }
    }
}
