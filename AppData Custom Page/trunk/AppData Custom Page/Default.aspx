<%@ Page Language="C#" AutoEventWireup="true" validateRequest="false" CodeBehind="Default.aspx.cs" Inherits="Example.CustomPage.Default" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
    <title>Semantic Mapping</title>
    <link rel="stylesheet" type="text/css" href="Theme/Styles.css">
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" EnablePartialRendering="true" runat="server" />
        <div id="LocationBar" class="control flisttopbar">
            <div class="location">
                <div id="Location" class="label" style="display: block;">Semantic Mapping</div>
                <div id="ReloadBtn" class="tridion button refresh" tabindex="1" onclick="location.reload();">
                    <span class="text">&nbsp;</span>
                </div>
            </div>
        </div>
        <div id="Content" class="stack-calc" style="height: 216px;">
            <table class="nolines">
                <tr>
                    <td><strong>Vocabularies</strong></td>
                    <td><strong>Schema mapping</strong></td>
                </tr>
                <tr>
                    <td>
                        <asp:TextBox runat="server" ID="AppData" TextMode="MultiLine" Rows="6" Width="450"></asp:TextBox>
                    </td>
                    <td>
                        <asp:DropDownList runat="server" ID="SchemaList" OnSelectedIndexChanged="SchemaListChanged" AutoPostBack="True"></asp:DropDownList>
                        <asp:TextBox runat="server" ID="TypeOf" Width="300"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Button runat="server" ID="UpdateVocabs" Text="Update" CssClass="button" OnClick="UpdateVocabsClick" />
                        <asp:Label runat="server" ID="VocabsLabel"></asp:Label>
                    </td>
                    <td>
                        <asp:Button runat="server" ID="UpdateSchema" Text="Update" CssClass="button" OnClick="UpdateSchemaClick" />
                        <asp:Label runat="server" ID="SchemaLabel"></asp:Label>
                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
