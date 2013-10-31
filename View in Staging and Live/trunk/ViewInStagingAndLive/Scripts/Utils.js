Type.registerNamespace("ViewInStagingAndLive.UI.Editor");

/**
* Gets the configSection.
* @returns {Object} The config Section configuration object.
* @private 
*/
ViewInStagingAndLive.UI.Editor.getEditorConfigSection = function Editor$getEditorConfigSection() {
    if (this._settings === undefined) {
        var editor = $config.Editors[ViewInStagingAndLive.UI.Editor.NAME];
        if (editor && editor.configuration && !String.isNullOrEmpty(editor.configuration)) {
            var configSectionXmlDoc = $xml.getNewXmlDocument(editor.configuration);
            this._settings = $xml.toJson(configSectionXmlDoc.documentElement);
        }
    }
    return this._settings;
};

/**
* Gets staging site URL Metadata field name defined in configuration.
* @return {String} The value.
*/
ViewInStagingAndLive.UI.Editor.getStagingSiteUrlMetadataFieldName = function Editor$getStagingSiteUrlMetadataFieldName() {
    var configSection = ViewInStagingAndLive.UI.Editor.getEditorConfigSection();
    if (configSection) {
        return configSection.stagingSiteUrlMetadataFieldName;
    }
    return null;
};

/**
* Gets live site URL Metadata field name defined in configuration.
* @return {String} The value.
*/
ViewInStagingAndLive.UI.Editor.getLiveSiteUrlMetadataFieldName = function Editor$getLiveSiteUrlMetadataFieldName() {
    var configSection = ViewInStagingAndLive.UI.Editor.getEditorConfigSection();
    if (configSection) {
        return configSection.liveSiteUrlMetadataFieldName;
    }
    return null;
};

/**
* Gets Publication URI from given URI.
* @return {String} uri TCM URI.
*/
ViewInStagingAndLive.UI.Editor.getPublicationUri = function Editor$getPublicationUri(uri) {
    // split the uri (tcm:0-1-2)
    var parts = uri.substr(4).split("-");
    var itemType = (parts.length == 2) ? 16 : parseInt(parts[2]);
    if (itemType == 1) {
        return uri;
    }
    var publicationId = parseInt(parts[0]);
    return String.format("tcm:0-{0}-1", publicationId);
};
