using System;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Win32;

namespace SecVers_Debloat.Patches
{
    public class SystemDataAnonymizer
    {
        private readonly Random _random = new Random();

        // ==================== MAC ADDRESS ====================

        // Randomize MAC Address for all Network Adapters
        public void RandomizeAllMACAddresses()
        {
            ExecutePowerShell(@"
                Get-NetAdapter | ForEach-Object {
                    $mac = ((1..6 | ForEach-Object { '{0:X2}' -f (Get-Random -Max 256) }) -join '-')
                    Set-NetAdapter -Name $_.Name -MacAddress $mac -Confirm:$false
                }
            ");
        }

        // Set specific MAC Address
        public void SetMACAddress(string adapterName, string macAddress)
        {
            ExecutePowerShell($"Set-NetAdapter -Name '{adapterName}' -MacAddress '{macAddress}' -Confirm:$false");
        }

        // Spoof MAC via Registry (alternative method)
        public void SpoofMACViaRegistry(string macAddress)
        {
            string basePath = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(basePath))
            {
                if (key == null) return;

                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    using (RegistryKey subKey = key.OpenSubKey(subKeyName, true))
                    {
                        if (subKey?.GetValue("DriverDesc") != null)
                        {
                            subKey.SetValue("NetworkAddress", macAddress.Replace("-", "").Replace(":", ""), RegistryValueKind.String);
                        }
                    }
                }
            }
        }

        // ==================== COMPUTER NAME ====================

        // Randomize Computer Name
        public void RandomizeComputerName()
        {
            string newName = "PC-" + GenerateRandomHex(8).ToUpper();
            SetComputerName(newName);
        }

