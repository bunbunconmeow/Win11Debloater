using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Patches.Hardening
{
    public class ServiceHardening
    {
        // Disable Remote Registry
        public void DisableRemoteRegistry()
        {
            DisableService("RemoteRegistry");
        }

        // Disable Remote Desktop
        public void DisableRemoteDesktop()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Terminal Server",
                "fDenyTSConnections", 1, RegistryValueKind.DWord);
            DisableService("TermService");
        }

        // Disable Print Spooler
        public void DisablePrintSpooler()
        {
            DisableService("Spooler");
        }

        // Disable Windows Script Host
        public void DisableWindowsScriptHost()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows Script Host\Settings",
                "Enabled", 0, RegistryValueKind.DWord);
        }

        // Disable PowerShell v2
        public void DisablePowerShellV2()
        {
            ExecutePowerShell("Disable-WindowsOptionalFeature -Online -FeatureName MicrosoftWindowsPowerShellV2Root -NoRestart");
        }

        // Disable Windows Error Reporting
        public void DisableWindowsErrorReporting()
        {
            DisableService("WerSvc");
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\Windows Error Reporting",
                "Disabled", 1, RegistryValueKind.DWord);
        }

        // Disable Telemetry Services
        public void DisableTelemetryServices()
        {
            DisableService("DiagTrack");
            DisableService("dmwappushservice");
        }

        // Disable Xbox Services
        public void DisableXboxServices()
        {
            DisableService("XblAuthManager");
            DisableService("XblGameSave");
            DisableService("XboxNetApiSvc");
            DisableService("XboxGipSvc");
        }

        // Disable Bluetooth
        public void DisableBluetooth()
        {
            DisableService("bthserv");
        }

        // Disable SSDP Discovery (UPnP)
        public void DisableSSDPDiscovery()
        {
            DisableService("SSDPSRV");
        }

        // Disable UPnP Device Host
        public void DisableUPnPDeviceHost()
        {
            DisableService("upnphost");
        }

        // Disable Computer Browser
        public void DisableComputerBrowser()
        {
            DisableService("Browser");
        }

        // Disable Function Discovery Resource Publication
        public void DisableFunctionDiscovery()
        {
            DisableService("FDResPub");
        }

        // Disable HomeGroup Services
        public void DisableHomeGroup()
        {
            DisableService("HomeGroupListener");
            DisableService("HomeGroupProvider");
        }

        // Disable Windows Media Player Network Sharing
        public void DisableWMPNetworkSharing()
        {
            DisableService("WMPNetworkSvc");
        }

        // Disable Offline Files
        public void DisableOfflineFiles()
        {
            DisableService("CscService");
        }

        private void DisableService(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }

                ExecuteCommand("sc.exe", $"config {serviceName} start=disabled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Service error ({serviceName}): {ex.Message}");
            }
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
