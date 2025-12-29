using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;

namespace SecVers_Debloat.Patches
{
    /// <summary>
    /// Manages Windows Defender configuration and settings
    /// Requires Administrator privileges
    /// </summary>
    public class Defender
    {
        #region Constants

        private const string DEFENDER_REG_PATH = @"SOFTWARE\Microsoft\Windows Defender";
        private const string REALTIME_REG_PATH = @"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection";
        private const string SPYNET_REG_PATH = @"SOFTWARE\Microsoft\Windows Defender\Spynet";
        private const string SMARTSCREEN_REG_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";
        private const string FEATURES_REG_PATH = @"SOFTWARE\Microsoft\Windows Defender\Features";
        private const string MPENGINE_REG_PATH = @"SOFTWARE\Microsoft\Windows Defender\MpEngine";
        private const string APPHOST_REG_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost";
        private const string POLICIES_REG_PATH = @"SOFTWARE\Policies\Microsoft\Windows Defender";
        private const string EXCLUSIONS_PATHS_REG = @"SOFTWARE\Microsoft\Windows Defender\Exclusions\Paths";
        private const string EXCLUSIONS_EXT_REG = @"SOFTWARE\Microsoft\Windows Defender\Exclusions\Extensions";
        private const string EXCLUSIONS_PROC_REG = @"SOFTWARE\Microsoft\Windows Defender\Exclusions\Processes";

        #endregion

        #region P/Invoke Declarations

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(IntPtr TokenHandle, int TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);

        private const uint TOKEN_QUERY = 0x0008;
        private const int TokenElevation = 20;

        #endregion

        #region Constructor

        public Defender()
        {
            // No initialization needed
        }

        #endregion

        #region Authorization

        public bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void RequireAdmin()
        {
            if (!IsAdministrator())
                throw new UnauthorizedAccessException("Administrator privileges required for this operation");
        }

        #endregion

        #region Real-Time Protection

        public void SetRealTimeProtection(bool enabled)
        {
            RequireAdmin();
            try
            {
                SetRegistryValue(REALTIME_REG_PATH, "DisableRealtimeMonitoring", enabled ? 0 : 1, RegistryValueKind.DWord);
                SetRegistryValue(POLICIES_REG_PATH + @"\Real-Time Protection", "DisableRealtimeMonitoring", enabled ? 0 : 1, RegistryValueKind.DWord);

                ExecuteCommand("reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableRealtimeMonitoring /t REG_DWORD /d " + (enabled ? "0" : "1") + " /f");

                Debug.WriteLine($"Real-Time Protection set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Real-Time Protection: {ex.Message}", ex);
            }
        }

        public bool GetRealTimeProtection()
        {
            try
            {
                int value = GetRegistryValue(REALTIME_REG_PATH, "DisableRealtimeMonitoring", 0);
                return value == 0;
            }
            catch
            {
                return true;
            }
        }

        public void SetBehaviorMonitoring(bool enabled)
        {
            RequireAdmin();
            try
            {
                SetRegistryValue(REALTIME_REG_PATH, "DisableBehaviorMonitoring", enabled ? 0 : 1, RegistryValueKind.DWord);
                SetRegistryValue(POLICIES_REG_PATH + @"\Real-Time Protection", "DisableBehaviorMonitoring", enabled ? 0 : 1, RegistryValueKind.DWord);
                Debug.WriteLine($"Behavior Monitoring set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Behavior Monitoring: {ex.Message}", ex);
            }
        }

        public bool GetBehaviorMonitoring()
        {
            try
            {
                int value = GetRegistryValue(REALTIME_REG_PATH, "DisableBehaviorMonitoring", 0);
                return value == 0;
            }
            catch
            {
                return true;
            }
        }

        public void SetOnAccessProtection(bool enabled)
        {
            RequireAdmin();
            try
            {
                SetRegistryValue(REALTIME_REG_PATH, "DisableOnAccessProtection", enabled ? 0 : 1, RegistryValueKind.DWord);
                SetRegistryValue(POLICIES_REG_PATH + @"\Real-Time Protection", "DisableOnAccessProtection", enabled ? 0 : 1, RegistryValueKind.DWord);
                Debug.WriteLine($"On Access Protection set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set On Access Protection: {ex.Message}", ex);
            }
        }

        public bool GetOnAccessProtection()
        {
            try
            {
                int value = GetRegistryValue(REALTIME_REG_PATH, "DisableOnAccessProtection", 0);
                return value == 0;
            }
            catch
            {
                return true;
            }
        }

