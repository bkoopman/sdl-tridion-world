/**
* Initialize and load Item Selector iframe.
*/
function init() {
    var args = window.dialogArguments;
    if (args) {
        /*
        // get current field value
        var value = "";
        var fields = args.getFields();
        if (fields && fields.length > 0) {
            var values = fields[0].getValues();
            if (values && values.length > 0) {
                value = values[0]
            }
        }
        */
        // load iframe with query string parameters and current publication uri 
        var pubid = args.controller.getItemPublicationUri();
        var querystring = "";
        if (window.location.search.length > 0) {
            querystring = "&" + window.location.search.substring(1);
        }
        document.getElementById("selectorframe").src = "ItemSelector.aspx?pubid=" + pubid + querystring;
    }
    else {
        document.getElementById("selectorframe").src = "ItemSelector.aspx" + window.location.search;
    }
}

/**
* Set selected value back in field and close popup window.
*/
function setvalue(id) {
    var args = window.dialogArguments;
    // set field value
    if (args) {
        var fields = args.getFields();
        if (fields && fields.length > 0) {
            fields[0].setValues([id]);
        }
    }
    else {
        // when there is no window.dialogArguments, just display value
        alert(id);
    }

    // close popup
    window.close();
}