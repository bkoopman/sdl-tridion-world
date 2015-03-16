# Introduction #

ECL in 2013 comes with a default set of Template Building Blocks which allow you to resolve ECL URIs in your templates to the actual item (similar to how that is done with Multimedia Components). But the build in TBBs only consider ECL items which have implemented the `GetDirectLinkToPublished` method. They do not work with providers that implemented the `GetContent` method expecting SDL Tridion to publish their content.

This TBB is meant as a replacement for the **Search External Content Library items** and **Resolve External Content Library items** TBBs which come with 2013 ECL and handles correctly implementations of either  `GetDirectLinkToPublished` or `GetContent` (if `GetDirectLinkToPublished` returns null or an empty string, it tries `GetContent` assuming the content must be published by SDL Tridion).

It also resolves ECL links in CSS using the `url("ecl:uri")` statement, similar to how you can use a TCMURI in there.

# Details #

this TBB can be added in the **Default Finish Actions** as the first item, or be placed anywhere in your Component or Page Templates as long as it comes after the DWT TBB and before the **Publish Binaries in Package** TBB .

The **Adjust SiteEdit 2012 markup for External Content Library items** which comes with 2013 ECL still remains valuable to resolve XPM/SiteEdit markup for direct ECL links.