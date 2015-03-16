# Introduction #

This UI extension adds support for copying the content of an ECL item into a Multimedia Component. It can be used as a one time import tool for entire folders with ECL items (right click the ECL folder and select Import item...).


# Details #

The content is imported from the ECL item by either calling `GetContent` or reading the content from the location specified by the `GetDirectLinkToPublished` method.

Through the Core Service a new Multimedia Component is created and if this contains a multi value Metadata field named `data` (consisting of an embeddable schema with `key` and `value` fields), also the External Metadata of the ECL item is imported in the Multimedia Component.

The extension works on both the CME and XPM, for the latter it is implemented as a replace function for an existing Multimedia link to an ECL item (replacing the ECL link with the imported item).

In the `EclImportEditor.config` file the item id of the Multimedia Schema to use for imported content and item id of the target Folder for imports can be configured (keep in mind that for the XPM import to work, the Multimedia Link field has to allow both the ECL schema and the configured Multimedia Schema).