Follow these steps to use this cartridge:

1. compile the code and create a jar file, making sure that pnp_processor_conf.xml is excluded from the jar.
2. copy the jar file to the lib folder of your website.
3. copy the file pnp_processor_conf.xml to the classes folder of your website.
4. edit the file to provide the appropriate cookie name for the site, as configured in cd_wai_conf.xml.
5. add the relevant cartridge configuration to cd_ambient_conf.xml for your website.

Claims will be added using one of two claim subjects. These are:

1. taf:claim:pnp:characteristics for Customer Characteristics.
2. taf:claim:pnp:trackingkeys for Tracking Keys.

Claim will be added for each characteristic or tracking key found in the relevant collections.