<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TridionTreeView.ascx.cs" Inherits="SDLTridion.Examples.ItemSelector.UserControls.TridionTreeView" %>
<asp:TreeView ID="EmbeddedTreeView" runat="server" NodeIndent="16" PopulateNodesFromClient="true" OnTreeNodePopulate="PopulateNode" ExpandDepth="FullyExpand" SkipLinkText="">
    <ParentNodeStyle Font-Bold="False" />
    <HoverNodeStyle Font-Underline="False" />
    <SelectedNodeStyle Font-Underline="False" HorizontalPadding="4px" VerticalPadding="0px" />
    <NodeStyle Font-Names="Verdana, Helvetica" Font-Size="8.5pt" ForeColor="#333333" HorizontalPadding="4px" NodeSpacing="1px" VerticalPadding="0px" />
    <Nodes>
        <asp:TreeNode Text="Content Management" PopulateOnDemand="true" Expanded="false" Value="tcm:0-0-0" />
    </Nodes>
</asp:TreeView>
