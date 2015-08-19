#Load DLLs

foreach($parameterSet in $ObjInfoArray)
{
	Plugins\Citrix\SetupHIMN\SetupHIMN.exe $parameterSet["url"] $parameterSet["sessionRef"] $parameterSet["class"] $parameterSet["objUuid"]
}
