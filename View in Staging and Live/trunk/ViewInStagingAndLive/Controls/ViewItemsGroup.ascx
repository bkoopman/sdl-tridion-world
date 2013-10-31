<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ViewItemsGroup.ascx.cs" Inherits="ViewInStagingAndLive.UI.Editor.Controls.ViewItemsGroup" %>
<%@ Import Namespace="Tridion.Web.UI" %>
<c:RibbonItemsGroup runat="server" ID="ViewItemsGroup">
    <c:RibbonButton runat="server" CommandName="ViewStaging" Title="<%$ Resources: ViewInStagingAndLive.UI.Editor.Strings, ViewStaging %>" Label="<%$ Resources:ViewInStagingAndLive.UI.Editor.Strings, ViewStaging %>" IsSmallButton="true" ID="ViewStagingBtn" />
    <c:RibbonButton runat="server" CommandName="ViewLive" Title="<%$ Resources: ViewInStagingAndLive.UI.Editor.Strings, ViewLive %>" Label="<%$ Resources:ViewInStagingAndLive.UI.Editor.Strings, ViewLive %>" IsSmallButton="true" ID="ViewLiveBtn" />
</c:RibbonItemsGroup>