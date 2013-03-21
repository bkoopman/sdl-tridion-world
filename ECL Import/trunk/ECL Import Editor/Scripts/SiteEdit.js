var onDisplayStarted = function () {
    $evt.removeEventHandler($display, "start", onDisplayStarted);

    var view = $display.getView();

    if (view.getId && view.getId() == "EditorView") { // EditorView => SiteEdit dashboard
        //console.log("EclImport: inside EditorView");
        var parent = $('#CLinkPropertiesPage');
        var props = $controls.getControl(parent, 'Tridion.Web.UI.Editors.SiteEdit.Controls.CLinkPropertiesPage');
        var marker = $('#CLinkFieldDescriptionWrapper');
        var separator = $('#CLinkSeparator');

        //<label>Import Item: </label>
        var label = document.createElement('label');
        label.innerHTML = 'Import multimedia content: ';
        label.setAttribute('style', 'min-height: 22px; padding: 5px;');
        parent.insertBefore(label, marker);
        //console.log("EclImport: added label");

        //<Button id="ImportEclItem" runat="server">Import</Button>
        var button = document.createElement('Button');
        button.setAttribute('id', 'ImportEclItem');
        button.setAttribute('runat', 'server');
        button.textContent = "Import";
        parent.insertBefore(button, marker);
        //console.log("EclImport: added button");

        // read configuration for schemaid
        var editorsettings = $xml.toJson($xml.getNewXmlDocument($config.Editors["EclImport"].configuration));

        var library = view.getLibraryControl();
        var selectedMMCId; // will be set when the Import button is clicked
        function EclImport$onFolderSelected(e) {
            view.closeLibrary();

            var locationId = e && e.data && e.data.items && e.data.items[0];
            if (locationId) {
                var publicationItemId = locationId.split(":")[1].split("-")[0];
                var progress = $messages.registerProgress("Importing multimedia content...");
                progress.setOnSuccessMessage("Multimedia content imported");
                console.log("Importing " + selectedMMCId + " into folder " + locationId);
                EclImport.Model.Services.ImportService.ImportItem(
                    selectedMMCId,
                    locationId,
                    "tcm:" + publicationItemId + "-" + editorsettings.schemaid + "-8",
                    function (result) {
                        console.log(result);
                        progress.finish({ success: true });
                        // set the value from the result on the current field
                        var value = result.split(',')[0].split(' ')[1]; // TODO: adapt to final result format
                        var item = $models.getItem(view.properties.selectedComponentId);
                        var fieldXpath = view._getSelectedFieldData().xpath;
                        view.applyFieldValue(item, fieldXpath, value, this);
                    },
                    function (error) {
                        progress.finish();
                        $messages.registerError(error.Message, error.Source, error.StackTrace, true, false);
                    }
                );
            }
        };
        function EclImport$onLibraryClose() {
            $evt.removeEventHandler(library, "changeitem", EclImport$onFolderSelected);
            $evt.removeEventHandler(view, "libraryclose", EclImport$onLibraryClose);
        };
        $evt.addEventHandler(button, 'click', function () {
            selectedMMCId = view._getSelectedFieldData().value;
            if (false === Tridion.OO.implementsInterface($models.getItem(selectedMMCId), "Tridion.ExternalContentLibrary.File")) return;
            view.openLibrary($see.Controls.SEDrillDown.Mode.SELECT, { allowSelectOrgItems: true }, { showFolders: true }, true, function () {
                // this code executes after the library has opened
                $evt.addEventHandler(library, "changeitem", EclImport$onFolderSelected);
                $evt.addEventHandler(view, "libraryclose", EclImport$onLibraryClose);

                view.setLibraryLabel("Import multimedia content");
                view.setLibraryModeLabel("Select Target Folder");
                view.setLibraryActionBtnsLabel("Import");
                // TODO: find a way to open the library with the configured target folder selected
                //var publicationItemId = selectedMMCId.split(":")[1].split("-")[0];
                //library.fireEvent("navigatetoitem", { itemId: "tcm:" + publicationItemId + "-" + editorsettings.folderid + "-2" });
            });
        });
        // TODO: hide/disable the Import button on non-ECL items
        //selectedMMCId = view._getSelectedFieldData().value;
        //if (false === Tridion.OO.implementsInterface($models.getItem(selectedMMCId), "Tridion.ExternalContentLibrary.File")) return;
        $evt.addEventHandler(view, "propertiesboxshown", function () {
            if (view._getSelectedFieldData()) {
                var selectedMMCId = view._getSelectedFieldData().value;
                label.style.display = button.style.display = Tridion.OO.implementsInterface($models.getItem(selectedMMCId), "Tridion.ExternalContentLibrary.File") ? "" : "none";
                // these are html elements not tridion controls, so don't implement enable/disable
            }
        });

        //<div id="CLinkSeparator2" class="separator"></div>
        parent.insertBefore(separator.cloneNode(), marker).setAttribute('id', 'CLinkSeparator_' + Date.now());
    }
};
$evt.addEventHandler($display, "start", onDisplayStarted);
//console.log("EclImport: onDisplayStarted event handler added");
