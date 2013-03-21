/* Copy of 2011 /WebUI/Core/Controls/Popup/PopupInit.js */
/**
* Script used to initialize a simple popup dialog.
* It makes sure window.dialogArguments is properly initialized and the opener of the popup is ready.
* Have it as the first javascript file linked from within the popup
*/
try {
    var op = window.opener;
    if (op && op.Tridion && op.Tridion.Controls && op.Tridion.Controls.Popup) {
        op.Tridion.Controls.Popup.registerView(null, window);
    }
}
catch (e) { }