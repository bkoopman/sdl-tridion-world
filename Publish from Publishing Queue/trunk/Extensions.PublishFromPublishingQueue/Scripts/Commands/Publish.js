Type.registerNamespace("Extensions.PublishFromPublishingQueue");

/**
* Implements the <c>PqPublish</c> command
*/
Extensions.PublishFromPublishingQueue.PqPublish = function PublishFromPublishingQueue$PqPublish() {
    Type.enableInterface(this, "Extensions.PublishFromPublishingQueue.PqPublish");
    this.addInterface("Tridion.Cme.Command", ["PqPublish", $const.AllowedActions.Publish]);

    this.properties.popup = null;
};


/**
* Returns a value indicating whether this command is applicable for the selected item(s)
* @param {Tridion.Core.Selection} selection The current selection
* @returns {Boolean} <c>true</c> if this command is applicable; otherwise false.
*/
Extensions.PublishFromPublishingQueue.PqPublish.prototype._isAvailable = function PqPublish$_isAvailable(selection) {
    //Rhodia update
    if (selection.getCount() >= 1) {
        for (var i = 0; i < selection.getCount() ; i++) {
            var itemType = $models.getItemType(selection.getItem(i));
            if (itemType == $const.ItemType.PUBLISH_TRANSACTION) {
                return true;
            }
        }
    }
    return false;
};

/**
* Returns a value indicating whether this command can be executed on selected item(s)
* @param {Tridion.Core.Selection} selection The current selection
* @returns {Boolean} <c>true</c> if this command can be executed; otherwise false.
*/
Extensions.PublishFromPublishingQueue.PqPublish.prototype._isEnabled = function PqPublish$_isEnabled(selection) {
    return this._isAvailable(selection);
};

Extensions.PublishFromPublishingQueue.PqPublish.prototype._execute = function PqPublish$_execute(selection) {
    var p = this.properties;

    // there must be at least one item selected
    if (!selection || selection.getCount() == 0) {
        return;
    }

    // popup management
    if (p.popup) {
        p.popup.focus();
    }
    else {
        //Rhodia update
        var items = [];
        for (var i = 0; i < selection.getCount() ; i++) {
            var id = selection.getItem(i);
            var item = $models.getItem(id);

            if (Type.isFunction(item.getPublishItemId)) {
                // get item from publish transaction
                id = item.getPublishItemId()
                items.push(id);
            }
        }
        this._openPublishPopup(items);
    }
};

//Rhodia update
Extensions.PublishFromPublishingQueue.PqPublish.prototype._onMultiPublish = function PqPublish$_onMultiPublish(event) {
    var p = this.properties;
    var items = event.data.items;
    var instruction = event.data.instruction;

    var msg = $messages.registerProgress(
		$localization.getEditorResource("PublishPopupItemsPublishing", [items.length])
	);

    function PqPublish$_onMultiPublish$_onSendToQueue(total) {
        msg.finish();
        if (p.popup) {
            p.popup.dispose();
            p.popup = null;
        }
        $messages.registerNotification($localization.getEditorResource("PublishPopupItemsSentToPublishQueue").format(total));
    };

    function PqPublish$_onMultiPublish$_onSendToQueueFailed(error) {
        msg.finish();
        $messages.registerError(error.Message, null, null, true, false);
    };

    tridion.Web.UI.ContentManager.Publishing.PublishItems(
		items,
		instruction,
		PqPublish$_onMultiPublish$_onSendToQueue,
		PqPublish$_onMultiPublish$_onSendToQueueFailed
	);
};

Extensions.PublishFromPublishingQueue.PqPublish.prototype._openPublishPopup = function PqPublish$_openPublishPopup(items) {
    var p = this.properties;

    // build params
    var doRepublish = false;

    for (var i = 0, cnt = items.length; i < cnt; i++) {
        var type = $models.getItemType(items[i]);
        if (type == $const.ItemType.PUBLICATION || type == $const.ItemType.STRUCTURE_GROUP) {
            doRepublish = true;
            break;
        }
    }

    var params = { command: "publish", items: items, republish: doRepublish, userWorkflow: false };

    p.popup = $popup.create($cme.Popups.PUBLISH.URL, $cme.Popups.PUBLISH.FEATURES, params);

    $evt.addEventHandler(p.popup, "unload",
		function PqPublish$_execute$_unload(event) {
		    if (p.popup) {
		        p.popup.dispose();
		        p.popup = null;
		    }
		});
    $evt.addEventHandler(p.popup, "error",
		function PqPublish$_execute$_error(event) {
		    $messages.registerError(event.data.error.Message, null, null, null, true);

		    if (p.popup) {
		        p.popup.dispose();
		        p.popup = null;
		    }
		});
    $evt.addEventHandler(p.popup, "publish",
		function PqPublish$_execute$_published(event) {
		    //Rhodia update for Tridion 2011 SP1 HR1
		    var item = $models.getItem(event.data.item);
		    $messages.registerNotification($localization.getEditorResource("PublishPopupSentToPublishQueue", item ? item.getStaticTitle() || item.getTitle() || item.getId() : event.data.item));

		    if (p.popup) {
		        p.popup.dispose();
		        p.popup = null;
		    }
		});

    //Rhodia update	
    $evt.addEventHandler(p.popup, "multipublish", this.getDelegate(this._onMultiPublish));

    p.popup.open();
};
