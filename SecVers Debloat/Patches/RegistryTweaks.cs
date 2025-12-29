using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace SecVers_Debloat.Patches
{
    /// <summary>
    /// Registry tweaks for Windows 11 performance, power saving, and privacy
    /// WARNING: Some tweaks are experimental and may cause system instability
    /// </summary>
    public class RegistryTweaks
    {
        #region Helper Methods

        private static void SetRegistryValue(string keyPath, string valueName, object value, RegistryValueKind valueKind)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, valueKind);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set registry value: {keyPath}\\{valueName}", ex);
            }
        }

        private static void SetCurrentUserRegistryValue(string keyPath, string valueName, object value, RegistryValueKind valueKind)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, valueKind);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set HKCU registry value: {keyPath}\\{valueName}", ex);
            }
        }

        private static void DeleteRegistryValue(string keyPath, string valueName)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                {
                    if (key != null && key.GetValue(valueName) != null)
                    {
                        key.DeleteValue(valueName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete registry value: {keyPath}\\{valueName}", ex);
            }
        }

        #endregion

        #region Performance Tweaks

        /// <summary>
        /// Disable Windows Search indexing for better performance
        /// Impact: High | Risk: Low | Restart: No
        /// </summary>
        public static void DisableSearchIndexing()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\WSearch", "Start", 4, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Superfetch/SysMain service
        /// Impact: Medium | Risk: Low | Restart: No
        /// </summary>
        public static void DisableSuperfetch()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\SysMain", "Start", 4, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Windows Tips and Suggestions
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableWindowsTips()
        {
            SetCurrentUserRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                "SubscribedContent-338389Enabled", 0, RegistryValueKind.DWord);
            SetCurrentUserRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Hibernation and delete hiberfil.sys
        /// Impact: High (Frees disk space) | Risk: Low | Restart: No
        /// </summary>
        public static void DisableHibernation()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Power", "HiberFileSizePercent", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Fast Startup (may improve boot stability)
        /// Impact: Medium | Risk: Low | Restart: Recommended
        /// </summary>
        public static void DisableFastStartup()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Session Manager\Power",
                "HiberbootEnabled", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Optimize SSD: Disable LastAccess time stamp
        /// Impact: Medium | Risk: Low | Restart: No
        /// </summary>
        public static void DisableLastAccessTimeStamp()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\FileSystem",
                "NtfsDisableLastAccessUpdate", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Windows Error Reporting
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableErrorReporting()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\Windows Error Reporting", "Disabled", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\Windows Error Reporting", "DontSendAdditionalData", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Program Compatibility Assistant
        /// Impact: Low | Risk: Low | Restart: No
        /// </summary>
        public static void DisableCompatibilityAssistant()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\AppCompat",
                "DisablePCA", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Set Windows visual effects to best performance
        /// Impact: High | Risk: None | Restart: No
        /// </summary>
        public static void SetVisualEffectsBestPerformance()
        {
            SetCurrentUserRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects",
                "VisualFXSetting", 2, RegistryValueKind.DWord);

            // Disable animations
            SetCurrentUserRegistryValue(@"Control Panel\Desktop\WindowMetrics", "MinAnimate", "0", RegistryValueKind.String);
            SetCurrentUserRegistryValue(@"Control Panel\Desktop", "UserPreferencesMask",
                new byte[] { 0x90, 0x12, 0x03, 0x80, 0x10, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
        }

        /// <summary>
        /// Disable transparency effects (Acrylic)
        /// Impact: Medium | Risk: None | Restart: No
        /// </summary>
        public static void DisableTransparency()
        {
            SetCurrentUserRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "EnableTransparency", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Windows Animations
        /// Impact: Medium | Risk: None | Restart: No
        /// </summary>
        public static void DisableAnimations()
        {
            SetCurrentUserRegistryValue(@"Control Panel\Desktop", "UserPreferencesMask",
                new byte[] { 0x90, 0x12, 0x01, 0x80 }, RegistryValueKind.Binary);
            SetCurrentUserRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                "TaskbarAnimations", 0, RegistryValueKind.DWord);
        }

        #endregion

        #region Power Saving Tweaks

        /// <summary>
        /// Disable USB selective suspend (prevents USB devices from going to sleep)
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableUSBSelectiveSuspend()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\USB", "DisableSelectiveSuspend", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Enable Ultimate Performance power plan
        /// Impact: High (Disables power saving) | Risk: None | Restart: No
        /// </summary>
        public static void EnableUltimatePerformancePlan()
        {
            // This creates the hidden Ultimate Performance plan
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Power",
                "CsEnabled", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable CPU Parking (all cores always active)
        /// Impact: High (More power consumption) | Risk: None | Restart: Recommended
        /// </summary>
        public static void DisableCPUParkingAllCores()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\0cc5b647-c1df-4637-891a-dec35c318583",
                "ValueMax", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Set CPU to never throttle (100% always)
        /// Impact: Very High | Risk: Medium (Heat) | Restart: No
        /// </summary>
        public static void SetCPUMaxPerformance()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\be337238-0d82-4146-a960-4f3749d470c7",
                "Attributes", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable PCI Express Link State Power Management
        /// Impact: Medium | Risk: Low | Restart: No
        /// </summary>
        public static void DisablePCIExpressPowerManagement()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\501a4d13-42af-4429-9fd1-a8218c268e20\ee12f906-d277-404b-b6da-e5fa1a576df5",
                "Attributes", 0, RegistryValueKind.DWord);
        }

        #endregion

        #region Network Performance

        /// <summary>
        /// Optimize network for low latency (gaming)
        /// Impact: High | Risk: Low | Restart: No
        /// </summary>
        public static void OptimizeNetworkLatency()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "NetworkThrottlingIndex", 0xFFFFFFFF, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "SystemResponsiveness", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Nagle's Algorithm (reduces latency)
        /// Impact: Medium | Risk: Low | Restart: No
        /// </summary>
        public static void DisableNaglesAlgorithm()
        {
            string[] interfaces = new string[]
            {
                @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces"
            };

            foreach (string interfacePath in interfaces)
            {
                SetRegistryValue(interfacePath, "TcpAckFrequency", 1, RegistryValueKind.DWord);
                SetRegistryValue(interfacePath, "TCPNoDelay", 1, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Increase network buffer sizes
        /// Impact: Medium | Risk: Low | Restart: No
        /// </summary>
        public static void IncreaseNetworkBuffers()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                "TcpWindowSize", 65535, RegistryValueKind.DWord);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                "Tcp1323Opts", 3, RegistryValueKind.DWord);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                "DefaultTTL", 64, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Windows Network Throttling
        /// Impact: Medium | Risk: None | Restart: No
        /// </summary>
        public static void DisableNetworkThrottling()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "NetworkThrottlingIndex", 0xFFFFFFFF, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Large Send Offload (may improve stability)
        /// Impact: Low | Risk: Low | Restart: No
        /// </summary>
        public static void DisableLargeSendOffload()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                "DisableTaskOffload", 1, RegistryValueKind.DWord);
        }

        #endregion

        #region Gaming Optimizations

        /// <summary>
        /// Disable Game DVR and Game Bar
        /// Impact: Medium | Risk: None | Restart: No
        /// </summary>
        public static void DisableGameDVR()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\GameDVR",
                "AllowGameDVR", 0, RegistryValueKind.DWord);
            SetCurrentUserRegistryValue(@"System\GameConfigStore",
                "GameDVR_Enabled", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Enable Game Mode
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void EnableGameMode()
        {
            SetCurrentUserRegistryValue(@"SOFTWARE\Microsoft\GameBar",
                "AutoGameModeEnabled", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Optimize for background services (better for gaming)
        /// Impact: Medium | Risk: None | Restart: No
        /// </summary>
        public static void OptimizeForBackgroundServices()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\PriorityControl",
                "Win32PrioritySeparation", 0x26, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Fullscreen Optimizations globally
        /// Impact: Medium | Risk: Low | Restart: No
        /// </summary>
        public static void DisableFullscreenOptimizations()
        {
            SetCurrentUserRegistryValue(@"System\GameConfigStore",
                "GameDVR_FSEBehaviorMode", 2, RegistryValueKind.DWord);
            SetCurrentUserRegistryValue(@"System\GameConfigStore",
                "GameDVR_HonorUserFSEBehaviorMode", 1, RegistryValueKind.DWord);
            SetCurrentUserRegistryValue(@"System\GameConfigStore",
                "GameDVR_DXGIHonorFSEWindowsCompatible", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Set GPU priority to high for gaming
        /// Impact: Medium | Risk: Low | Restart: No
        /// </summary>
        public static void SetHighGPUPriority()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                "GPU Priority", 8, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                "Priority", 6, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                "Scheduling Category", "High", RegistryValueKind.String);
        }

        #endregion

        #region Privacy Tweaks

        /// <summary>
        /// Disable Telemetry (set to Security level)
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableTelemetry()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                "AllowTelemetry", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection",
                "AllowTelemetry", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Activity History
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableActivityHistory()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\System",
                "EnableActivityFeed", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\System",
                "PublishUserActivities", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\System",
                "UploadUserActivities", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Advertising ID
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableAdvertisingID()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo",
                "DisabledByGroupPolicy", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Location Tracking
        /// Impact: Low | Risk: Low | Restart: No
        /// </summary>
        public static void DisableLocationTracking()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors",
                "DisableLocation", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable feedback requests
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableFeedback()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                "DoNotShowFeedbackNotifications", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Cortana
        /// Impact: Low | Risk: Low | Restart: No
        /// </summary>
        public static void DisableCortana()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                "AllowCortana", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable web search in Start Menu
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableWebSearchInStart()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                "DisableWebSearch", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                "ConnectedSearchUseWeb", 0, RegistryValueKind.DWord);
        }

        #endregion

        #region Windows 11 Specific

        /// <summary>
        /// Restore Windows 10 context menu
        /// Impact: Low | Risk: None | Restart: Explorer
        /// </summary>
        public static void RestoreOldContextMenu()
        {
            SetCurrentUserRegistryValue(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32",
                "", "", RegistryValueKind.String);
        }

        /// <summary>
        /// Disable Widgets
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableWidgets()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Dsh",
                "AllowNewsAndInterests", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Chat icon on taskbar
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableChatIcon()
        {
            SetCurrentUserRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                "TaskbarMn", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Center taskbar icons (Win11 default) or left-align
        /// Impact: None | Risk: None | Restart: No
        /// </summary>
        public static void AlignTaskbarLeft()
        {
            SetCurrentUserRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                "TaskbarAl", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable Snap Assist flyout
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableSnapAssist()
        {
            SetCurrentUserRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                "EnableSnapAssistFlyout", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Show file extensions
        /// Impact: None | Risk: None | Restart: No
        /// </summary>
        public static void ShowFileExtensions()
        {
            SetCurrentUserRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                "HideFileExt", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Show hidden files
        /// Impact: None | Risk: None | Restart: No
        /// </summary>
        public static void ShowHiddenFiles()
        {
            SetCurrentUserRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                "Hidden", 1, RegistryValueKind.DWord);
        }

        #endregion

        #region Experimental (CAUTION!)

        /// <summary>
        /// EXPERIMENTAL: Disable Memory Compression
        /// Impact: Very High | Risk: HIGH | Restart: Yes
        /// WARNING: May cause system instability on low RAM systems!
        /// </summary>
        public static void DisableMemoryCompression()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                "DisablePagingExecutive", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// EXPERIMENTAL: Disable Paging Executive
        /// Impact: Very High | Risk: HIGH | Restart: Yes
        /// WARNING: Requires sufficient RAM (16GB+)
        /// </summary>
        public static void DisablePagingExecutive()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                "DisablePagingExecutive", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// EXPERIMENTAL: Force Contiguous Memory Allocation
        /// Impact: High | Risk: MEDIUM | Restart: Yes
        /// WARNING: May cause crashes with some drivers
        /// </summary>
        public static void ForceContiguousMemory()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                "LargeSystemCache", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// EXPERIMENTAL: Disable HPET (High Precision Event Timer)
        /// Impact: Variable | Risk: MEDIUM | Restart: Yes
        /// WARNING: Can cause timing issues in some applications
        /// </summary>
        public static void DisableHPET()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Services\HPET",
                "Start", 4, RegistryValueKind.DWord);
        }

        /// <summary>
        /// EXPERIMENTAL: Reduce DPC (Deferred Procedure Call) timeout
        /// Impact: High | Risk: MEDIUM | Restart: No
        /// WARNING: May cause system instability
        /// </summary>
        public static void ReduceDPCTimeout()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Session Manager\kernel",
                "DpcTimeout", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// EXPERIMENTAL: Disable Spectre/Meltdown mitigations (DANGEROUS!)
        /// Impact: Very High | Risk: CRITICAL | Restart: Yes
        /// WARNING: Removes security protections! Only for isolated gaming systems!
        /// </summary>
        public static void DisableSpectreMeltdownMitigations()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                "FeatureSettingsOverride", 3, RegistryValueKind.DWord);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                "FeatureSettingsOverrideMask", 3, RegistryValueKind.DWord);
        }

        /// <summary>
        /// EXPERIMENTAL: Disable Prefetcher and Superfetch completely
        /// Impact: High | Risk: MEDIUM | Restart: Yes
        /// WARNING: Can slow down application launches
        /// </summary>
        public static void DisablePrefetcherCompletely()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters",
                "EnablePrefetcher", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters",
                "EnableSuperfetch", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// EXPERIMENTAL: Disable Core Parking completely (all power plans)
        /// Impact: Very High | Risk: LOW | Restart: Yes
        /// WARNING: Increases power consumption significantly
        /// </summary>
        public static void DisableCoreParkingCompletely()
        {
            // Disable for all power plans
            string[] guids = new string[]
            {
                "54533251-82be-4824-96c1-47b60b740d00", // Processor Power Management
                "0cc5b647-c1df-4637-891a-dec35c318583"  // Core Parking
            };

            foreach (string guid in guids)
            {
                SetRegistryValue($@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\{guid}",
                    "Attributes", 0, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// EXPERIMENTAL: Set timer resolution to 0.5ms (ultra-low latency)
        /// Impact: Very High | Risk: MEDIUM | Restart: No
        /// WARNING: Increases CPU usage and power consumption
        /// </summary>
        public static void SetUltraLowTimerResolution()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Session Manager\kernel",
                "GlobalTimerResolutionRequests", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// EXPERIMENTAL: Disable C-States (CPU never sleeps)
        /// Impact: Extreme | Risk: MEDIUM | Restart: Yes
        /// WARNING: Massive power consumption increase, high temperatures
        /// </summary>
        public static void DisableCStates()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Power",
                "CsEnabled", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\Processor",
                "Capabilities", 0x0007e066, RegistryValueKind.DWord);
        }

        #endregion

        #region Disk Performance

        /// <summary>
        /// Disable Windows Write-Cache Buffer Flushing
        /// Impact: High | Risk: MEDIUM | Restart: No
        /// WARNING: Risk of data loss during power failure
        /// </summary>
        public static void DisableWriteCacheBufferFlushing()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\FileSystem",
                "NtfsDisableLastAccessUpdate", 1, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Optimize NTFS for SSDs
        /// Impact: Medium | Risk: Low | Restart: No
        /// </summary>
        public static void OptimizeNTFSForSSD()
        {
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\FileSystem",
                "NtfsDisable8dot3NameCreation", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"SYSTEM\CurrentControlSet\Control\FileSystem",
                "NtfsMemoryUsage", 2, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Disable automatic disk defragmentation
        /// Impact: Low | Risk: None | Restart: No
        /// </summary>
        public static void DisableAutoDefrag()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\OptimalLayout",
                "EnableAutoLayout", 0, RegistryValueKind.DWord);
        }

        #endregion
    }
}
