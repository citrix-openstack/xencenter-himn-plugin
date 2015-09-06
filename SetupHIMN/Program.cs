using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XenAPI;
using System.Net;
using System.IO;
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

        static string getVIF(Session session, string netRef, string vmRef)
        {
            List<XenRef<VIF>> vifRefs = VIF.get_all(session);
            foreach (XenRef<VIF> vifRef in vifRefs)
            {
                VIF vif = VIF.get_record(session, vifRef);

                if (vif.network.opaque_ref == netRef &&
                    vif.VM.opaque_ref == vmRef)
                {
                    return vifRef.opaque_ref;
                }
            }
            return null;
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

        static StreamWriter CreateLogger(string log_root)
        {
            if (!Directory.Exists(log_root))
            {
                Directory.CreateDirectory(log_root);
            }
            string logfile = DateTime.Now.ToString("yyyyMMddHHmmss");
            return new StreamWriter(Path.Combine(log_root, logfile + ".log")) { AutoFlush = true };
        }
        static void AddHIMN(StreamWriter logger, string url, string sessionRef, string cls, string vm_uuid)
        {
            logger.WriteLine("");
            System.Console.WriteLine();

            logger.WriteLine(string.Format("url: {0}", url));
            logger.WriteLine(string.Format("sessionRef: {0}", sessionRef));
            logger.WriteLine(string.Format("cls: {0}", cls));
            logger.WriteLine(string.Format("vm_uuid: {0}", vm_uuid));

            if (cls != "VM")
            {
                return;
            }

            System.Console.WriteLine("Discovering VM...");

            Session session = new Session(url, sessionRef);
            logger.WriteLine("session created");

            string vmRef = getVM(session, vm_uuid);
            if (string.IsNullOrEmpty(vmRef))
            {
                logger.WriteLine("vm not founded");
                return;
            }
            else
            {
                logger.WriteLine("vm founded");
            }
            VM vm = VM.get_record(session, vmRef);
            logger.WriteLine("vm.power_state:" + vm.power_state);
            if (vm.power_state != vm_power_state.Halted)
            {
                string s = "Management network need to be added when the VM '{0}' is powered off.\n" +
                    "Please shut down VM '{0}' first then redo.";
                System.Console.WriteLine(s, vm.name_label);
                logger.WriteLine(string.Format("VM '{0}' need to be shutdown", vm.name_label));
                return;
            }

            System.Console.WriteLine("Adding network 'xenapi' to VM '{0}'...", vm.name_label);

            string netRef = getNetwork(session, "xenapi");
            if (string.IsNullOrEmpty(netRef))
            {
                logger.WriteLine("xenapi not founded");
                return;
            }
            else
            {
                logger.WriteLine("xenapi founded");
            }

            string device = "9";
            string vifRef = getVIF(session, netRef, vmRef);
            VIF vif;
            if (string.IsNullOrEmpty(vifRef))
            {
                vifRef = createVIF(session, netRef, vmRef, device);
                vif = VIF.get_record(session, vifRef);
                logger.WriteLine(string.Format("vif {0} created", vifRef));
                System.Console.WriteLine("Management network added as VIF <{0}>, " +
                    "but not be visible in XenCenter", vif.device);
            }
            else
            {
                logger.WriteLine(string.Format("vif {0} existed", vifRef));
                vif = VIF.get_record(session, vifRef);
                System.Console.WriteLine("Management network already existed as VIF <{0}>, " +
                    "but not be visible in XenCenter", vif.device);
            }
            //VIF.plug(session, vifRef);


            logger.WriteLine(string.Format("himn_mac: {0}", vif.MAC));

            string himn_mac_key = "vm-data/himn_mac";
            Dictionary<string, string> xenstore_data = VM.get_xenstore_data(session, vmRef);
            if (xenstore_data.ContainsKey(himn_mac_key))
            {
                logger.WriteLine(string.Format("{0}: {1} => {2}",
                    himn_mac_key, xenstore_data[himn_mac_key], vif.MAC));
                xenstore_data[himn_mac_key] = vif.MAC;

            }
            else
            {
                xenstore_data.Add(himn_mac_key, vif.MAC);
                logger.WriteLine(string.Format("{0}: {1}", himn_mac_key, vif.MAC));
            }

            VM.set_xenstore_data(session, vmRef, xenstore_data);
            logger.WriteLine("xenstore written");
        }


        static void Main(string[] args)
        {
            string log_root = @"Plugins\Citrix\SetupHIMN\Logs";
            StreamWriter logger = CreateLogger(log_root);

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            if (args.Length < 4 || args.Length % 4 != 0)
            {
                logger.WriteLine(string.Format("Invalid paramenter length: {0}", args.Length));
                System.Console.WriteLine("Invalid paramenter length: {0}", args.Length);
                return;
            }

            for (int i = 0; i < args.Length; i += 4)
            {
                string url = args[i];
                string sessionRef = args[i + 1];
                string cls = args[i + 2];
                string vm_uuid = args[i + 3];
                AddHIMN(logger, url, sessionRef, cls, vm_uuid);
            }

            logger.Close();
            System.Console.WriteLine("");
            System.Console.WriteLine("Press Enter Key to continue.");
            System.Console.ReadLine();
        }
    }
}
