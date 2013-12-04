Type.registerNamespace("Extensions.PublishFromPublishingQueue");

/**
* Implements the <c>PqUnPublish</c> command
*/
Extensions.PublishFromPublishingQueue.PqUnPublish = function PublishFromPublishingQueue$PqUnPublish() {
    Type.enableInterface(this, "Extensions.PublishFromPublishingQueue.PqUnPublish");
    this.addInterface("Tridion.Cme.Command", ["PqUnPublish", $const.AllowedActions.UnPublish]);

    this.properties.popup = null;
};


/**
* Returns a value indicating whether this command can applicable for the selected item(s)
* @param {Tridion.Core.Selection} selection The current selection
* @returns {Boolean} <c>true</c> if this command can be executed; otherwise false.
*/
Extensions.PublishFromPublishingQueue.PqUnPublish.prototype._isAvailable = function PqUnPublish$_isAvailable(selection) {
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
Extensions.PublishFromPublishingQueue.PqUnPublish.prototype._isEnabled = function PqUnPublish$_isEnabled(selection) {
    return this._isAvailable(selection);
};

//Rhodia Update
Extensions.PublishFromPublishingQueue.PqUnPublish.prototype._onMultiUnpublish = function PqUnPublish$_onMultiUnpublish(event) {
    var p = this.properties;
    var items = event.data.items;
    var instruction = event.data.instruction;

    var msg = $messages.registerProgress(
		$localization.getEditorResource("PublishPopupItemsUnPublishing", [items.length])
	);

    function PqUnPublish$_onMultiUnpublish$_onSendToQueue(total) {
        msg.finish();
        if (p.popup) {
            p.popup.dispose();
            p.popup = null;
        }
        $messages.registerNotification($localization.getEditorResource("PublishPopupItemsSentToPublishQueue").format(total));
    };

    function PqUnPublish$_onMultiUnpublish$_onSendToQueueFailed() {
        msg.finish();
        $messages.registerError($localization.getEditorResource("PublishPopupItemsSentToPublishQueueFailed"));
    };

    tridion.Web.UI.ContentManager.Publishing.UnpublishItems(
		items,
		instruction,
		PqUnPublish$_onMultiUnpublish$_onSendToQueue,
		PqUnPublish$_onMultiUnpublish$_onSendToQueueFailed
	);
};

Extensions.PublishFromPublishingQueue.PqUnPublish.prototype._execute = function PqUnPublish$_execute(selection) {
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
        var items = [];

        //Rhodia update
        for (var i = 0; i < selection.getCount() ; i++) {
            var id = selection.getItem(i);
            var item = $models.getItem(id);

            if (Type.isFunction(item.getPublishItemId)) {
                // get item from publish transaction
                id = item.getPublishItemId();
                items.push(id);
            }
        }

        //Rhodia update
        if (items.length > 0) {
            // build params
            var params = { command: "unpublish", items: items, userWorkflow: false };

            p.popup = $popup.create($cme.Popups.PUBLISH.URL, $cme.Popups.PUBLISH.FEATURES, params);
            $evt.addEventHandler(p.popup, "unload",
			    function PqUnPublish$_execute$_unload(event) {
			        if (p.popup) {
			            p.popup.dispose();
			            p.popup = null;
			        }
			    });
            $evt.addEventHandler(p.popup, "error",
			    function PqUnPublish$_execute$_error(event) {
			        $messages.registerError(event.data.error.Message, null, null, null, true);

			        if (p.popup) {
			            p.popup.dispose();
			            p.popup = null;
			        }
			    });
            $evt.addEventHandler(p.popup, "unpublish",
			    function PqUnPublish$_execute$_unpublished(event) {
			        //Rhodia update for Tridion 2011 SP1 HR1
			        var item = $models.getItem(event.data.item);
			        $messages.registerNotification($localization.getEditorResource("PublishPopupSentToPublishQueue", item ? item.getStaticTitle() || item.getTitle() || item.getId() : event.data.item));

			        if (p.popup) {
			            p.popup.dispose();
			            p.popup = null;
			        }
			    });

            //Rhodia update	
            $evt.addEventHandler(p.popup, "multiunpublish", this.getDelegate(this._onMultiUnpublish));

            p.popup.open();
        }
    }
};
