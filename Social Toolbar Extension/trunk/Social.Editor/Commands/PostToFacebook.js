Type.registerNamespace("Social.Commands");

Social.Commands.PostToFacebook = function Commands$PostToFacebook(name) {
    Type.enableInterface(this, "Social.Commands.PostToFacebook");
    this.addInterface("Social.ShareBaseCommand", ["PostToFacebook"]);

    var p = this.properties;
};

Social.Commands.PostToFacebook.prototype.openPopup = function(pageUrl, pageTitle) {
    var FacebookPopUpUrl = "http://www.facebook.com/sharer.php?u=" + pageUrl + '&amp;t=' + pageTitle;
    var popup = $popup.create(FacebookPopUpUrl, "toolbar=no,width=626,height=436,resizable=yes,scrollbars=yes", null);
    popup.open();
};