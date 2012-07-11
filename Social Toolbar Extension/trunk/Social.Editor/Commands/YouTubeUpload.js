Type.registerNamespace("Social.Commands");

Social.Commands.YouTubeUpload = function Commands$YouTubeUpload() {
    Type.enableInterface(this, "Social.Commands.YouTubeUpload");
    this.addInterface("Tridion.Cme.Command", [name]);
};

Social.Commands.YouTubeUpload.prototype.isAvailable = function YouTubeUpload$isAvailable(selection) {
    if (selection.getCount() == 1) {
        var itemType = $models.getItemType(selection.getItem(0));
        if (itemType == $const.ItemType.COMPONENT) {
            return true;
        }
    }
    return false;
};

Social.Commands.YouTubeUpload.prototype.isEnabled = function YouTubeUpload$isEnabled(selection) {
    if (selection.getCount() == 1) {
        var selectedItem = selection.getItem(0);
        var itemType = $models.getItemType(selectedItem);

        if (itemType == $const.ItemType.COMPONENT) {
            var component = $models.getItem(selectedItem);

            if (!component.isStaticLoaded()) {
                component.staticLoad();

                var endTime = new Date();
                endTime.setTime(endTime.getTime() + 3000);
                while (new Date().getTime() < endTime.getTime());
            }

            if (component.isStaticLoaded()) {
                if (component.isMultimedia()) {
                    var componentXml = component.getStaticXmlDocument(false);
                    var xpathStringToMimeType = "/tcm:Component/tcm:Data/tcm:MultimediaType/@xlink:title";
                    var multimediaType = $xml.getInnerText(componentXml, xpathStringToMimeType);

                    var xpathStringToMimeTypeToMMTypeConfig = "/configuration/appSettings/add[@key='MultimediaType' and @value='" + multimediaType + "']"
                    var mmTypeConfigNode = $xml.selectSingleNode(this.properties.config, xpathStringToMimeTypeToMMTypeConfig);

                    if (mmTypeConfigNode != null) {
                        return true;
                    }
                }
            } else {
                //alert("Error occured loading Component");
            }
        }
    }
    return false;
};

Social.Commands.YouTubeUpload.prototype._execute = function YouTubeUpload$_execute(selection) {
    alert("You Tube Upload");
}