Type.registerNamespace("Social.Commands");

Social.Commands.DiggThis = function Commands$DiggThis(name) {
    Type.enableInterface(this, "Social.Commands.DiggThis");
    this.addInterface("Social.ShareBaseCommand", ["DiggThis"]);

    var p = this.properties;
};

Social.Commands.DiggThis.prototype.openPopup = function(pageUrl, pageTitle) {
    var DiggThisPopUpUrl = "http://digg.com/submit?url=" + pageUrl + "&title=" + pageTitle;
    var popup = $popup.create(DiggThisPopUpUrl, "toolbar=no,width=790,height=500,resizable=yes,scrollbars=yes", null);
    popup.open();
};