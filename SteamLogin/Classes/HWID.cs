using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace SteamLogin
{
    class HWID
    {

        // Get UUID
        public string GetUniqueID(string drive = null)
        {
            if (drive == null)
            {
                foreach (DriveInfo compDrive in DriveInfo.GetDrives())
                {
                    if (compDrive.IsReady)
                    {
                        drive = compDrive.RootDirectory.ToString();
                        break;
                    }
                }
            }

            if (drive.EndsWith(":\\")) drive = drive.Substring(0, drive.Length - 2);

            string volumeSerial = GetVolumeSerial(drive);
            string cpuID = GetCPUID();

            Guid result;

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(cpuID + cpuID + volumeSerial + cpuID));
                result = new Guid(hash);
            }

            return result.ToString();
        }

        // Get Serial No. of Volume
        private string GetVolumeSerial(string drive)
        {
            ManagementObject disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            disk.Get();

            string volumeSerial = disk["VolumeSerialNumber"].ToString();
            disk.Dispose();

            return volumeSerial;
        }

        // Get ID for CPU
        private string GetCPUID()
        {
            string cpuInfo = "";

            ManagementClass managClass = new ManagementClass("win32_processor");
            ManagementObjectCollection managCollec = managClass.GetInstances();

            foreach (ManagementObject managObj in managCollec)
            {
                if (cpuInfo == "")
                {
                    cpuInfo = managObj.Properties["processorID"].Value.ToString();
                    break;
                }
            }

            return cpuInfo;
        }

    }
}
