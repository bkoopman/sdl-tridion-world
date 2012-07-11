Type.registerNamespace("Social.Commands");

Social.Commands.GoogleLinks = function Commands$GoogleLinks(name) {
    Type.enableInterface(this, "Social.Commands.GoogleLinks");
    this.addInterface("Social.ShareBaseCommand", ["GoogleLinks"]);

    var p = this.properties;
};

Social.Commands.GoogleLinks.prototype._execute = function (selection, pipeline) {
    if (selection.getCount() == 1) {
        var p = this.properties;
        var url = p.PageData.Url;

        this.openPopup(url, p.PageData.Title);
    }
};

Social.Commands.GoogleLinks.prototype.openPopup = function(pageUrl, pageTitle) {
    var googleLinksPopUpUrl = "http://www.google.com/#sclient=psy&hl=en&site=&source=hp&q=link%3A"+ pageUrl + "&fp=ab5cdb1806fef4aa";
    var popup = $popup.create(googleLinksPopUpUrl, "toolbar=no,width=790,height=500,resizable=yes,scrollbars=yes", null);
    popup.open();
};