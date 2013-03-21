@echo off

echo copying %2 to %3
copy /y %2 %3

echo deploying model to tridion
copy %1\Configuration\*.* "C:\Tridion\web\WebUI\Models\EclImport\Configuration"
copy %1\Services\ImportService.svc "C:\Tridion\web\WebUI\Models\EclImport\Services"
copy %1\web.config "C:\Tridion\web\WebUI\Models\EclImport"
