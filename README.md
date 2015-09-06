xencenter-himn-plugin
=====================

xencenter-himn-plugin is a XenCenter Plugin to add Host Internal Management Network (HIMN) to a guest VM. Please be noted that you need to run the make file from a Visual Sutdio Command Prompt.

Prerequisites
-------------

+ PowerShell version 2.0
+ WiX 3.7

Compile
-------

	xencenter-himn-plugin\SetupHIMN\make.cmd

Install
-------

	xencenter-himn-plugin\SetupHIMN\output\SetupHIMN-VERSION.msi

How to Use
----------

+ Restart XenCenter
+ Right-click on the selected VMs, and click on "Add management network" in the context menu.

Logs
----

All log files will be stored under `%programfiles(x86)%\Citrix\XenCenter\Plugins\Citrix\SetupHIMN\Logs` separately.

Under the hood
--------------

Basically this tool is for plugging network 'xenapi' to selected VM and write the Mac Address of created VIF into xenstore.

To check the existence of created VIF, you can execute below command line under XenServer.

	xe vif-list network-uuid="$(xe network-list bridge=xenapi minimal=true)" vm-uuid="$(xe vm-list name-label="VM_NAME_LABEL" --minimal)" --minimal

To check whether the Mac Address of management network has been written into xenstore, you can execute below command line under XenServer.

	xenstore-read /local/domain/$(xe vm-list params=dom-id name-label="VM_NAME_LABEL" --minimal)/vm-data/himn_mac

Or below command line under guest VMs with xs-tools installed.

	xenstore-read /local/domain/$(xenstore-read domid)/vm-data/himn_mac
