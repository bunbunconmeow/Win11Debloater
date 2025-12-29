using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Patches.Hardening
{
    public class SystemIntegrity
    {
        // Enable Secure Boot
        public void VerifySecureBoot()
        {
            ExecutePowerShell("Confirm-SecureBootUEFI");
        }

        // Enable Windows Defender Application Control (WDAC)
        public void EnableWDAC()
        {
            ExecutePowerShell(@"
                $policy = Get-CimInstance -Namespace root/Microsoft/Windows/CI -ClassName CIPolicy
                if($policy) { Set-CimInstance -InputObject $policy -Property @{IsSystemPolicy=$true} }
            ");
        }

        // Enable Kernel-mode Code Integrity
        public void EnableKernelModeCodeIntegrity()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity",
                "Enabled", 1, RegistryValueKind.DWord);
        }

        // Enable Credential Guard
        public void EnableCredentialGuard()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Lsa",
                "LsaCfgFlags", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\DeviceGuard",
                "EnableVirtualizationBasedSecurity", 1, RegistryValueKind.DWord);
        }

        // Enable Virtualization Based Security
        public void EnableVirtualizationBasedSecurity()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\DeviceGuard",
                "EnableVirtualizationBasedSecurity", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\DeviceGuard",
                "RequirePlatformSecurityFeatures", 3, RegistryValueKind.DWord);
        }

        // Enable Early Launch Anti-Malware (ELAM)
        public void EnableEarlyLaunchAntiMalware()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\EarlyLaunch",
                "DriverLoadPolicy", 1, RegistryValueKind.DWord);
        }

        // Enable Driver Signature Enforcement
        public void EnableDriverSignatureEnforcement()
        {
            ExecuteCommand("bcdedit.exe", "/set nointegritychecks off");
            ExecuteCommand("bcdedit.exe", "/set testsigning off");
        }

        // Block Suspicious Drivers (Vulnerable Driver Blocklist)
        public void EnableVulnerableDriverBlocklist()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\CI\Config",
                "VulnerableDriverBlocklistEnable", 1, RegistryValueKind.DWord);
        }

        // Enable Memory Integrity (Core Isolation)
        public void EnableMemoryIntegrity()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity",
                "Enabled", 1, RegistryValueKind.DWord);
        }

        // Disable AutoRun/AutoPlay
        public void DisableAutoRun()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                "NoDriveTypeAutoRun", 255, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                "NoAutorun", 1, RegistryValueKind.DWord);
        }

        // Disable 8.3 File Name Creation
        public void Disable8Dot3Names()
        {
            ExecuteCommand("fsutil.exe", "behavior set disable8dot3 1");
        }

        // Enable File System Encryption (BitLocker)
        public void CheckBitLockerStatus()
        {
            ExecutePowerShell("Get-BitLockerVolume");
        }

        // Disable Remote Assistance
        public void DisableRemoteAssistance()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Remote Assistance",
                "fAllowToGetHelp", 0, RegistryValueKind.DWord);
        }

        // Disable Hibernation (to prevent cold boot attacks)
        public void DisableHibernation()
        {
            ExecuteCommand("powercfg.exe", "-h off");
        }

        // Enable Windows Defender Tamper Protection
        public void EnableTamperProtection()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows Defender\Features",
                "TamperProtection", 5, RegistryValueKind.DWord);
        }

        // Configure Windows Update to Download Only
        public void SetWindowsUpdateSecure()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                "NoAutoUpdate", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                "AUOptions", 3, RegistryValueKind.DWord);
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
