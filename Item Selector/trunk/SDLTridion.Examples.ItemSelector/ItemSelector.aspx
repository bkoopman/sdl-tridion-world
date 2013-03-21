<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ItemSelector.aspx.cs" Inherits="SDLTridion.Examples.ItemSelector.ItemSelector" %>
<%@ Register Src="~/UserControls/TridionTreeView.ascx" TagPrefix="sdl" TagName="TridionTreeView" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Item Selector</title>
    <script type="text/javascript">
        function setvalue(id) {
            if (parent.frames.length > 0) {
                // call function from parent (iframe)
                parent.setvalue(id);
            }
            else {
                // when there is no parent, just display value
                alert(id);
            }
        }
    </script>
</head>
<body>
    <form id="tree" runat="server">
    <sdl:TridionTreeView ID="TridionTreeView" runat="server" StartFromUri="tcm:0-0-0" SelectTypes="0" />
    <!-- StartFromUri can be set on querystring using "start", SelectTypes can bet set using "types" -->
    <!-- for example: http://website.url/ItemSelector.aspx?start=tcm:0-2-1&types=1112 -->
    <!-- if "pubid" is set on the querystring, StartFromUri will be set with this publication id -->
    <!-- configure SDL Tridion URL and CoreService URL in Web.config -->
    </form>
</body>
</html>