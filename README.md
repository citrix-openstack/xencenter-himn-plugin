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

	xencenter-himn-plugin\SetupHIMN\output\SetupHIMN.msi


How to Use
----------

+ Restart XenCenter
+ Right-click on the selected VMs, and click on "Add management network" in the context menu.
