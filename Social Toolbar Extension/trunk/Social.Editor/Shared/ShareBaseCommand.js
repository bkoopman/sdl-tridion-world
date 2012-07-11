Type.registerNamespace("Social");

Social.ShareBaseCommand = function (id) {
    Tridion.OO.enableInterface(this, "Social.ShareBaseCommand");
    this.addInterface("Tridion.Cme.Command", [id]);

    var p = this.properties;
    p.id = id;
    p.initialized = false;
}

Social.ShareBaseCommand.prototype.initialize = function () {
    var p = this.properties;
    p.initialized = true;
}

Social.ShareBaseCommand.prototype.isAvailable = function(selection, pipeline) {
    var p = this.properties;

    if (selection.getCount() == 1) {
        var itemType = $models.getItemType(selection.getItem(0));
        if (itemType == $const.ItemType.PAGE) {
            var uriSelection = selection.getItem(0);

            if (p.PageData == null || p.PageData.Uri.equals != uriSelection) {
                Social.Model.Services.GetPageData.Execute(uriSelection, function (response) { p.PageData = response; }, null, null, true);
            }

            return p.PageData.IsPublished;
        }
    }
    return false;
};

Social.ShareBaseCommand.prototype.isEnabled = function(selection, pipeline) {
    var p = this.properties;

    if (p.PageData != null) {
        return p.PageData.IsPublished;
    }
    return false;
};

Social.ShareBaseCommand.prototype._execute = function(selection, pipeline) {
    if (selection.getCount() == 1) {
        var p = this.properties;
        var url = p.PageData.Url;

        if (p.PageData.UseShortUrl) {
            url = p.PageData.ShortUrl;
        }

        this.openPopup(url, p.PageData.Title);
    }
};