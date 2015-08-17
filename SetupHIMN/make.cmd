echo PowerShell version 2.0 or higher is required to run this
echo WiX 3.7 is assumed installed to compile the installer

del .\SetupHIMN.msi /q
del .\plugins /q
del .\output /q
mkdir plugins\Citrix\SetupHIMN
mkdir output
cp SetupHIMN* plugins\Citrix\SetupHIMN
cp putty.exe plugins\Citrix\SetupHIMN


Echo creating installer
powershell -ExecutionPolicy ByPass -File ..\PluginInstaller\Create-PluginInstaller.ps1 -out .\output\SetupHIMN.msi -title "XenCenter Setup HIMN Plugin" -description "Setup Host Internal Management Network for Guest VM" -manufacturer "Citrix" -upgrade_code $([System.Guid]::NewGuid().ToString())

Del .\output\*.r* /q
Del .\output\*.w* /q

Echo Done.