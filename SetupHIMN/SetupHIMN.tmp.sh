vm_uuid="60c79a48-f3c3-bf24-c99a-323a246bcc02" 
device_number=2
net_uuid=$(xe network-list bridge=xenapi minimal=true)
vif_uuid=$(xe vif-list network-uuid="$net_uuid" vm-uuid="$vm_uuid" --minimal)
if [ -z "$vif_uuid" ]; then
	eth2_uuid=$(xe vif-create network-uuid="$net_uuid" vm-uuid="$vm_uuid" device="$device_number")
	echo "$vm_name : HIMN created"
fi

