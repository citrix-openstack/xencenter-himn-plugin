echo PowerShell version 2.0 or higher is required
echo Microsoft .Net Framework SDK 3.5 or higher is required
echo WiX 3.7 is assumed installed to compile the installer

@set "PATH=%windir%\Microsoft.NET\Framework\v3.5;%PATH%"

for /f "delims=" %%L in (branding.inc) do SET "%%L"

echo "Company Name: %MANUFACTURER_NAME%"
echo "Hypervisor Name: %HYPERVISOR_NAME%"
echo "Client Name: %PRODUCT_NAME%"
echo "Plugin Version: %PLUGIN_VERSION%"
echo "PRODUCT Code: %PRODUCT_CODE%"

del .\SetupHIMN.msi /q
rmdir .\plugins /s /q
rmdir .\output /s /q
mkdir plugins\%MANUFACTURER_NAME%\SetupHIMN
mkdir output

copy SetupHIMN* plugins\%MANUFACTURER_NAME%\SetupHIMN
copy *.dll plugins\%MANUFACTURER_NAME%\SetupHIMN
copy AppIcon.ico plugins\%MANUFACTURER_NAME%\SetupHIMN

csc /target:winexe /out:plugins\%MANUFACTURER_NAME%\SetupHIMN\SetupHIMN.exe /reference:XenServer.dll Program.cs HIMNForm.cs HIMNForm.Designer.cs

Echo creating installer
powershell -ExecutionPolicy ByPass -File ..\PluginInstaller\Create-PluginInstaller.ps1 -out .\output\SetupHIMN-%PLUGIN_VERSION%.msi -title "%PRODUCT_NAME% Setup HIMN Plugin" -description "Setup Host Internal Management Network for Guest VM" -manufacturer "%MANUFACTURER_NAME%" -upgrade_code $([System.Guid]::NewGuid().ToString()) -product_code "%PRODUCT_CODE%"

Del .\output\*.r* /q
Del .\output\*.w* /q

Echo Done.