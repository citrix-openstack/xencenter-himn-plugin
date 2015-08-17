@Echo off
Set CurrentDir=%~dp0
Set CurrentDir=%CurrentDir:~0,-1%

Set HOST=%~1
Set HOST=%HOST:~8,-5%
Set TYPE=%~3
Set VM=%~4

set /p PASSWORD=Please input root password:

echo vm_uuid="%VM%" > "%CurrentDir%\SetupHIMN.tmp.sh"
cat "%CurrentDir%\SetupHIMN.sh" >> "%CurrentDir%\SetupHIMN.tmp.sh"

"%CurrentDir%\Putty.exe" "root@%HOST%" -pw "%PASSWORD%" -m "%CurrentDir%\SetupHIMN.tmp.sh"
