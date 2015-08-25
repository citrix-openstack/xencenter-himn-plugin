using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XenAPI;
using System.Net;
using System.Text.RegularExpressions;

namespace SetupHIMN
{
    class Program
    {
        static string getVM(Session session, string vm_uuid)
        {
            List<XenRef<VM>> vmRefs = VM.get_all(session);
            foreach (XenRef<VM> vmRef in vmRefs)
            {
                VM vm = VM.get_record(session, vmRef);
                if (vm.uuid == vm_uuid)
                {
                    return vmRef.opaque_ref;
                }
            }
            return null;
        }

        static string getNetwork(Session session, string bridge)
        {
            List<XenRef<Network>> netRefs = Network.get_all(session);
            foreach (XenRef<Network> netRef in netRefs)
            {
                Network net = Network.get_record(session, netRef);
                if (net.bridge == bridge)
                {
                    return netRef.opaque_ref;
                }
            }
            return null;
        }

        static string getVIF(Session session, string netRef, string vmRef, string device)
        {
            List<XenRef<VIF>> vifRefs = VIF.get_all(session);
            foreach (XenRef<VIF> vifRef in vifRefs)
            {
                VIF vif = VIF.get_record(session, vifRef);

                if (vif.network.opaque_ref == netRef &&
                    vif.VM.opaque_ref == vmRef && vif.device == device)
                {
                    return vifRef.opaque_ref;
                }
            }
            return null;
        }

        static string getNextDevice(Session session, string vmRef)
        {
            List<XenRef<VIF>> vifRefs = VIF.get_all(session);
            int device = 0;
            foreach (XenRef<VIF> vifRef in vifRefs)
            {
                VIF vif = VIF.get_record(session, vifRef);

                if (Regex.IsMatch(vif.device, @"^\d+$") && vif.VM.opaque_ref == vmRef)
                {
                    device = Math.Max(int.Parse(vif.device), device);
                }
            }
            return (device + 1).ToString();
        }

        static string createVIF(Session session, string netRef, string vmRef, string device)
        {
            VIF vif = new VIF()
            {
                VM = new XenRef<VM>(vmRef),
                network = new XenRef<Network>(netRef),
                device = device
            };
            XenRef<VIF> vifRef = VIF.create(session, vif);

            return vifRef.opaque_ref;
        }

        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            if (args.Length < 4)
            {
                return;
            }

            string url = args[0];
            string sessionRef = args[1];
            string cls = args[2];
            string vm_uuid = args[3];


            if (cls != "VM")
            {
                return;
            }
            System.Console.WriteLine("connection url: " + url);
            Session session = new Session(url, sessionRef);
            System.Console.WriteLine("session: " + session.uuid);
            string netRef = getNetwork(session, "xenapi");
            System.Console.WriteLine("net: " + netRef);
            string vmRef = getVM(session, vm_uuid);
            System.Console.WriteLine("vm: " + vmRef);
            //string device = getNextDevice(session, vmRef);
            //System.Console.WriteLine("device: " + device);
            string device = "9";
            string vifRef = getVIF(session, netRef, vmRef, device);
            if (vifRef == null)
            {
                vifRef = createVIF(session, netRef, vmRef, device);
                System.Console.WriteLine("vif: " + vifRef + " created");
            }
            else
            {
                System.Console.WriteLine("vif: " + vifRef + " existed");
            }
            try
            {
                VIF.plug(session, vifRef);
            }
            catch (Exception)
            {
            }

            System.Console.WriteLine();
        }
    }
}
