echo PowerShell version 2.0 or higher is required
echo Microsoft .Net Framework SDK 3.5 or higher is required
echo WiX 3.7 is assumed installed to compile the installer

@set "PATH=%windir%\Microsoft.NET\Framework\v3.5;%PATH%"

for /f "tokens=4 delims= " %%a in ('findstr /r /c:"plugin_version=\"[0-9][0-9]*.[0-9][0-9]*.[0-9][0-9]*\"" SetupHIMN.xcplugin.xml') do @set seg=%%a

set ver=%seg:~16,-2%

del .\SetupHIMN.msi /q
del .\plugins /q
del .\output /q
mkdir plugins\Citrix\SetupHIMN
mkdir output


copy SetupHIMN* plugins\Citrix\SetupHIMN
copy *.dll plugins\Citrix\SetupHIMN
copy AppIcon.ico plugins\Citrix\SetupHIMN

csc /target:winexe /out:plugins\Citrix\SetupHIMN\SetupHIMN.exe /reference:XenServer.dll Program.cs HIMNForm.cs HIMNForm.Designer.cs

Echo creating installer
powershell -ExecutionPolicy ByPass -File ..\PluginInstaller\Create-PluginInstaller.ps1 -out .\output\SetupHIMN-%ver%.msi -title "XenCenter Setup HIMN Plugin" -description "Setup Host Internal Management Network for Guest VM" -manufacturer "Citrix" -upgrade_code $([System.Guid]::NewGuid().ToString())

Del .\output\*.r* /q
Del .\output\*.w* /q

Echo Done.