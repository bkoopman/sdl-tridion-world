Type.registerNamespace("Tridion.EclImport.Commands");

/**
* Implements the <c>CreateMediaDistribution</c> command.
*/
Tridion.EclImport.Commands.ImportItem = function EclImport$ImportItem() {
    Type.enableInterface(this, "Tridion.EclImport.Commands.ImportItem");
    this.addInterface("Tridion.ExternalContentLibrary.Command");
};

/**
* Returns a value indicating whether this command can applicable for the selected item(s)
* @param {Tridion.Core.Selection} selection The current selection
* @returns {Boolean} <c>true</c> if this command can be executed; otherwise false.
*/
Tridion.EclImport.Commands.ImportItem.prototype._isAvailable = function ImportItem$_isAvailable(selection, pipeline) {
    return Type.implementsInterface(selection && selection.getParentItem(), "Tridion.ExternalContentLibrary.Folder");
};

/**
* Returns a value indicating whether this command can be executed on selected item(s)
* @param {Tridion.Core.Selection} selection The current selection
* @returns {Boolean} <c>true</c> if this command can be executed; otherwise false.
*/
Tridion.EclImport.Commands.ImportItem.prototype._isEnabled = function ImportItem$_isEnabled(selection, pipeline) {
    var selectionParentItem = selection && selection.getParentItem();
    return Type.implementsInterface(selectionParentItem, "Tridion.ExternalContentLibrary.Folder");
};

/**
* Executes this command on the selection.
* @param {Tridion.Core.Selection} selection The current selection.
*/
Tridion.EclImport.Commands.ImportItem.prototype._execute = function ImportItem$_execute(selection, pipeline) {
    var p = this.properties;
    if (p.popup) {
        p.popup.close();
    }

    // "ecl:2-flickr-6008082388_a3277629da_72157627229244403-img-file"
    p.selectedPublicationItemId = selection.getParentItem().getId().split(':')[1].split('-')[0];

    p.popup = $popup.create(
		$cme.Popups.TREE_ITEM_SELECT.URL,
		$cme.Popups.TREE_ITEM_SELECT.FEATURES,
		{
		    rootId: "tcm:0-" + p.selectedPublicationItemId + "-1",
		    itemTypes: [
				$const.ItemType.PUBLICATION,
				$const.ItemType.FOLDER
		    ],
		    //managedActions: $const.ItemType.Folder, doesn't work due to a weird bitwise and
		    selectedId: this._getTargetFolder(p.selectedPublicationItemId),
		    allowRootSelection: false
		}
	);

    $evt.addEventHandler(p.popup, "unload", this.getDelegate(this._onPopupUnload));
    $evt.addEventHandler(p.popup, "select", this.getDelegate(this._onFolderSelect));
    p.popup.open();

    p.selectedMMCId = selection.getItem(0);

};

Tridion.EclImport.Commands.ImportItem.prototype._onPopupUnload = function ImportItem$_onPopupUnload(e) {
    var p = this.properties;
    if (p.popup) {
        $evt.removeAllEventHandlers(p.popup);
        p.popup.dispose();
        p.popup = null;
    }
};


Tridion.EclImport.Commands.ImportItem.prototype._onFolderSelect = function ImportItem$_onFolderSelect(e) {
    var p = this.properties;
    if (p.popup) {
        $evt.removeEventHandler(p.popup, "select", this.getDelegate(this._onFolderSelect));
        p.popup = null;
    }

    var locationId = (e && e.data) ? e.data.itemId : null;
    if (locationId) {
        var progress = $messages.registerProgress("Importing multimedia content...");
        progress.setOnSuccessMessage("Multimedia content imported");
        console.log("Importing " + p.selectedMMCId + " into folder " + locationId);
        EclImport.Model.Services.ImportService.ImportItem(
            p.selectedMMCId,
            locationId,
            this._getSchema(p.selectedPublicationItemId),
            function (result) {
                //console.log(result);
                progress.finish({ success: true });
            },
            function (error) {
                progress.finish();
                $messages.registerError(error.Message, error.Source, error.StackTrace, true, false);
            }
        );

    }
};

Tridion.EclImport.Commands.ImportItem.prototype._getSettings = function ImportItem$_getSettings() {
    var p = this.properties;
    if (p.editorsettings === undefined) {
        var editor = $config.Editors["EclImport"];
        if (editor && editor.configuration && !String.isNullOrEmpty(editor.configuration)) {
            var configSectionXmlDoc = $xml.getNewXmlDocument(editor.configuration);
            p.editorsettings = $xml.toJson(configSectionXmlDoc.documentElement);
        }
    }
    return p.editorsettings;
};


Tridion.EclImport.Commands.ImportItem.prototype._getSchema = function ImportItem$_getSchema(pubid) {
    var settings = this._getSettings();
    if (settings) {
        return "tcm:" + pubid + "-" + settings.schemaid + "-8";
    }
};

Tridion.EclImport.Commands.ImportItem.prototype._getTargetFolder = function ImportItem$_getTargetFolder(pubid) {
    var settings = this._getSettings();
    if (settings) {
        return "tcm:" + pubid + "-" + settings.folderid + "-2";
    }
};