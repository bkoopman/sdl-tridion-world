Type.registerNamespace("Social.Commands");

Social.Commands.PostToLinkedIn = function Commands$PostToLinkedIn(name) {
    Type.enableInterface(this, "Social.Commands.PostToLinkedIn");
    this.addInterface("Social.ShareBaseCommand", ["PostToLinkedIn"]);

    var p = this.properties;
};

Social.Commands.PostToLinkedIn.prototype.openPopup = function(pageUrl, pageTitle) {
    var PostToLinkedInPopUpUrl = "http://www.linkedin.com/shareArticle?mini=true&url=" + pageUrl + "&summary=&title=" + pageTitle;
    var popup = $popup.create(PostToLinkedInPopUpUrl, "toolbar=no,width=590,height=600,resizable=yes,scrollbars=yes", null);
    popup.open();
};