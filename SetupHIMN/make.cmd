echo PowerShell version 2.0 or higher is required to run this
echo WiX 3.7 is assumed installed to compile the installer

for /f "tokens=4 delims= " %%a in ('findstr /r /c:"plugin_version=\"[0-9][0-9]*.[0-9][0-9]*.[0-9][0-9]*\"" SetupHIMN.xcplugin.xml') do @set seg=%%a

set ver=%seg:~16,-2%

del .\SetupHIMN.msi /q
del .\plugins /q
del .\output /q
mkdir plugins\Citrix\SetupHIMN
mkdir output


cp SetupHIMN* plugins\Citrix\SetupHIMN
cp *.dll plugins\Citrix\SetupHIMN
cp AppIcon.ico plugins\Citrix\SetupHIMN

csc /target:winexe /out:plugins\Citrix\SetupHIMN\SetupHIMN.exe /reference:XenServer.dll Program.cs HIMNForm.cs HIMNForm.Designer.cs

Echo creating installer
powershell -ExecutionPolicy ByPass -File ..\PluginInstaller\Create-PluginInstaller.ps1 -out .\output\SetupHIMN-%ver%.msi -title "XenCenter Setup HIMN Plugin" -description "Setup Host Internal Management Network for Guest VM" -manufacturer "Citrix" -upgrade_code $([System.Guid]::NewGuid().ToString())

Del .\output\*.r* /q
Del .\output\*.w* /q

Echo Done.