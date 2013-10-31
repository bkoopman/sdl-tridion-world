Type.registerNamespace("ViewInStagingAndLive.UI.Editor.Commands");

/**
* Implements the <c>ViewLive</c> command
*/
ViewInStagingAndLive.UI.Editor.Commands.ViewLive = function Commands$ViewLive() {
    Type.enableInterface(this, "ViewInStagingAndLive.UI.Editor.Commands.ViewLive");
    this.addInterface("Tridion.Cme.Command", ["ViewLive", $const.AllowedActions.Publish]);

    var p = this.properties;
    p.id;
    p.pubId;
    p.siteUrl;
    p.tab = null;
};

/**
* Returns a value indicating whether this command is applicable for the selected item(s)
* @param {Tridion.Core.Selection} selection The current selection
* @returns {Boolean} <c>true</c> if this command is applicable; otherwise false.
*/
ViewInStagingAndLive.UI.Editor.Commands.ViewLive.prototype._isAvailable = function ViewLive$_isAvailable(selection) {
    var itemId = selection.getItem(0);
    if (itemId != null) {
        // enable when selected item is a page
        var itemType = $models.getItemType(itemId);
        return itemType == $const.ItemType.PAGE;
    }
    return false;
};

/**
* Returns a value indicating whether this command can be executed on selected item(s)
* @param {Tridion.Core.Selection} selection The current selection
* @returns {Boolean} <c>true</c> if this command can be executed; otherwise false.
*/
ViewInStagingAndLive.UI.Editor.Commands.ViewLive.prototype._isEnabled = function ViewLive$_isEnabled(selection) {
    return this._isAvailable(selection);
};

/**
* Executes this command on the selection.
* Uses context publication when called in XPM.
* @param {Tridion.Cme.Selection} selection The current selection.
*/
ViewInStagingAndLive.UI.Editor.Commands.ViewLive.prototype._execute = function ViewLive$_execute(selection) {
    var p = this.properties;

    p.id = selection && selection.getItem(0);
    if (p.id) {
        var editor = $display.getView();
        if (editor) {
            if (typeof editor.getPublicationId === "function") {
                // get context publication in XPM
                p.pubId = editor.getPublicationId();

                // get item in context publication in XPM
                p.id = $tcmutils.getItemIdInSpecifiedPublicationContext(p.id, p.pubId);
            } else {
                p.pubId = ViewInStagingAndLive.UI.Editor.getPublicationUri(p.id);
            }

            var pub = $models.getItem(p.pubId);
            if (pub) {
                var self = this;
                var onPublicationLoaded = function ViewStaging$_execute$_onPublicationLoaded() {
                    $evt.removeEventHandler(pub, "load", onPublicationLoaded);

                    // read publication metadata
                    var xmlDocument = pub.getXmlDocument() || pub.getStaticXmlDocument();
                    var siteUrlMetadataFieldName = ViewInStagingAndLive.UI.Editor.getLiveSiteUrlMetadataFieldName();
                    p.siteUrl = $xml.getInnerText(xmlDocument, ViewInStagingAndLive.UI.Editor.XPATH.format(siteUrlMetadataFieldName)) || "";
                    if (p.siteUrl.slice(-1) == "/") {
                        // strip forward slash from end of url (page path already starts with a forward slash)
                        p.siteUrl = p.siteUrl.slice(0, -1);
                    }

                    if (p.siteUrl == "") {
                        // close tab and register warning
                        p.tab.close();
                        $messages.registerWarning($localization.getResource("ViewInStagingAndLive.UI.Editor.Strings", "MissingPublicationMetadata").format(siteUrlMetadataFieldName), null, null, true);
                    } else {
                        // load page for its path
                        var page = $models.getItem(p.id);
                        if (page) {
                            var onPageLoaded = function ViewStaging$_execute$_onPublicationLoaded$_onPageLoaded() {
                                $evt.removeEventHandler(page, "load", onPageLoaded);
                                var xmlDocument = page.getXmlDocument() || page.getStaticXmlDocument();
                                var pagePath = $xml.getInnerText(xmlDocument, ViewInStagingAndLive.UI.Editor.PUBLISH_LOCATION_URL) || "";

                                // open url in tab and set focus
                                p.tab.location.href = p.siteUrl + pagePath;
                                p.tab.focus();
                            };
                            var onPageLoadFailed = function ViewStaging$_execute$_onPublicationLoaded$_onPageLoadFailed(event) {
                                $evt.removeEventHandler(page, "loadfailed", onPageLoadFailed);
                                $messages.registerError(event.data.error.Message, null, null, null, true);
                            };

                            // register page load event handlers
                            $evt.addEventHandler(page, "load", onPageLoaded);
                            $evt.addEventHandler(page, "loadfailed", onPageLoadFailed);
                            page.load(page.isLoaded(true), $const.OpenMode.VIEW);
                        }
                    }
                };
                var onPublicationLoadFailed = function ViewStaging$_execute$_onPublicationLoadFailed(event) {
                    $evt.removeEventHandler(pub, "loadfailed", onPublicationLoadFailed);
                    $messages.registerError(event.data.error.Message, null, null, null, true);
                };

                // open new tab and keep focus (url is set after publication and page are loaded)
                p.tab = window.open("", "_newtab.live");
                window.focus();
            }

            // register publication load event handlers
            $evt.addEventHandler(pub, "load", onPublicationLoaded);
            $evt.addEventHandler(pub, "loadfailed", onPublicationLoadFailed);
            pub.load(pub.isLoaded(true), $const.OpenMode.VIEW);
        }
    }
};
