Type.registerNamespace("Social.Commands");

Social.Commands.TweetThis = function Commands$TweetThis(name) {
    Type.enableInterface(this, "Social.Commands.TweetThis");
    this.addInterface("Social.ShareBaseCommand", ["TweetThis"]);

    var p = this.properties;
};

Social.Commands.TweetThis.prototype.openPopup = function(pageUrl, pageTitle) {
    var TweetThisPopUpUrl = 'http://twitter.com/home?status=Currently reading "' + pageTitle + '" - ' + pageUrl;
    var popup = $popup.create(TweetThisPopUpUrl, "toolbar=no,width=790,height=500,resizable=yes,scrollbars=yes", null);
    popup.open();
};