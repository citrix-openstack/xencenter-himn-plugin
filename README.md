xencenter-himn-plugin
=====================

xencenter-himn-plugin is a XenCenter Plugin to add Host Internal Management Network (HIMN) to a guest VM.

Download and Install
--------------------

Download SetupHIMN-VERSION.msi from <http://ca.downloads.xensource.com/OpenStack/Plugins/> and install it to XenCenter.


Compile and Install
-------------------

Make sure you have _Microsoft .Net Framework SDK 3.5+_, _PowerShell version 2.0+_ and _WiX 3.7_ installed and run `make.cmd`. The msi file will be generated under `xencenter-himn-plugin\SetupHIMN\output\`.


How to Use
----------

+ Restart XenCenter
+ Right-click on the selected VMs, and click on "Add management network" in the context menu. ![](https://raw.githubusercontent.com/citrix-openstack/xencenter-himn-plugin/master/doc/images/xchimn00.png)
+ Wait for status detection for all selected VMs and click "Add management network" button. ![](https://raw.githubusercontent.com/citrix-openstack/xencenter-himn-plugin/master/doc/images/xchimn10.png)
+ Management network then will be added and shown with generated MAC address. ![](https://raw.githubusercontent.com/citrix-openstack/xencenter-himn-plugin/master/doc/images/xchimn20.png)


Logs
----

All log files will be stored as `%LOCALAPPDATA%/Temp/XCHIMN.log`.

Under the hood
--------------

Basically this tool is for plugging network 'xenapi' to selected VM and write the Mac Address of created VIF into xenstore.

To check the existence of created VIF, you can execute below command line under XenServer.

	xe vif-list network-uuid="$(xe network-list bridge=xenapi minimal=true)" vm-uuid="$(xe vm-list name-label="VM_NAME_LABEL" --minimal)" --minimal

To check whether the Mac Address of management network has been written into xenstore, you can execute below command line under XenServer.

	xenstore-read /local/domain/$(xe vm-list params=dom-id name-label="VM_NAME_LABEL" --minimal)/vm-data/himn_mac

Or below command line under guest VMs with xs-tools installed.

	xenstore-read /local/domain/$(xenstore-read domid)/vm-data/himn_mac
