using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace SecVers_Debloat.Patches.Hardening
{
    public class NetworkHardening
    {
        private const string NetworkRegPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
        private const string NetworkIPv6Path = @"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters";

        // Disable NetBIOS over TCP/IP
        public void DisableNetBIOS()
        {
            ExecutePowerShell(@"
                $adapters = Get-WmiObject Win32_NetworkAdapterConfiguration -Filter 'IPEnabled=True'
                foreach($adapter in $adapters) {
                    $adapter.SetTcpipNetbios(2)
                }
            ");
        }

        // Disable LLMNR (Link-Local Multicast Name Resolution)
        public void DisableLLMNR()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows NT\DNSClient",
                "EnableMulticast", 0, RegistryValueKind.DWord);
        }

        // Disable SMBv1
        public void DisableSMBv1()
        {
            ExecutePowerShell("Disable-WindowsOptionalFeature -Online -FeatureName SMB1Protocol -NoRestart");
        }

        // Enable SMB Encryption
        public void EnableSMBEncryption()
        {
            ExecutePowerShell("Set-SmbServerConfiguration -EncryptData $true -Force");
        }

        // Disable SMB Compression (CVE-2020-0796)
        public void DisableSMBCompression()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters",
                "DisableCompression", 1, RegistryValueKind.DWord);
        }

        // Enable Windows Firewall for all profiles
        public void EnableFirewallAllProfiles()
        {
            ExecutePowerShell(@"
                Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled True
            ");
        }

        // Block inbound connections by default
        public void SetFirewallDefaultBlockInbound()
        {
            ExecutePowerShell(@"
                Set-NetFirewallProfile -Profile Domain,Public,Private -DefaultInboundAction Block
            ");
        }

        // Disable IPv6 if not needed
        public void DisableIPv6()
        {
            SetRegistryValue(NetworkIPv6Path, "DisabledComponents", 0xFF, RegistryValueKind.DWord);
        }

        // Enable TCP/IP Stack Hardening
        public void EnableTCPStackHardening()
        {
            // SYN Attack Protection
            SetRegistryValue(NetworkRegPath, "SynAttackProtect", 1, RegistryValueKind.DWord);
            // Enable Dead Gateway Detection
            SetRegistryValue(NetworkRegPath, "EnableDeadGWDetect", 0, RegistryValueKind.DWord);
            // Disable ICMP Redirects
            SetRegistryValue(NetworkRegPath, "EnableICMPRedirect", 0, RegistryValueKind.DWord);
            // Enable TCP Timestamps
            SetRegistryValue(NetworkRegPath, "Tcp1323Opts", 1, RegistryValueKind.DWord);
        }

        // Disable WPAD (Web Proxy Auto-Discovery)
        public void DisableWPAD()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Wpad",
                "WpadOverride", 1, RegistryValueKind.DWord);
        }

        // Disable Windows Connect Now
        public void DisableWindowsConnectNow()
        {
            ExecutePowerShell("Stop-Service wcncsvc; Set-Service wcncsvc -StartupType Disabled");
        }

        // Disable LMHOSTS Lookup
        public void DisableLMHOSTS()
        {
            SetRegistryValue(NetworkRegPath, "EnableLMHosts", 0, RegistryValueKind.DWord);
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
                RedirectStandardError = true
            };

            using (Process process = Process.Start(psi))
            {
                process?.WaitForExit();
            }
        }
    }
}
