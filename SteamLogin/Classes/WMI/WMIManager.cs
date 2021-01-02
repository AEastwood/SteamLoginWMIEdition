using Newtonsoft.Json;
using SteamLogin.Classes.WMI;
using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;

namespace SteamLogin.Classes
{
    class WMIManager
    {
        public List<Instance> instances = new List<Instance>();
        private readonly string WMINamespace = @"root\virtualization\v2";
        private ManagementObjectSearcher WMISearch;

        // Change state of virtual machine
        public void SetInstanceState(Instance instance, int state)
        {
            ManagementScope WMIScope = new ManagementScope(this.WMINamespace, null);
            ManagementObject WMIObject = Utility.GetTargetComputer(instance.UUID, WMIScope);

            if (WMIObject == null)
            {
                MessageBox.Show("Specified virtual machine doesn't exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ManagementBaseObject inParams = WMIObject.GetMethodParameters("RequestStateChange");

            inParams["RequestedState"] = state;

            ManagementBaseObject outParams = WMIObject.InvokeMethod("RequestStateChange", inParams, null);

            switch((uint)outParams["ReturnValue"])
            {
                case ReturnCode.Started:
                case ReturnCode.Completed:
                    MessageBox.Show("State changed successfully", "State Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;

                default:
                    MessageBox.Show("Unable to change state", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }
        }

        // Try to create and return new instance
        public void CreateInstanceForEach(ManagementObjectCollection WMIResults)
        {

            foreach (ManagementObject WMIResult in WMIResults)
            {
                if (WMIResult == null) throw new NullReferenceException(JsonConvert.SerializeObject(WMIResult));
                if (WMIResult["ElementName"].ToString() == Environment.MachineName) continue;

                try
                {
                    Instance instance = new Instance()
                    {
                        ElementName = WMIResult["ElementName"].ToString(),
                        EnabledState = int.Parse(WMIResult["EnabledState"].ToString()),
                        HealthState = int.Parse(WMIResult["HealthState"].ToString()),
                        UUID = WMIResult["Name"].ToString(),
                        OnTime = int.Parse(WMIResult["OnTimeInMilliseconds"].ToString()),
                        Status = WMIResult["Status"].ToString()
                    };

                    this.instances.Add(instance);
                }

                catch (NullReferenceException e)
                {
                    MessageBox.Show($"Non valid ManagementObject provided: \n\nJSON: {e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Property doesn't exist: {e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Gets and returns all instances of a vm from WMI
        public void GetVirtualMachines()
        {
            try
            {
                WMISearch = new ManagementObjectSearcher(this.WMINamespace, "SELECT * FROM Msvm_ComputerSystem");
                this.CreateInstanceForEach(WMISearch.Get());
            }
            catch (ManagementException e)
            {
                MessageBox.Show("Error Occurred in WMIManager:\n\n" + e.GetBaseException(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
