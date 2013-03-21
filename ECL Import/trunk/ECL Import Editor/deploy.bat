@echo off

REM echo copying %2 to %3
REM copy /y %2 %3

echo deploying editor to tridion
copy %1\Configuration\*.* "C:\Tridion\web\WebUI\Editors\EclImport\Configuration"
copy %1\Scripts\Commands.js "C:\Tridion\web\WebUI\Editors\EclImport\Scripts"
copy %1\Scripts\SiteEdit.js "C:\Tridion\web\WebUI\Editors\EclImport\Scripts"
copy %1\Themes\theme.css "C:\Tridion\web\WebUI\Editors\EclImport\Themes"
