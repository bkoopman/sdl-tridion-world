Type.registerNamespace("Example.UiCoreService.Editor.Commands");

Example.UiCoreService.Editor.Commands.Decommission = function () {
    Type.enableInterface(this, "Example.UiCoreService.Editor.Commands.Decommission");
    this.addInterface("Tridion.Cme.Command", ["Decommission", $const.AllowedActions.Delete]);

    var p = this.properties;
    p.processId = null;
    p.pollInterval = 500;  // milliseconds between each call to check the status of a process
};

Example.UiCoreService.Editor.Commands.Decommission.prototype._execute = function (selection) {
    var onSuccess = Function.getDelegate(this, this._onExecuteStarted);
    var onFailure = null;
    var context = null;

    var itemUri = selection && selection.getItem(0);
    if (itemUri) {
        $log.info("Decommisioning Publication Target: " + itemUri);
        Example.UiCoreService.Model.Services.DecommissionService.Execute(itemUri, onSuccess, onFailure, context, false);
    }
};

Example.UiCoreService.Editor.Commands.Decommission.prototype._isAvailable = function (selection) {
    var itemUri = selection && selection.getItem(0);
    if (itemUri) {
        // enable when selected item is a publication target
        var itemType = $models.getItemType(itemUri);
        return itemType == $const.ItemType.PUBLICATION_TARGET;
    }
    return false;
};

Example.UiCoreService.Editor.Commands.Decommission.prototype._isEnabled = function (selection) {
    return this.isAvailable(selection);
};

Example.UiCoreService.Editor.Commands.Decommission.prototype._handleStatusResponse = function (result) {
    var p = this.properties;

    p.processId = result.Id;
    //this._updateProgressBar(result);

    if (result.PercentComplete < 100) {
        $log.debug("Process #" + p.processId + ": " + result.PercentComplete + "%");
        this._pollStatus(p.processId);
    } else {
        $log.debug("Process #" + p.processId + ": " + result.Status);
        //$j('#ProgressStatus').html(result.Status);
        //$j('#CloseDialog').show();
        p.processId = "";
    }
};

Example.UiCoreService.Editor.Commands.Decommission.prototype._pollStatus = function (id) {
    var onSuccess = Function.getDelegate(this, this._handleStatusResponse);
    var onFailure = null;
    var context = null;

    var callback = function () {
        //$log.debug("Checking the status of process #" + id);
        Example.UiCoreService.Model.Services.DecommissionService.GetProcessStatus(id, onSuccess, onFailure, context, false);
    };

    setTimeout(callback, this.properties.pollInterval);
};

Example.UiCoreService.Editor.Commands.Decommission.prototype._onExecuteStarted = function (result) {
    if (result) {
        this._pollStatus(result.Id);
    }
};