        // Set Custom Computer Name
        public void SetComputerName(string newName)
        {
            ExecuteCommand("wmic.exe", $"computersystem where name='%computername%' call rename name='{newName}'");

            // Also set in registry
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName",
                "ComputerName", newName, RegistryValueKind.String);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName",
                "ComputerName", newName, RegistryValueKind.String);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                "Hostname", newName, RegistryValueKind.String);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                "NV Hostname", newName, RegistryValueKind.String);
        }

        // ==================== MACHINE GUID ====================

        // Randomize Machine GUID
        public void RandomizeMachineGUID()
        {
            string newGuid = Guid.NewGuid().ToString().ToUpper();
            SetRegistryValue(@"SOFTWARE\Microsoft\Cryptography",
                "MachineGuid", newGuid, RegistryValueKind.String);
        }

        // ==================== DISK/VOLUME SERIAL ====================

        // Change Volume Serial Number (requires VolumeID.exe from Sysinternals)
        public void ChangeVolumeSerial(string drive, string newSerial)
        {
            // Note: Requires VolumeID64.exe in PATH or specify full path
            ExecuteCommand("volumeid64.exe", $"{drive}: {newSerial}");
        }

        // Randomize Volume Serial
        public void RandomizeVolumeSerial(string drive)
        {
            string randomSerial = GenerateRandomHex(4).ToUpper() + "-" + GenerateRandomHex(4).ToUpper();
            ChangeVolumeSerial(drive, randomSerial);
        }

        // ==================== HARDWARE IDs (SMBIOS/WMI) ====================

        // Spoof System Serial Number (BIOS)
        public void SpoofSystemSerialNumber(string newSerial)
        {
            SetRegistryValue(@"HARDWARE\DESCRIPTION\System\BIOS",
                "SystemSerialNumber", newSerial, RegistryValueKind.String);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\mssmbios\Data",
                "SMBiosData", Encoding.ASCII.GetBytes(newSerial), RegistryValueKind.Binary);
        }

        // Randomize BIOS Serial Number
        public void RandomizeBIOSSerial()
        {
            string newSerial = "SN-" + GenerateRandomHex(12).ToUpper();
            SpoofSystemSerialNumber(newSerial);
        }

        // Spoof Baseboard (Motherboard) Serial
        public void SpoofBaseboardSerial(string newSerial)
        {
            SetRegistryValue(@"HARDWARE\DESCRIPTION\System\BIOS",
                "BaseBoardSerialNumber", newSerial, RegistryValueKind.String);
        }

        // Randomize Baseboard Serial
        public void RandomizeBaseboardSerial()
        {
            string newSerial = "MB-" + GenerateRandomHex(12).ToUpper();
            SpoofBaseboardSerial(newSerial);
        }

        // Spoof System Product Name
        public void SpoofSystemProductName(string newName)
        {
            SetRegistryValue(@"HARDWARE\DESCRIPTION\System\BIOS",
                "SystemProductName", newName, RegistryValueKind.String);
        }

        // Spoof System Manufacturer
        public void SpoofSystemManufacturer(string newManufacturer)
        {
            SetRegistryValue(@"HARDWARE\DESCRIPTION\System\BIOS",
                "SystemManufacturer", newManufacturer, RegistryValueKind.String);
        }

        // ==================== DISK/HDD SERIAL ====================
        // This attempts registry-level changes

        public void SpoofDiskSerial(string newSerial)
        {
            SetRegistryValue(@"HARDWARE\DEVICEMAP\Scsi",
                "SerialNumber", newSerial, RegistryValueKind.String);
        }

        // ==================== GPU SERIAL/ID ====================

        public void SpoofGPUDeviceID(string newDeviceID)
        {
            string gpuPath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";
            SetRegistryValue(gpuPath, "HardwareInformation.AdapterString", newDeviceID, RegistryValueKind.String);
        }

        // ==================== INSTALLATION ID ====================

        public void RandomizeInstallationID()
        {
            string newInstallID = GenerateRandomHex(32).ToUpper();
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                "InstallationID", newInstallID, RegistryValueKind.String);
        }

        // ==================== PRODUCT ID ====================

        public void SpoofProductID(string newProductID)
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                "ProductId", newProductID, RegistryValueKind.String);
        }

        public void RandomizeProductID()
        {
            string newProductID = $"{GenerateRandomNumber(5)}-{GenerateRandomNumber(5)}-{GenerateRandomNumber(5)}-{GenerateRandomNumber(5)}";
            SpoofProductID(newProductID);
        }

        // ==================== WINDOWS INSTALLATION DATE ====================

        public void RandomizeInstallDate()
        {
            int randomDaysAgo = _random.Next(30, 730);
            DateTime randomDate = DateTime.Now.AddDays(-randomDaysAgo);
            int unixTimestamp = (int)(randomDate.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                "InstallDate", unixTimestamp, RegistryValueKind.DWord);
        }

        // ==================== SYSTEM BUILD INFO ====================

        public void SpoofBuildLab(string newBuildLab)
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                "BuildLab", newBuildLab, RegistryValueKind.String);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                "BuildLabEx", newBuildLab, RegistryValueKind.String);
        }

        // ==================== USER/OWNER INFO ====================

        public void RandomizeRegisteredOwner()
        {
            string newOwner = "User" + GenerateRandomNumber(6);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                "RegisteredOwner", newOwner, RegistryValueKind.String);
        }

        public void RandomizeRegisteredOrganization()
        {
            string newOrg = "Org" + GenerateRandomNumber(6);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                "RegisteredOrganization", newOrg, RegistryValueKind.String);
        }

        // ==================== TRACKING IDs ====================

        public void DisableAndClearAdvertisingID()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
                "Enabled", 0, RegistryValueKind.DWord);

            // Clear existing ID
            ExecutePowerShell(@"
                Remove-ItemProperty -Path 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo' -Name 'Id' -ErrorAction SilentlyContinue
            ");
        }

        public void ClearTelemetryID()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\SQMClient",
                "MachineId", "{00000000-0000-0000-0000-000000000000}", RegistryValueKind.String);
        }

        // ==================== ETHERNET ADAPTER INFO ====================

        public void SpoofNetworkAdapterInfo()
        {
            ExecutePowerShell(@"
                Get-NetAdapter | ForEach-Object {
                    $randomSerial = -join ((48..57) + (65..90) | Get-Random -Count 16 | ForEach-Object {[char]$_})
                    Set-NetAdapterAdvancedProperty -Name $_.Name -DisplayName 'Network Address' -DisplayValue $randomSerial -ErrorAction SilentlyContinue
                }
            ");
        }

        // ==================== BLUETOOTH MAC ====================

        public void RandomizeBluetoothMAC()
        {
            string newMAC = GenerateRandomMAC();
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\BTHPORT\Parameters\Keys",
                "BluetoothAddress", newMAC, RegistryValueKind.String);
        }

        // ==================== TPM CLEARING (WARNING: DANGEROUS) ====================

        public void ClearTPM()
        {
            // WARNING: This will reset BitLocker and other TPM-dependent features
            ExecutePowerShell("Clear-Tpm -UsPhysicalPresence");
        }

        // ==================== COMPREHENSIVE ANONYMIZATION ====================

        public void AnonymizeEverything()
        {
            // EXTREME CAUTION: This will change EVERYTHING
            RandomizeComputerName();
            RandomizeMachineGUID();
            RandomizeBIOSSerial();
            RandomizeBaseboardSerial();
            RandomizeInstallationID();
            RandomizeProductID();
            RandomizeInstallDate();
            RandomizeRegisteredOwner();
            RandomizeRegisteredOrganization();
            RandomizeAllMACAddresses();
            DisableAndClearAdvertisingID();
            ClearTelemetryID();

            Debug.WriteLine("SYSTEM FULLY ANONYMIZED - RESTART REQUIRED!");
        }

        // ==================== HELPER METHODS ====================

        private string GenerateRandomHex(int length)
        {
            byte[] buffer = new byte[length / 2];
            _random.NextBytes(buffer);
            return BitConverter.ToString(buffer).Replace("-", "");
        }

        private string GenerateRandomNumber(int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(_random.Next(0, 10));
            }
            return sb.ToString();
        }

        private string GenerateRandomMAC()
        {
            byte[] mac = new byte[6];
            _random.NextBytes(mac);
            mac[0] = (byte)(mac[0] & 0xFE); // Ensure unicast
            mac[0] = (byte)(mac[0] | 0x02);  // Ensure locally administered

            return string.Join("-", Array.ConvertAll(mac, b => b.ToString("X2")));
        }

        private void SetRegistryValue(string path, string name, object value, RegistryValueKind kind)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(path))
                {
                    key?.SetValue(name, value, kind);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registry error: {ex.Message}");
            }
        }

        private void ExecutePowerShell(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Verb = "runas"
            };

            using (Process process = Process.Start(psi))
            {
                process?.WaitForExit();
            }
        }

        private void ExecuteCommand(string fileName, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using (Process process = Process.Start(psi))
            {
                process?.WaitForExit();
            }
        }
    }
}