        public void SetScriptScanning(bool enabled)
        {
            RequireAdmin();
            try
            {
                SetRegistryValue(REALTIME_REG_PATH, "DisableScriptScanning", enabled ? 0 : 1, RegistryValueKind.DWord);
                SetRegistryValue(POLICIES_REG_PATH + @"\Real-Time Protection", "DisableScriptScanning", enabled ? 0 : 1, RegistryValueKind.DWord);
                Debug.WriteLine($"Script Scanning set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Script Scanning: {ex.Message}", ex);
            }
        }

        public bool GetScriptScanning()
        {
            try
            {
                int value = GetRegistryValue(REALTIME_REG_PATH, "DisableScriptScanning", 0);
                return value == 0;
            }
            catch
            {
                return true;
            }
        }

        #endregion

        #region Cloud Protection

        public void SetCloudProtection(bool enabled)
        {
            RequireAdmin();
            try
            {
                int value = enabled ? 2 : 0;
                SetRegistryValue(SPYNET_REG_PATH, "SpynetReporting", value, RegistryValueKind.DWord);
                SetRegistryValue(POLICIES_REG_PATH + @"\Spynet", "SpynetReporting", value, RegistryValueKind.DWord);
                Debug.WriteLine($"Cloud Protection set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Cloud Protection: {ex.Message}", ex);
            }
        }

        public bool GetCloudProtection()
        {
            try
            {
                int value = GetRegistryValue(SPYNET_REG_PATH, "SpynetReporting", 2);
                return value > 0;
            }
            catch
            {
                return true;
            }
        }

        public void SetAutomaticSampleSubmission(bool enabled)
        {
            RequireAdmin();
            try
            {
                int value = enabled ? 1 : 2;
                SetRegistryValue(SPYNET_REG_PATH, "SubmitSamplesConsent", value, RegistryValueKind.DWord);
                SetRegistryValue(POLICIES_REG_PATH + @"\Spynet", "SubmitSamplesConsent", value, RegistryValueKind.DWord);
                Debug.WriteLine($"Automatic Sample Submission set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Automatic Sample Submission: {ex.Message}", ex);
            }
        }

        public bool GetAutomaticSampleSubmission()
        {
            try
            {
                int value = GetRegistryValue(SPYNET_REG_PATH, "SubmitSamplesConsent", 1);
                return value == 1;
            }
            catch
            {
                return true;
            }
        }

        public void SetCloudBlockLevel(int level)
        {
            RequireAdmin();
            try
            {
                // CloudBlockLevel: 0=Default, 2=Moderate, 4=High, 6=ZeroTolerance
                int[] blockLevels = { 0, 2, 4, 6, 6 };
                int blockLevel = blockLevels[Math.Min(level, 4)];

                SetRegistryValue(MPENGINE_REG_PATH, "MpCloudBlockLevel", blockLevel, RegistryValueKind.DWord);
                Debug.WriteLine($"Cloud Block Level set to: {blockLevel}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Cloud Block Level: {ex.Message}", ex);
            }
        }

