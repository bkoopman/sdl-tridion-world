using Tridion.Web.UI.Core;
using Tridion.Web.UI.Controls;
using Tridion.Web.UI.Core.Controls;

namespace ViewInStagingAndLive.UI.Editor.Controls
{
    //[ControlResources("ViewInStagingAndLive.UI.Editor.ViewItemsGroup")]
    public class ViewItemsGroup : TridionUserControl
    {
        public string CmeThemePath
        {
            get
            {
                return ConfigurationManager.Editors["CME"].ThemeUrl;
            }
        }
    }
}