        public int GetCloudBlockLevel()
        {
            try
            {
                int level = GetRegistryValue(MPENGINE_REG_PATH, "MpCloudBlockLevel", 0);
                switch (level)
                {
                    case 0: return 0; // Default
                    case 2: return 1; // Moderate
                    case 4: return 2; // High
                    case 6: return 3; // High+
                    default: return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Scanning Options

        public void SetArchiveScanning(bool enabled)
        {
            RequireAdmin();
            try
            {
                SetRegistryValue(DEFENDER_REG_PATH, "DisableArchiveScanning", enabled ? 0 : 1, RegistryValueKind.DWord);
                Debug.WriteLine($"Archive Scanning set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Archive Scanning: {ex.Message}", ex);
            }
        }

        public bool GetArchiveScanning()
        {
            try
            {
                int value = GetRegistryValue(DEFENDER_REG_PATH, "DisableArchiveScanning", 0);
                return value == 0;
            }
            catch
            {
                return true;
            }
        }

        public void SetEmailScanning(bool enabled)
        {
            RequireAdmin();
            try
            {
                SetRegistryValue(REALTIME_REG_PATH, "DisableEmailScanning", enabled ? 0 : 1, RegistryValueKind.DWord);
                Debug.WriteLine($"Email Scanning set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Email Scanning: {ex.Message}", ex);
            }
        }

        public bool GetEmailScanning()
        {
            try
            {
                int value = GetRegistryValue(REALTIME_REG_PATH, "DisableEmailScanning", 0);
                return value == 0;
            }
            catch
            {
                return true;
            }
        }

        public void SetRemovableDriveScanning(bool enabled)
        {
            RequireAdmin();
            try
            {
                SetRegistryValue(DEFENDER_REG_PATH, "DisableRemovableDriveScanning", enabled ? 0 : 1, RegistryValueKind.DWord);
                Debug.WriteLine($"Removable Drive Scanning set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Removable Drive Scanning: {ex.Message}", ex);
            }
        }

        public bool GetRemovableDriveScanning()
        {
            try
            {
                int value = GetRegistryValue(DEFENDER_REG_PATH, "DisableRemovableDriveScanning", 0);
                return value == 0;
            }
            catch
            {
                return true;
            }
        }

        public void SetNetworkScanning(bool enabled)
        {
            RequireAdmin();
            try
            {
                SetRegistryValue(REALTIME_REG_PATH, "DisableScanningNetworkFiles", enabled ? 0 : 1, RegistryValueKind.DWord);
                Debug.WriteLine($"Network Scanning set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Network Scanning: {ex.Message}", ex);
            }
        }

        public bool GetNetworkScanning()
        {
            try
            {
                int value = GetRegistryValue(REALTIME_REG_PATH, "DisableScanningNetworkFiles", 1);
                return value == 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Additional Protection

        public void SetControlledFolderAccess(bool enabled)
        {
            RequireAdmin();
            try
            {
                int value = enabled ? 1 : 0;
                SetRegistryValue(@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard\Controlled Folder Access", "EnableControlledFolderAccess", value, RegistryValueKind.DWord);
                Debug.WriteLine($"Controlled Folder Access set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Controlled Folder Access: {ex.Message}", ex);
            }
        }

        public bool GetControlledFolderAccess()
        {
            try
            {
                int value = GetRegistryValue(@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard\Controlled Folder Access", "EnableControlledFolderAccess", 0);
                return value == 1;
            }
            catch
            {
                return false;
            }
        }

        public void SetPUAProtection(bool enabled)
        {
            RequireAdmin();
            try
            {
                SetRegistryValue(MPENGINE_REG_PATH, "MpEnablePus", enabled ? 1 : 0, RegistryValueKind.DWord);
                SetRegistryValue(POLICIES_REG_PATH, "PUAProtection", enabled ? 1 : 0, RegistryValueKind.DWord);
                Debug.WriteLine($"PUA Protection set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set PUA Protection: {ex.Message}", ex);
            }
        }

        public bool GetPUAProtection()
        {
            try
            {
                int value = GetRegistryValue(MPENGINE_REG_PATH, "MpEnablePus", 1);
                return value == 1;
            }
            catch
            {
                return true;
            }
        }

        public void SetNetworkProtection(bool enabled)
        {
            RequireAdmin();
            try
            {
                int value = enabled ? 1 : 0;
                SetRegistryValue(@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard\Network Protection", "EnableNetworkProtection", value, RegistryValueKind.DWord);
                Debug.WriteLine($"Network Protection set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Network Protection: {ex.Message}", ex);
            }
        }

        public bool GetNetworkProtection()
        {
            try
            {
                int value = GetRegistryValue(@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard\Network Protection", "EnableNetworkProtection", 0);
                return value == 1;
            }
            catch
            {
                return false;
            }
        }

        public void SetExploitProtection(bool enabled)
        {
            RequireAdmin();
            try
            {
                // Exploit Protection is managed through Windows Security
                // We can't easily toggle it via registry, so we'll note it's enabled by default
                Debug.WriteLine($"Exploit Protection configuration noted (managed by Windows Security)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Warning: Exploit Protection configuration limited: {ex.Message}");
            }
        }

        public bool GetExploitProtection()
        {
            return true; // Always enabled by default in Windows 11
        }

        #endregion

        #region Windows Security Features

        public void SetSmartScreen(bool enabled)
        {
            RequireAdmin();
            try
            {
                string value = enabled ? "Warn" : "Off";
                SetRegistryValue(SMARTSCREEN_REG_PATH, "SmartScreenEnabled", value, RegistryValueKind.String);

                string policyPath = @"SOFTWARE\Policies\Microsoft\Windows\System";
                SetRegistryValue(policyPath, "EnableSmartScreen", enabled ? 1 : 0, RegistryValueKind.DWord);

                Debug.WriteLine($"SmartScreen set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set SmartScreen: {ex.Message}", ex);
            }
        }

        public bool GetSmartScreen()
        {
            try
            {
                object value = GetRegistryValueObject(SMARTSCREEN_REG_PATH, "SmartScreenEnabled");
                if (value != null)
                {
                    string strValue = value.ToString();
                    return strValue != "Off";
                }
            }
            catch { }
            return true;
        }

        public void SetSmartScreenApps(bool enabled)
        {
            RequireAdmin();
            try
            {
                SetRegistryValue(APPHOST_REG_PATH, "EnableWebContentEvaluation", enabled ? 1 : 0, RegistryValueKind.DWord);
                SetRegistryValue(APPHOST_REG_PATH, "PreventOverride", enabled ? 1 : 0, RegistryValueKind.DWord);
                Debug.WriteLine($"SmartScreen for Apps set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set SmartScreen for Apps: {ex.Message}", ex);
            }
        }

        public bool GetSmartScreenApps()
        {
            try
            {
                int value = GetRegistryValue(APPHOST_REG_PATH, "EnableWebContentEvaluation", 1);
                return value == 1;
            }
            catch
            {
                return true;
            }
        }

        public void SetTamperProtection(bool enabled)
        {
            RequireAdmin();
            try
            {
                int value = enabled ? 5 : 0;
                SetRegistryValue(FEATURES_REG_PATH, "TamperProtection", value, RegistryValueKind.DWord);
                Debug.WriteLine($"Tamper Protection set to: {enabled}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set Tamper Protection: {ex.Message}", ex);
            }
        }

        public bool GetTamperProtection()
        {
            try
            {
                int value = GetRegistryValue(FEATURES_REG_PATH, "TamperProtection", 5);
                return value == 5 || value == 1;
            }
            catch
            {
                return true;
            }
        }

        #endregion

        #region Exclusions Management

        public void AddFileExclusion(string path)
        {
            RequireAdmin();
            try
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                    throw new FileNotFoundException($"Path not found: {path}");

                SetRegistryValue(EXCLUSIONS_PATHS_REG, path, 0, RegistryValueKind.DWord);
                Debug.WriteLine($"Added file/folder exclusion: {path}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add file exclusion: {ex.Message}", ex);
            }
        }

        public void RemoveFileExclusion(string path)
        {
            RequireAdmin();
            try
            {
                DeleteRegistryValue(EXCLUSIONS_PATHS_REG, path);
                Debug.WriteLine($"Removed file/folder exclusion: {path}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove file exclusion: {ex.Message}", ex);
            }
        }

        public void AddFolderExclusion(string path)
        {
            // Same as AddFileExclusion
            AddFileExclusion(path);
        }

        public void AddExtensionExclusion(string extension)
        {
            RequireAdmin();
            try
            {
                if (!extension.StartsWith("."))
                    extension = "." + extension;

                SetRegistryValue(EXCLUSIONS_EXT_REG, extension, 0, RegistryValueKind.DWord);
                Debug.WriteLine($"Added extension exclusion: {extension}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add extension exclusion: {ex.Message}", ex);
            }
        }

        public void RemoveExtensionExclusion(string extension)
        {
            RequireAdmin();
            try
            {
                if (!extension.StartsWith("."))
                    extension = "." + extension;

                DeleteRegistryValue(EXCLUSIONS_EXT_REG, extension);
                Debug.WriteLine($"Removed extension exclusion: {extension}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove extension exclusion: {ex.Message}", ex);
            }
        }

        public void AddProcessExclusion(string processName)
        {
            RequireAdmin();
            try
            {
                SetRegistryValue(EXCLUSIONS_PROC_REG, processName, 0, RegistryValueKind.DWord);
                Debug.WriteLine($"Added process exclusion: {processName}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add process exclusion: {ex.Message}", ex);
            }
        }

        public void RemoveProcessExclusion(string processName)
        {
            RequireAdmin();
            try
            {
                DeleteRegistryValue(EXCLUSIONS_PROC_REG, processName);
                Debug.WriteLine($"Removed process exclusion: {processName}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove process exclusion: {ex.Message}", ex);
            }
        }

        public List<string> GetFileExclusions()
        {
            try
            {
                return GetRegistryValueNames(EXCLUSIONS_PATHS_REG);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting file exclusions: {ex.Message}");
                return new List<string>();
            }
        }

        public List<string> GetExtensionExclusions()
        {
            try
            {
                return GetRegistryValueNames(EXCLUSIONS_EXT_REG);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting extension exclusions: {ex.Message}");
                return new List<string>();
            }
        }

        public List<string> GetProcessExclusions()
        {
            try
            {
                return GetRegistryValueNames(EXCLUSIONS_PROC_REG);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting process exclusions: {ex.Message}");
                return new List<string>();
            }
        }

        #endregion

        #region Preset Configurations

        public void ApplyDefaultPreset()
        {
            RequireAdmin();
            try
            {
                SetRealTimeProtection(true);
                SetBehaviorMonitoring(true);
                SetOnAccessProtection(true);
                SetScriptScanning(true);

                SetCloudProtection(true);
                SetAutomaticSampleSubmission(true);
                SetCloudBlockLevel(0);

                SetArchiveScanning(true);
                SetEmailScanning(true);
                SetRemovableDriveScanning(true);
                SetNetworkScanning(false);

                SetControlledFolderAccess(false);
                SetPUAProtection(true);
                SetNetworkProtection(true);

                SetSmartScreen(true);
                SetSmartScreenApps(true);
                SetTamperProtection(true);

                Debug.WriteLine("Applied Default preset");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to apply Default preset: {ex.Message}", ex);
            }
        }

        public void ApplyGamingPreset()
        {
            RequireAdmin();
            try
            {
                SetRealTimeProtection(true);
                SetBehaviorMonitoring(true);
                SetOnAccessProtection(true);
                SetScriptScanning(false);

                SetCloudProtection(true);
                SetAutomaticSampleSubmission(false);
                SetCloudBlockLevel(1);

                SetArchiveScanning(false);
                SetEmailScanning(false);
                SetRemovableDriveScanning(false);
                SetNetworkScanning(false);

                SetControlledFolderAccess(false);
                SetPUAProtection(false);
                SetNetworkProtection(true);

                SetSmartScreen(true);
                SetSmartScreenApps(false);
                SetTamperProtection(true);

                Debug.WriteLine("Applied Gaming preset");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to apply Gaming preset: {ex.Message}", ex);
            }
        }

        public void ApplyMinimalPreset()
        {
            RequireAdmin();
            try
            {
                SetRealTimeProtection(false);
                SetBehaviorMonitoring(false);
                SetOnAccessProtection(false);
                SetScriptScanning(false);

                SetCloudProtection(false);
                SetAutomaticSampleSubmission(false);
                SetCloudBlockLevel(0);

                SetArchiveScanning(false);
                SetEmailScanning(false);
                SetRemovableDriveScanning(false);
                SetNetworkScanning(false);

                SetControlledFolderAccess(false);
                SetPUAProtection(false);
                SetNetworkProtection(false);

                SetSmartScreen(false);
                SetSmartScreenApps(false);
                SetTamperProtection(true); // Keep for safety

                Debug.WriteLine("Applied Minimal preset");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to apply Minimal preset: {ex.Message}", ex);
            }
        }

        #endregion

        #region Service Management

        public void DisableDefenderService()
        {
            RequireAdmin();
            try
            {
                ExecuteCommand("sc config WinDefend start=disabled");
                ExecuteCommand("sc stop WinDefend");
                ExecuteCommand("sc config SecurityHealthService start=disabled");
                ExecuteCommand("sc stop SecurityHealthService");

                // Also disable via registry
                SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\WinDefend", "Start", 4, RegistryValueKind.DWord);
                SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\SecurityHealthService", "Start", 4, RegistryValueKind.DWord);

                Debug.WriteLine("Disabled Defender services");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to disable Defender service: {ex.Message}", ex);
            }
        }

        public void EnableDefenderService()
        {
            RequireAdmin();
            try
            {
                ExecuteCommand("sc config WinDefend start=auto");
                ExecuteCommand("sc start WinDefend");
                ExecuteCommand("sc config SecurityHealthService start=auto");
                ExecuteCommand("sc start SecurityHealthService");

                SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\WinDefend", "Start", 2, RegistryValueKind.DWord);
                SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\SecurityHealthService", "Start", 2, RegistryValueKind.DWord);

                Debug.WriteLine("Enabled Defender services");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to enable Defender service: {ex.Message}", ex);
            }
        }

        public bool IsDefenderServiceRunning()
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "sc";
                process.StartInfo.Arguments = "query WinDefend";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains("RUNNING");
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Scanning Operations

        public void StartQuickScan()
        {
            RequireAdmin();
            try
            {
                string defenderPath = @"C:\Program Files\Windows Defender\MpCmdRun.exe";
                if (File.Exists(defenderPath))
                {
                    ExecuteCommand($"\"{defenderPath}\" -Scan -ScanType 1");
                    Debug.WriteLine("Started Quick Scan");
                }
                else
                {
                    throw new FileNotFoundException("Windows Defender executable not found");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to start Quick Scan: {ex.Message}", ex);
            }
        }

        public void StartFullScan()
        {
            RequireAdmin();
            try
            {
                string defenderPath = @"C:\Program Files\Windows Defender\MpCmdRun.exe";
                if (File.Exists(defenderPath))
                {
                    ExecuteCommand($"\"{defenderPath}\" -Scan -ScanType 2");
                    Debug.WriteLine("Started Full Scan");
                }
                else
                {
                    throw new FileNotFoundException("Windows Defender executable not found");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to start Full Scan: {ex.Message}", ex);
            }
        }

        public void StartCustomScan(string path)
        {
            RequireAdmin();
            try
            {
                string defenderPath = @"C:\Program Files\Windows Defender\MpCmdRun.exe";
                if (File.Exists(defenderPath))
                {
                    ExecuteCommand($"\"{defenderPath}\" -Scan -ScanType 3 -File \"{path}\"");
                    Debug.WriteLine($"Started Custom Scan on: {path}");
                }
                else
                {
                    throw new FileNotFoundException("Windows Defender executable not found");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to start Custom Scan: {ex.Message}", ex);
            }
        }

        public void UpdateDefinitions()
        {
            RequireAdmin();
            try
            {
                string defenderPath = @"C:\Program Files\Windows Defender\MpCmdRun.exe";
                if (File.Exists(defenderPath))
                {
                    ExecuteCommand($"\"{defenderPath}\" -SignatureUpdate");
                    Debug.WriteLine("Updated Defender definitions");
                }
                else
                {
                    throw new FileNotFoundException("Windows Defender executable not found");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update definitions: {ex.Message}", ex);
            }
        }

        #endregion

        #region Helper Methods

        private void SetRegistryValue(string path, string name, object value, RegistryValueKind kind)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(path, true))
                {
                    if (key != null)
                    {
                        key.SetValue(name, value, kind);
                        Debug.WriteLine($"Set registry: HKLM\\{path}\\{name} = {value}");
                    }
                    else
                    {
                        throw new InvalidOperationException($"Failed to create/open registry key: {path}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"Access denied to registry path: HKLM\\{path}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set registry value: {ex.Message}", ex);
            }
        }

        private int GetRegistryValue(string path, string name, int defaultValue)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path, false))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(name);
                        if (value != null)
                        {
                            return Convert.ToInt32(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading registry HKLM\\{path}\\{name}: {ex.Message}");
            }
            return defaultValue;
        }

        private object GetRegistryValueObject(string path, string name)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path, false))
                {
                    if (key != null)
                    {
                        return key.GetValue(name);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading registry HKLM\\{path}\\{name}: {ex.Message}");
            }
            return null;
        }

        private void DeleteRegistryValue(string path, string name)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(name, false);
                        Debug.WriteLine($"Deleted registry value: HKLM\\{path}\\{name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting registry value: {ex.Message}");
                throw;
            }
        }

        private List<string> GetRegistryValueNames(string path)
        {
            List<string> values = new List<string>();
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path, false))
                {
                    if (key != null)
                    {
                        string[] valueNames = key.GetValueNames();
                        values.AddRange(valueNames);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading registry values from HKLM\\{path}: {ex.Message}");
            }
            return values;
        }

        private string ExecuteCommand(string command)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + command;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.Verb = "runas";

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine($"Command error: {error}");
                }

                Debug.WriteLine($"Executed command: {command}");
                return output;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to execute command '{command}': {ex.Message}");
                throw new InvalidOperationException($"Command execution failed: {ex.Message}", ex);
            }
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            // No resources to dispose
        }

        #endregion
    }

    #region Helper Classes

    public class DefenderThreat
    {
        public string ThreatName { get; set; }
        public string Severity { get; set; }
        public string[] Resources { get; set; }
        public string DomainUser { get; set; }
        public string ProcessName { get; set; }
    }

    #endregion
}
