using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace SecVers_Debloat.Patches
{
    /// <summary>
    /// Main Debloat Engine for Windows 11
    /// Based on Atlas Playbook and community best practices
    /// NO PowerShell dependency - uses native Win32 APIs and CMD
    /// </summary>
    public class DebloatEngine
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SecVers_Debloat",
            "debloat.log"
        );

        public event EventHandler<DebloatProgressEventArgs> ProgressChanged;
        public event EventHandler<DebloatLogEventArgs> LogMessage;

        private void Log(string message, LogLevel level = LogLevel.Info)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            LogMessage?.Invoke(this, new DebloatLogEventArgs(message, level));

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
                File.AppendAllText(LogPath, logEntry + Environment.NewLine);
            }
            catch { }
        }

        private void UpdateProgress(int percentage, string status)
        {
            ProgressChanged?.Invoke(this, new DebloatProgressEventArgs(percentage, status));
        }

        private string ExecuteCommand(string command, string arguments = "", bool asAdmin = true)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = asAdmin ? "runas" : ""
                };

                using (var process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        return string.IsNullOrEmpty(error) ? output : error;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Command execution failed: {ex.Message}", LogLevel.Error);
            }

            return string.Empty;
        }

        #region UWP Apps Removal

        /// <summary>
        /// Category: UWP Apps
        /// Removes built-in Windows 11 UWP applications
        /// </summary>
        public class UWPApps
        {
            private readonly DebloatEngine _engine;

            public UWPApps(DebloatEngine engine)
            {
                _engine = engine;
            }

            public List<AppPackageInfo> GetRemovableApps()
            {
                return new List<AppPackageInfo>
                {
                    // Communication & Social
                    new AppPackageInfo("Microsoft.YourPhone", "Phone Link", "Link your Android/iOS phone", AppCategory.Communication),
                    new AppPackageInfo("Microsoft.People", "People", "Contacts app", AppCategory.Communication),
                    new AppPackageInfo("microsoft.windowscommunicationsapps", "Mail & Calendar", "Built-in email client", AppCategory.Communication),
                    new AppPackageInfo("Microsoft.OutlookForWindows", "Outlook (new)", "New Outlook app", AppCategory.Communication),
                    
                    // Entertainment & Gaming
                    new AppPackageInfo("Microsoft.XboxApp", "Xbox Console Companion", "Legacy Xbox app", AppCategory.Gaming),
                    new AppPackageInfo("Microsoft.XboxGamingOverlay", "Xbox Game Bar", "Gaming overlay (Win+G)", AppCategory.Gaming),
                    new AppPackageInfo("Microsoft.XboxGameOverlay", "Xbox Game Overlay", "In-game overlay", AppCategory.Gaming),
                    new AppPackageInfo("Microsoft.XboxIdentityProvider", "Xbox Identity Provider", "Xbox authentication", AppCategory.Gaming),
                    new AppPackageInfo("Microsoft.XboxSpeechToTextOverlay", "Xbox Speech to Text", "Voice chat transcription", AppCategory.Gaming),
                    new AppPackageInfo("Microsoft.Xbox.TCUI", "Xbox TCUI", "Xbox UI component", AppCategory.Gaming),
                    new AppPackageInfo("Microsoft.GamingApp", "Xbox (new)", "New Xbox app", AppCategory.Gaming),
                    new AppPackageInfo("Microsoft.ZuneMusic", "Groove Music", "Legacy music player", AppCategory.Entertainment),
                    new AppPackageInfo("Microsoft.ZuneVideo", "Movies & TV", "Video player", AppCategory.Entertainment),
                    new AppPackageInfo("Microsoft.BingNews", "News", "News aggregator", AppCategory.Entertainment),
                    new AppPackageInfo("Microsoft.BingWeather", "Weather", "Weather app", AppCategory.Entertainment),
                    new AppPackageInfo("SpotifyAB.SpotifyMusic", "Spotify", "Pre-installed Spotify", AppCategory.Entertainment),
                    
                    // Productivity (Bloat)
                    new AppPackageInfo("Microsoft.Todos", "Microsoft To Do", "Task management", AppCategory.Productivity),
                    new AppPackageInfo("Microsoft.PowerAutomateDesktop", "Power Automate", "Automation tool", AppCategory.Productivity),
                    new AppPackageInfo("Microsoft.Office.OneNote", "OneNote", "Note-taking app", AppCategory.Productivity),
                    new AppPackageInfo("Microsoft.MicrosoftOfficeHub", "Office Hub", "Office launcher", AppCategory.Productivity),
                    new AppPackageInfo("Microsoft.MicrosoftStickyNotes", "Sticky Notes", "Desktop notes", AppCategory.Productivity),
                    
                    // Media & Graphics
                    new AppPackageInfo("Microsoft.ScreenSketch", "Snipping Tool", "Screenshot tool", AppCategory.Media),
                    new AppPackageInfo("Microsoft.Paint", "Paint", "Classic Paint app", AppCategory.Media),
                    new AppPackageInfo("Microsoft.MSPaint", "Paint 3D", "3D modeling tool", AppCategory.Media),
                    new AppPackageInfo("Microsoft.Windows.Photos", "Photos", "Photo viewer/editor", AppCategory.Media),
                    new AppPackageInfo("Microsoft.WindowsCamera", "Camera", "Webcam app", AppCategory.Media),
                    new AppPackageInfo("Microsoft.WindowsSoundRecorder", "Sound Recorder", "Audio recording", AppCategory.Media),
                    new AppPackageInfo("Clipchamp.Clipchamp", "Clipchamp", "Video editor", AppCategory.Media),
                    
                    // Mixed Reality & 3D
                    new AppPackageInfo("Microsoft.MixedReality.Portal", "Mixed Reality Portal", "VR/AR interface", AppCategory.MixedReality),
                    new AppPackageInfo("Microsoft.Microsoft3DViewer", "3D Viewer", "3D model viewer", AppCategory.MixedReality),
                    
                    // Maps & Navigation
                    new AppPackageInfo("Microsoft.WindowsMaps", "Maps", "Navigation app", AppCategory.Navigation),
                    
                    // Feedback & Telemetry
                    new AppPackageInfo("Microsoft.WindowsFeedbackHub", "Feedback Hub", "Send feedback to Microsoft", AppCategory.Telemetry),
                    new AppPackageInfo("Microsoft.GetHelp", "Get Help", "Support app", AppCategory.Telemetry),
                    new AppPackageInfo("Microsoft.Getstarted", "Tips", "Windows tips", AppCategory.Telemetry),
                    
                    // Store & Shopping
                    new AppPackageInfo("Microsoft.WindowsStore", "Microsoft Store", "App store (CAUTION: breaks some features)", AppCategory.Store),
                    
                    // Others
                    new AppPackageInfo("Microsoft.WindowsAlarms", "Alarms & Clock", "Timer/alarm app", AppCategory.Utilities),
                    new AppPackageInfo("Microsoft.windowscalculator", "Calculator", "Calculator app", AppCategory.Utilities),
                    new AppPackageInfo("Microsoft.Windows.DevHome", "Dev Home", "Developer tools hub", AppCategory.Development),
                    new AppPackageInfo("MicrosoftCorporationII.QuickAssist", "Quick Assist", "Remote assistance", AppCategory.Utilities),
                    new AppPackageInfo("MicrosoftWindows.Client.WebExperience", "Widgets", "Taskbar widgets", AppCategory.Widgets),
                    new AppPackageInfo("MicrosoftTeams", "Microsoft Teams", "Chat/collaboration", AppCategory.Communication),
                    new AppPackageInfo("Microsoft.549981C3F5F10", "Cortana", "Voice assistant", AppCategory.Assistant),
                    new AppPackageInfo("Microsoft.BingSearch", "Bing Search", "Search integration", AppCategory.Search),
                    new AppPackageInfo("Microsoft.OneDriveSync", "OneDrive", "Cloud storage sync", AppCategory.Cloud),
                    // Füge diese Apps zur GetRemovableApps() hinzu:

                    // Additional Apps - Security & System
                    new AppPackageInfo("Microsoft.SecHealthUI", "Windows Security", "Security Center UI", AppCategory.Security),
                    new AppPackageInfo("Microsoft.Windows.SecHealthUI", "Windows Security (Alt)", "Alternative Security UI", AppCategory.Security),
                    new AppPackageInfo("Windows.CBSPreview", "CBS Preview", "Component Based Servicing", AppCategory.System),

                    // Additional Productivity & Office
                    new AppPackageInfo("Microsoft.Office.Desktop", "Office Desktop", "Office integration", AppCategory.Productivity),
                    new AppPackageInfo("Microsoft.SkypeApp", "Skype", "Video calling app", AppCategory.Communication),
                    new AppPackageInfo("Microsoft.Microsoft365", "Microsoft 365 (Office)", "Office 365 hub", AppCategory.Productivity),

                    // Additional Entertainment
                    new AppPackageInfo("Microsoft.XboxLive", "Xbox Live", "Xbox Live services", AppCategory.Gaming),
                    new AppPackageInfo("Microsoft.WindowsTerminal", "Windows Terminal", "Command-line terminal", AppCategory.Development),
                    new AppPackageInfo("Microsoft.PowerShell", "PowerShell", "PowerShell Core", AppCategory.Development),

                    // Additional Social & Communication  
                    new AppPackageInfo("Microsoft.Messaging", "Messaging", "SMS/MMS app", AppCategory.Communication),
                    new AppPackageInfo("Microsoft.OneConnect", "Mobile Plans", "Cellular data plans", AppCategory.Communication),

                    // Additional Media
                    new AppPackageInfo("Microsoft.HEIFImageExtension", "HEIF Image Extension", "iOS photo format support", AppCategory.Media),
                    new AppPackageInfo("Microsoft.VP9VideoExtensions", "VP9 Video Extensions", "Video codec", AppCategory.Media),
                    new AppPackageInfo("Microsoft.WebMediaExtensions", "Web Media Extensions", "Media codec pack", AppCategory.Media),
                    new AppPackageInfo("Microsoft.WebpImageExtension", "WebP Image Extension", "WebP image support", AppCategory.Media),
                    new AppPackageInfo("Microsoft.RawImageExtension", "Raw Image Extension", "RAW photo support", AppCategory.Media),
                    new AppPackageInfo("Microsoft.MPEG2VideoExtension", "MPEG-2 Video Extension", "MPEG-2 codec", AppCategory.Media),
                    new AppPackageInfo("Microsoft.AV1VideoExtension", "AV1 Video Extension", "AV1 codec", AppCategory.Media),

                    // Additional Widgets & News
                    new AppPackageInfo("Microsoft.BingFinance", "Money", "Finance tracker", AppCategory.Widgets),
                    new AppPackageInfo("Microsoft.BingSports", "Sports", "Sports news", AppCategory.Widgets),
                    new AppPackageInfo("MicrosoftCorporationII.MicrosoftFamily", "Microsoft Family Safety", "Parental controls", AppCategory.Security),

                    // Additional Gaming
                    new AppPackageInfo("Microsoft.GamingServices", "Gaming Services", "Xbox gaming backend", AppCategory.Gaming),

                    // Additional Accessibility
                    new AppPackageInfo("Microsoft.ScreenReader", "Narrator", "Screen reader (accessibility)", AppCategory.Accessibility),

                    // Additional Store & Shopping
                    new AppPackageInfo("Microsoft.StorePurchaseApp", "Store Purchase App", "In-app purchase handler", AppCategory.Store),

                    // Additional Developer Tools
                    new AppPackageInfo("Microsoft.WinDbg", "WinDbg", "Windows Debugger", AppCategory.Development),

                    // Additional OEM/Bloatware (varies by manufacturer)
                    new AppPackageInfo("Dell.SupportAssistforPCs", "Dell SupportAssist", "Dell support software", AppCategory.OEM),
                    new AppPackageInfo("HPInc.HPSupportAssistant", "HP Support Assistant", "HP support software", AppCategory.OEM),
                    new AppPackageInfo("Lenovo.LenovoUtility", "Lenovo Utility", "Lenovo system tools", AppCategory.OEM),
                    new AppPackageInfo("ASUSTeK.ASUSPCAssistant", "ASUS PC Assistant", "ASUS system tools", AppCategory.OEM),
                    new AppPackageInfo("AcerIncorporated.AcerCare", "Acer Care Center", "Acer support app", AppCategory.OEM),
                    new AppPackageInfo("Microsoft.Advertising.Xaml", "Microsoft Advertising SDK", "Ad framework", AppCategory.System),

                    // Disney+, Netflix etc (pre-installed on some systems)
                    new AppPackageInfo("Disney.37853FC22B2CE", "Disney+", "Streaming service", AppCategory.Entertainment),
                    new AppPackageInfo("NetflixInc.Netflix", "Netflix", "Streaming service", AppCategory.Entertainment),
                    new AppPackageInfo("PandoraMediaInc.Pandora", "Pandora", "Music streaming", AppCategory.Entertainment),
                    new AppPackageInfo("5319275A.WhatsAppDesktop", "WhatsApp", "Messaging app", AppCategory.Communication),
                    new AppPackageInfo("2414FC7A.Viber", "Viber", "VoIP & messaging", AppCategory.Communication),
                    new AppPackageInfo("Facebook.Facebook", "Facebook", "Social media", AppCategory.Communication),
                    new AppPackageInfo("Facebook.InstagramBeta", "Instagram", "Social media", AppCategory.Communication),
                    new AppPackageInfo("Twitter.Twitter", "Twitter (X)", "Social media", AppCategory.Communication),
                    new AppPackageInfo("Amazon.com.Amazon", "Amazon", "Shopping app", AppCategory.Shopping),
                    new AppPackageInfo("Flipboard.Flipboard", "Flipboard", "News aggregator", AppCategory.Entertainment),
                    new AppPackageInfo("Duolingo.DuolingoforSchools", "Duolingo", "Language learning", AppCategory.Education),
                    new AppPackageInfo("king.com.CandyCrushSaga", "Candy Crush Saga", "Mobile game", AppCategory.Gaming),
                    new AppPackageInfo("king.com.CandyCrushSodaSaga", "Candy Crush Soda Saga", "Mobile game", AppCategory.Gaming),
                    new AppPackageInfo("king.com.FarmHeroesSaga", "Farm Heroes Saga", "Mobile game", AppCategory.Gaming),
                    new AppPackageInfo("Minecraft.Minecraft", "Minecraft (trial)", "Game trial", AppCategory.Gaming),
                    new AppPackageInfo("GAMELOFTSA.Asphalt8Airborne", "Asphalt 8", "Racing game", AppCategory.Gaming),
                    new AppPackageInfo("BytedanceInc.TikTok", "TikTok", "Video social media", AppCategory.Communication),

                };
            }

            public bool RemoveApp(string packageName)
            {
                try
                {
                    _engine.Log($"Attempting to remove: {packageName}");

                    // Remove for current user using DISM
                    string output = _engine.ExecuteCommand("powershell.exe",
                        $"-Command \"Get-AppxPackage -Name '{packageName}' | Remove-AppxPackage\"",
                        false);

                    // Alternative: Using DISM directly (works without PowerShell)
                    string[] commands = new[]
                    {
                        // Remove for current user
                        $"powershell -Command \"Get-AppxPackage '{packageName}' | Remove-AppxPackage\"",
                        
                        // Remove for all users
                        $"powershell -Command \"Get-AppxPackage -AllUsers '{packageName}' | Remove-AppxPackage -AllUsers\"",
                        
                        // Remove provisioned package
                        $"powershell -Command \"Get-AppxProvisionedPackage -Online | Where-Object DisplayName -eq '{packageName}' | Remove-AppxProvisionedPackage -Online\""
                    };

                    foreach (var cmd in commands)
                    {
                        ExecuteCmd(cmd);
                    }

                    _engine.Log($"Successfully removed: {packageName}", LogLevel.Success);
                    return true;
                }
                catch (Exception ex)
                {
                    _engine.Log($"Failed to remove {packageName}: {ex.Message}", LogLevel.Error);
                    return false;
                }
            }

            private void ExecuteCmd(string command)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {command}",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (var process = Process.Start(psi))
                    {
                        process?.WaitForExit();
                    }
                }
                catch { }
            }

            public void RemoveMultiple(List<string> packageNames)
            {
                int total = packageNames.Count;
                int current = 0;

                foreach (var package in packageNames)
                {
                    current++;
                    _engine.UpdateProgress((current * 100) / total, $"Removing {package}...");
                    RemoveApp(package);
                }
            }
        }

        #endregion

        #region Windows Features

        /// <summary>
        /// Category: Windows Features
        /// Disable/Remove optional Windows features
        /// </summary>
        public class WindowsFeatures
        {
            private readonly DebloatEngine _engine;

            public WindowsFeatures(DebloatEngine engine)
            {
                _engine = engine;
            }

            public List<FeatureInfo> GetDisableableFeatures()
            {
                return new List<FeatureInfo>
                {
                    new FeatureInfo("Internet Explorer", "Internet-Explorer-Optional-amd64", "Legacy IE browser", FeatureCategory.Legacy),
                    new FeatureInfo("Windows Media Player", "WindowsMediaPlayer", "Legacy media player", FeatureCategory.Media),
                    new FeatureInfo("Work Folders Client", "WorkFolders-Client", "Enterprise file sync", FeatureCategory.Enterprise),
                    new FeatureInfo("Windows Fax and Scan", "FaxServicesClientPackage", "Fax functionality", FeatureCategory.Legacy),
                    new FeatureInfo("Windows Hello Face", "Hello-Face-Package", "Facial recognition (if not used)", FeatureCategory.Biometrics),
                    new FeatureInfo("Math Recognizer", "MathRecognizer", "Handwriting math input", FeatureCategory.Input),
                    new FeatureInfo("Steps Recorder", "StepsRecorder", "Problem recording tool", FeatureCategory.Troubleshooting),
                    new FeatureInfo("Windows PowerShell 2.0", "MicrosoftWindowsPowerShellV2Root", "Legacy PowerShell (security risk)", FeatureCategory.Legacy),
                };
            }

            public bool DisableFeature(string featureName)
            {
                try
                {
                    _engine.Log($"Disabling Windows Feature: {featureName}");

                    string output = _engine.ExecuteCommand("dism.exe",
                        $"/Online /Disable-Feature /FeatureName:{featureName} /NoRestart");

                    _engine.Log($"Feature disabled: {featureName}", LogLevel.Success);
                    return true;
                }
                catch (Exception ex)
                {
                    _engine.Log($"Failed to disable feature {featureName}: {ex.Message}", LogLevel.Error);
                    return false;
                }
            }

            public void DisableMultiple(List<string> featureNames)
            {
                int total = featureNames.Count;
                int current = 0;

                foreach (var feature in featureNames)
                {
                    current++;
                    _engine.UpdateProgress((current * 100) / total, $"Disabling {feature}...");
                    DisableFeature(feature);
                }
            }
        }

        #endregion

        #region Telemetry & Privacy

        /// <summary>
        /// Category: Telemetry & Privacy
        /// Disable Windows telemetry and tracking
        /// </summary>
        public class TelemetryDebloat
        {
            private readonly DebloatEngine _engine;

            public TelemetryDebloat(DebloatEngine engine)
            {
                _engine = engine;
            }

            public List<TelemetryOption> GetTelemetryOptions()
            {
                return new List<TelemetryOption>
                {
                    new TelemetryOption("Disable Telemetry", "Sets telemetry to minimum level", TelemetryCategory.DataCollection),
                    new TelemetryOption("Disable Advertising ID", "Prevents ad tracking", TelemetryCategory.Advertising),
                    new TelemetryOption("Disable Activity History", "Stops timeline tracking", TelemetryCategory.DataCollection),
                    new TelemetryOption("Disable App Diagnostics", "Prevents app usage tracking", TelemetryCategory.DataCollection),
                    new TelemetryOption("Disable Feedback Notifications", "Stops feedback prompts", TelemetryCategory.Notifications),
                    new TelemetryOption("Disable Cloud Clipboard", "Local clipboard only", TelemetryCategory.Cloud),
                    new TelemetryOption("Disable Tailored Experiences", "No personalized tips", TelemetryCategory.Personalization),
                    new TelemetryOption("Disable Location Tracking", "Disables location services", TelemetryCategory.Location),
                    new TelemetryOption("Disable Web Search in Start Menu", "Local search only", TelemetryCategory.Search),
                    new TelemetryOption("Disable Cortana", "Removes voice assistant", TelemetryCategory.Assistant),
                };
            }

            public void DisableAllTelemetry()
            {
                _engine.Log("Starting comprehensive telemetry disable...");

                DisableTelemetryServices();
                DisableTelemetryRegistry();
                DisableTelemetryTasks();

                _engine.Log("Telemetry disable completed", LogLevel.Success);
            }

            private void DisableTelemetryServices()
            {
                var services = new[]
                {
                    "DiagTrack",
                    "dmwappushservice",
                    "diagnosticshub.standardcollector.service",
                    "DPS",
                    "WerSvc",
                    "PcaSvc",
                    "RemoteRegistry",
                };

                foreach (var service in services)
                {
                    try
                    {
                        _engine.ExecuteCommand("sc", $"config {service} start= disabled");
                        _engine.ExecuteCommand("sc", $"stop {service}");
                        _engine.Log($"Disabled service: {service}");
                    }
                    catch (Exception ex)
                    {
                        _engine.Log($"Could not disable service {service}: {ex.Message}", LogLevel.Warning);
                    }
                }
            }

            private void DisableTelemetryRegistry()
            {
                var registryTweaks = new Dictionary<string, Dictionary<string, object>>
                {
                    [@"SOFTWARE\Policies\Microsoft\Windows\DataCollection"] = new Dictionary<string, object>
                    {
                        { "AllowTelemetry", 0 },
                        { "DoNotShowFeedbackNotifications", 1 },
                        { "DisableEnterpriseAuthProxy", 1 }
                    },

                    [@"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo"] = new Dictionary<string, object>
                    {
                        { "Enabled", 0 }
                    },

                    [@"SOFTWARE\Policies\Microsoft\Windows\System"] = new Dictionary<string, object>
                    {
                        { "EnableActivityFeed", 0 },
                        { "PublishUserActivities", 0 },
                        { "UploadUserActivities", 0 }
                    },

                    [@"SOFTWARE\Policies\Microsoft\Windows\Windows Search"] = new Dictionary<string, object>
                    {
                        { "AllowCortana", 0 },
                        { "AllowSearchToUseLocation", 0 },
                        { "DisableWebSearch", 1 },
                        { "ConnectedSearchUseWeb", 0 }
                    },

                    [@"SOFTWARE\Policies\Microsoft\Windows\CloudContent"] = new Dictionary<string, object>
                    {
                        { "DisableWindowsConsumerFeatures", 1 },
                        { "DisableSoftLanding", 1 },
                        { "DisableCloudOptimizedContent", 1 }
                    },

                    [@"SOFTWARE\Microsoft\Windows\CurrentVersion\Diagnostics\DiagTrack"] = new Dictionary<string, object>
                    {
                        { "ShowedToastAtLevel", 1 }
                    },

                    [@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location"] = new Dictionary<string, object>
                    {
                        { "Value", "Deny" }
                    },

                    [@"SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy"] = new Dictionary<string, object>
                    {
                        { "TailoredExperiencesWithDiagnosticDataEnabled", 0 }
                    },

                    [@"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"] = new Dictionary<string, object>
                    {
                        { "SubscribedContent-338393Enabled", 0 },
                        { "SubscribedContent-353694Enabled", 0 },
                        { "SubscribedContent-353696Enabled", 0 }
                    },
                };

                foreach (var regPath in registryTweaks)
                {
                    try
                    {
                        using (var key = Registry.LocalMachine.CreateSubKey(regPath.Key))
                        {
                            if (key != null)
                            {
                                foreach (var value in regPath.Value)
                                {
                                    RegistryValueKind kind = value.Value is int ? RegistryValueKind.DWord : RegistryValueKind.String;
                                    key.SetValue(value.Key, value.Value, kind);
                                }
                            }
                        }
                        _engine.Log($"Applied registry tweaks: {regPath.Key}");
                    }
                    catch (Exception ex)
                    {
                        _engine.Log($"Registry error at {regPath.Key}: {ex.Message}", LogLevel.Warning);
                    }
                }
            }

            private void DisableTelemetryTasks()
            {
                var tasks = new[]
                {
                    @"Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser",
                    @"Microsoft\Windows\Application Experience\ProgramDataUpdater",
                    @"Microsoft\Windows\Autochk\Proxy",
                    @"Microsoft\Windows\Customer Experience Improvement Program\Consolidator",
                    @"Microsoft\Windows\Customer Experience Improvement Program\UsbCeip",
                    @"Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector",
                    @"Microsoft\Windows\Feedback\Siuf\DmClient",
                    @"Microsoft\Windows\Feedback\Siuf\DmClientOnScenarioDownload",
                    @"Microsoft\Windows\Windows Error Reporting\QueueReporting",
                };

                foreach (var task in tasks)
                {
                    try
                    {
                        _engine.ExecuteCommand("schtasks", $"/Change /TN \"{task}\" /Disable");
                        _engine.Log($"Disabled scheduled task: {task}");
                    }
                    catch
                    {
                        _engine.Log($"Could not disable task: {task}", LogLevel.Warning);
                    }
                }
            }
        }

        #endregion

        #region Services Debloat

        /// <summary>
        /// Category: Services
        /// Disable unnecessary Windows services
        /// </summary>
        public class ServicesDebloat
        {
            private readonly DebloatEngine _engine;

            public ServicesDebloat(DebloatEngine engine)
            {
                _engine = engine;
            }

            public List<ServiceInfo> GetDisableableServices()
            {
                return new List<ServiceInfo>
                {
                    // Gaming
                    new ServiceInfo("XblAuthManager", "Xbox Live Auth Manager", "Xbox authentication", ServiceCategory.Gaming, ServiceRisk.Safe),
                    new ServiceInfo("XblGameSave", "Xbox Live Game Save", "Xbox cloud saves", ServiceCategory.Gaming, ServiceRisk.Safe),
                    new ServiceInfo("XboxGipSvc", "Xbox Accessory Management", "Xbox controller support", ServiceCategory.Gaming, ServiceRisk.Caution),
                    new ServiceInfo("XboxNetApiSvc", "Xbox Live Networking", "Xbox multiplayer", ServiceCategory.Gaming, ServiceRisk.Safe),
                    
                    // Telemetry
                    new ServiceInfo("DiagTrack", "Connected User Experiences and Telemetry", "Main telemetry service", ServiceCategory.Telemetry, ServiceRisk.Recommended),
                    new ServiceInfo("dmwappushservice", "WAP Push Message Routing", "Telemetry routing", ServiceCategory.Telemetry, ServiceRisk.Recommended),
                    new ServiceInfo("diagnosticshub.standardcollector.service", "Diagnostics Hub", "Diagnostics collection", ServiceCategory.Telemetry, ServiceRisk.Safe),
                    
                    // Remote & Sharing
                    new ServiceInfo("RemoteRegistry", "Remote Registry", "Remote registry access (security risk)", ServiceCategory.Remote, ServiceRisk.Recommended),
                    new ServiceInfo("RemoteAccess", "Routing and Remote Access", "VPN/routing (if not used)", ServiceCategory.Remote, ServiceRisk.Caution),
                    new ServiceInfo("SessionEnv", "Remote Desktop Configuration", "RDP config", ServiceCategory.Remote, ServiceRisk.Caution),
                    new ServiceInfo("TermService", "Remote Desktop Services", "RDP service", ServiceCategory.Remote, ServiceRisk.Caution),
                    new ServiceInfo("UmRdpService", "Remote Desktop Services UserMode Port Redirector", "RDP port redirector", ServiceCategory.Remote, ServiceRisk.Caution),
                    
                    // Printing
                    new ServiceInfo("Spooler", "Print Spooler", "Printing service (if no printer)", ServiceCategory.Printing, ServiceRisk.Caution),
                    new ServiceInfo("PrintNotify", "Printer Extensions and Notifications", "Printer notifications", ServiceCategory.Printing, ServiceRisk.Safe),
                    new ServiceInfo("PrintWorkflowUserSvc", "PrintWorkflow", "Modern printing", ServiceCategory.Printing, ServiceRisk.Safe),
                    
                    // Search
                    new ServiceInfo("WSearch", "Windows Search", "File indexing (slows down)", ServiceCategory.Search, ServiceRisk.Caution),
                    
                    // Biometrics
                    new ServiceInfo("WbioSrvc", "Windows Biometric Service", "Fingerprint/face recognition", ServiceCategory.Biometrics, ServiceRisk.Caution),
                    
                    // Location
                    new ServiceInfo("lfsvc", "Geolocation Service", "Location tracking", ServiceCategory.Location, ServiceRisk.Safe),
                    
                    // Performance (can cause issues)
                    new ServiceInfo("SysMain", "Superfetch/SysMain", "Preloading apps (high disk usage)", ServiceCategory.Performance, ServiceRisk.Caution),
                    new ServiceInfo("WerSvc", "Windows Error Reporting", "Crash reporting", ServiceCategory.Telemetry, ServiceRisk.Safe),
                    
                    // Mixed Reality
                    new ServiceInfo("MixedRealityOpenXRSvc", "Mixed Reality OpenXR Service", "VR/AR support", ServiceCategory.MixedReality, ServiceRisk.Safe),
                    new ServiceInfo("perceptionsimulation", "Windows Perception Simulation", "VR simulation", ServiceCategory.MixedReality, ServiceRisk.Safe),
                    
                    // Others
                    new ServiceInfo("MapsBroker", "Downloaded Maps Manager", "Offline maps", ServiceCategory.Navigation, ServiceRisk.Safe),
                    new ServiceInfo("PcaSvc", "Program Compatibility Assistant", "Compatibility checks", ServiceCategory.Performance, ServiceRisk.Safe),
                };
            }

            public bool DisableService(string serviceName)
            {
                try
                {
                    _engine.Log($"Disabling service: {serviceName}");

                    _engine.ExecuteCommand("sc", $"config {serviceName} start= disabled");
                    _engine.ExecuteCommand("sc", $"stop {serviceName}");

                    _engine.Log($"Service disabled: {serviceName}", LogLevel.Success);
                    return true;
                }
                catch (Exception ex)
                {
                    _engine.Log($"Failed to disable service {serviceName}: {ex.Message}", LogLevel.Error);
                    return false;
                }
            }

            public void DisableMultiple(List<string> serviceNames)
            {
                int total = serviceNames.Count;
                int current = 0;

                foreach (var service in serviceNames)
                {
                    current++;
                    _engine.UpdateProgress((current * 100) / total, $"Disabling service {service}...");
                    DisableService(service);
                }
            }
        }

        #endregion

        #region OneDrive Removal

        /// <summary>
        /// Category: OneDrive
        /// Complete OneDrive removal
        /// </summary>
        public class OneDriveDebloat
        {
            private readonly DebloatEngine _engine;

            public OneDriveDebloat(DebloatEngine engine)
            {
                _engine = engine;
            }

            public bool RemoveOneDrive()
            {
                try
                {
                    _engine.Log("Starting OneDrive removal...");

                    // Stop OneDrive processes
                    KillOneDriveProcesses();

                    // Uninstall OneDrive
                    UninstallOneDrive();

                    // Clean up registry
                    CleanupOneDriveRegistry();

                    // Remove leftover folders
                    RemoveOneDriveFolders();

                    _engine.Log("OneDrive removed successfully", LogLevel.Success);
                    return true;
                }
                catch (Exception ex)
                {
                    _engine.Log($"OneDrive removal failed: {ex.Message}", LogLevel.Error);
                    return false;
                }
            }

            private void KillOneDriveProcesses()
            {
                var processes = new[] { "OneDrive", "OneDriveSetup", "FileCoAuth" };

                foreach (var proc in processes)
                {
                    try
                    {
                        _engine.ExecuteCommand("taskkill", $"/F /IM {proc}.exe");
                    }
                    catch { }
                }
            }

            private void UninstallOneDrive()
            {
                string setupPath32 = @"C:\Windows\System32\OneDriveSetup.exe";
                string setupPath64 = @"C:\Windows\SysWOW64\OneDriveSetup.exe";

                if (File.Exists(setupPath64))
                {
                    _engine.ExecuteCommand(setupPath64, "/uninstall");
                }
                else if (File.Exists(setupPath32))
                {
                    _engine.ExecuteCommand(setupPath32, "/uninstall");
                }
            }

            private void CleanupOneDriveRegistry()
            {
                var regPaths = new[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{018D5C66-4533-4307-9B53-224DE2ED1FE6}",
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{018D5C66-4533-4307-9B53-224DE2ED1FE6}",
                    @"SOFTWARE\Classes\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}",
                    @"SOFTWARE\Classes\Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}",
                };

                foreach (var path in regPaths)
                {
                    try
                    {
                        Registry.LocalMachine.DeleteSubKeyTree(path, false);
                    }
                    catch { }
                }

                // Disable OneDrive via policy
                try
                {
                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\OneDrive"))
                    {
                        key?.SetValue("DisableFileSyncNGSC", 1, RegistryValueKind.DWord);
                    }
                }
                catch { }
            }

            private void RemoveOneDriveFolders()
            {
                var foldersToRemove = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\OneDrive"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft OneDrive"),
                    @"C:\OneDriveTemp"
                };

                foreach (var folder in foldersToRemove)
                {
                    try
                    {
                        if (Directory.Exists(folder))
                        {
                            Directory.Delete(folder, true);
                        }
                    }
                    catch { }
                }
            }
        }

        #endregion

        #region Edge Removal

        /// <summary>
        /// Category: Edge Browser
        /// Remove Microsoft Edge (WARNING: Can cause issues)
        /// </summary>
        public class EdgeDebloat
        {
            private readonly DebloatEngine _engine;

            public EdgeDebloat(DebloatEngine engine)
            {
                _engine = engine;
            }

            public bool RemoveEdge()
            {
                try
                {
                    _engine.Log("WARNING: Removing Edge can cause issues with Windows features!", LogLevel.Warning);

                    // Stop Edge processes
                    KillEdgeProcesses();

                    // Find and run Edge uninstaller
                    UninstallEdge();

                    // Remove Edge Update
                    RemoveEdgeUpdate();

                    // Registry cleanup
                    CleanupEdgeRegistry();

                    _engine.Log("Edge removal attempted", LogLevel.Success);
                    return true;
                }
                catch (Exception ex)
                {
                    _engine.Log($"Edge removal failed: {ex.Message}", LogLevel.Error);
                    return false;
                }
            }

            private void KillEdgeProcesses()
            {
                var processes = new[] { "msedge", "MicrosoftEdgeUpdate", "msedgewebview2" };

                foreach (var proc in processes)
                {
                    try
                    {
                        _engine.ExecuteCommand("taskkill", $"/F /IM {proc}.exe");
                    }
                    catch { }
                }
            }

            private void UninstallEdge()
            {
                string edgePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Microsoft",
                    "Edge",
                    "Application"
                );

                if (Directory.Exists(edgePath))
                {
                    var setupFiles = Directory.GetFiles(edgePath, "setup.exe", SearchOption.AllDirectories);

                    foreach (var setup in setupFiles)
                    {
                        _engine.ExecuteCommand(setup, "--uninstall --force-uninstall --system-level");
                    }
                }
            }

            private void RemoveEdgeUpdate()
            {
                string updatePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Microsoft",
                    "EdgeUpdate"
                );

                if (Directory.Exists(updatePath))
                {
                    try
                    {
                        Directory.Delete(updatePath, true);
                    }
                    catch { }
                }

                // Stop EdgeUpdate service
                _engine.ExecuteCommand("sc", "delete edgeupdate");
                _engine.ExecuteCommand("sc", "delete edgeupdatem");
            }

            private void CleanupEdgeRegistry()
            {
                try
                {
                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Edge"))
                    {
                        key?.SetValue("InstallDefault", 0, RegistryValueKind.DWord);
                    }
                }
                catch { }
            }
        }

        #endregion

        #region Context Menu Cleanup

        /// <summary>
        /// Category: Context Menu
        /// Clean up Windows 11 context menu
        /// </summary>
        public class ContextMenuDebloat
        {
            private readonly DebloatEngine _engine;

            public ContextMenuDebloat(DebloatEngine engine)
            {
                _engine = engine;
            }

            public List<ContextMenuOption> GetContextMenuOptions()
            {
                return new List<ContextMenuOption>
                {
                    new ContextMenuOption("Restore Windows 10 Context Menu", "Shows full menu without 'Show more options'"),
                    new ContextMenuOption("Remove 3D Objects from This PC", "Removes 3D Objects folder"),
                    new ContextMenuOption("Remove OneDrive from Sidebar", "Removes OneDrive entry"),
                    new ContextMenuOption("Remove Edit with Paint 3D", "Removes Paint 3D context menu"),
                    new ContextMenuOption("Remove Edit with Photos", "Removes Photos app entry"),
                    new ContextMenuOption("Remove Share", "Removes Share option"),
                    new ContextMenuOption("Remove 'Give access to'", "Removes sharing wizard"),
                };
            }

            public void RestoreWin10ContextMenu()
            {
                try
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32"))
                    {
                        key?.SetValue("", "", RegistryValueKind.String);
                    }

                    RestartExplorer();
                    _engine.Log("Windows 10 context menu restored", LogLevel.Success);
                }
                catch (Exception ex)
                {
                    _engine.Log($"Context menu restore failed: {ex.Message}", LogLevel.Error);
                }
            }

            public void Remove3DObjects()
            {
                try
                {
                    var regPaths = new[]
                    {
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}",
                        @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}"
                    };

                    foreach (var path in regPaths)
                    {
                        Registry.LocalMachine.DeleteSubKeyTree(path, false);
                    }

                    _engine.Log("3D Objects removed from This PC", LogLevel.Success);
                }
                catch (Exception ex)
                {
                    _engine.Log($"Failed to remove 3D Objects: {ex.Message}", LogLevel.Error);
                }
            }

            public void RemoveShareMenu()
            {
                try
                {
                    // Remove Share from context menu
                    Registry.ClassesRoot.DeleteSubKeyTree(@"*\shellex\ContextMenuHandlers\Sharing", false);
                    Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\shellex\ContextMenuHandlers\Sharing", false);
                    Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\shellex\CopyHookHandlers\Sharing", false);
                    Registry.ClassesRoot.DeleteSubKeyTree(@"Drive\shellex\ContextMenuHandlers\Sharing", false);

                    _engine.Log("Share menu removed", LogLevel.Success);
                }
                catch (Exception ex)
                {
                    _engine.Log($"Failed to remove Share menu: {ex.Message}", LogLevel.Error);
                }
            }

            public void RemovePaint3DMenu()
            {
                try
                {
                    var regPaths = new[]
                    {
                        @"SystemFileAssociations\.3mf\Shell\3D Edit",
                        @"SystemFileAssociations\.bmp\Shell\3D Edit",
                        @"SystemFileAssociations\.fbx\Shell\3D Edit",
                        @"SystemFileAssociations\.gif\Shell\3D Edit",
                        @"SystemFileAssociations\.jfif\Shell\3D Edit",
                        @"SystemFileAssociations\.jpe\Shell\3D Edit",
                        @"SystemFileAssociations\.jpeg\Shell\3D Edit",
                        @"SystemFileAssociations\.jpg\Shell\3D Edit",
                        @"SystemFileAssociations\.png\Shell\3D Edit",
                        @"SystemFileAssociations\.tif\Shell\3D Edit",
                        @"SystemFileAssociations\.tiff\Shell\3D Edit",
                    };

                    foreach (var path in regPaths)
                    {
                        Registry.ClassesRoot.DeleteSubKeyTree(path, false);
                    }

                    _engine.Log("Paint 3D context menu removed", LogLevel.Success);
                }
                catch (Exception ex)
                {
                    _engine.Log($"Failed to remove Paint 3D menu: {ex.Message}", LogLevel.Error);
                }
            }

            private void RestartExplorer()
            {
                try
                {
                    foreach (var proc in Process.GetProcessesByName("explorer"))
                    {
                        proc.Kill();
                    }

                    System.Threading.Thread.Sleep(1000);
                    Process.Start("explorer.exe");
                }
                catch { }
            }
        }

        #endregion

        #region Startup Programs

        /// <summary>
        /// Category: Startup
        /// Manage startup programs
        /// </summary>
        public class StartupDebloat
        {
            private readonly DebloatEngine _engine;

            public StartupDebloat(DebloatEngine engine)
            {
                _engine = engine;
            }

            public List<StartupInfo> GetStartupPrograms()
            {
                var startupItems = new List<StartupInfo>();

                var registryPaths = new[]
                {
                    new { Root = Registry.LocalMachine, Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run" },
                    new { Root = Registry.LocalMachine, Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" },
                    new { Root = Registry.LocalMachine, Path = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run" },
                    new { Root = Registry.CurrentUser, Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run" },
                    new { Root = Registry.CurrentUser, Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" },
                };

                foreach (var regPath in registryPaths)
                {
                    try
                    {
                        using (var key = regPath.Root.OpenSubKey(regPath.Path))
                        {
                            if (key != null)
                            {
                                foreach (var valueName in key.GetValueNames())
                                {
                                    var value = key.GetValue(valueName)?.ToString();
                                    startupItems.Add(new StartupInfo
                                    {
                                        Name = valueName,
                                        Command = value,
                                        Location = $"{regPath.Root.Name}\\{regPath.Path}",
                                        Enabled = true
                                    });
                                }
                            }
                        }
                    }
                    catch { }
                }

                // Also check Startup folder
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                if (Directory.Exists(startupFolder))
                {
                    foreach (var file in Directory.GetFiles(startupFolder))
                    {
                        startupItems.Add(new StartupInfo
                        {
                            Name = Path.GetFileName(file),
                            Command = file,
                            Location = "Startup Folder",
                            Enabled = true
                        });
                    }
                }

                return startupItems;
            }

            public void DisableStartupItem(StartupInfo item)
            {
                try
                {
                    if (item.Location.Contains("HKEY_LOCAL_MACHINE"))
                    {
                        string path = item.Location.Replace("HKEY_LOCAL_MACHINE\\", "");
                        using (var key = Registry.LocalMachine.OpenSubKey(path, true))
                        {
                            key?.DeleteValue(item.Name, false);
                        }
                    }
                    else if (item.Location.Contains("HKEY_CURRENT_USER"))
                    {
                        string path = item.Location.Replace("HKEY_CURRENT_USER\\", "");
                        using (var key = Registry.CurrentUser.OpenSubKey(path, true))
                        {
                            key?.DeleteValue(item.Name, false);
                        }
                    }
                    else if (item.Location == "Startup Folder")
                    {
                        File.Delete(item.Command);
                    }

                    _engine.Log($"Disabled startup item: {item.Name}", LogLevel.Success);
                }
                catch (Exception ex)
                {
                    _engine.Log($"Failed to disable startup item {item.Name}: {ex.Message}", LogLevel.Error);
                }
            }
        }

        #endregion

        #region Windows Update Tweaks

        /// <summary>
        /// Category: Windows Update
        /// Optimize Windows Update behavior
        /// </summary>
        public class UpdateDebloat
        {
            private readonly DebloatEngine _engine;

            public UpdateDebloat(DebloatEngine engine)
            {
                _engine = engine;
            }

            public List<UpdateOption> GetUpdateOptions()
            {
                return new List<UpdateOption>
                {
                    new UpdateOption("Disable Automatic Driver Updates", "Prevent automatic driver installation", UpdateRisk.Caution),
                    new UpdateOption("Disable Update Restarts", "No automatic restarts", UpdateRisk.Safe),
                    new UpdateOption("Disable P2P Update Sharing", "No Delivery Optimization", UpdateRisk.Safe),
                    new UpdateOption("Delay Feature Updates", "Postpone major updates", UpdateRisk.Safe),
                };
            }

            public void DisableAutomaticDriverUpdates()
            {
                try
                {
                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate"))
                    {
                        key?.SetValue("ExcludeWUDriversInQualityUpdate", 1, RegistryValueKind.DWord);
                    }

                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching"))
                    {
                        key?.SetValue("SearchOrderConfig", 0, RegistryValueKind.DWord);
                    }

                    _engine.Log("Automatic driver updates disabled", LogLevel.Success);
                }
                catch (Exception ex)
                {
                    _engine.Log($"Failed to disable driver updates: {ex.Message}", LogLevel.Error);
                }
            }

            public void DisableP2PUpdates()
            {
                try
                {
                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization"))
                    {
                        key?.SetValue("DODownloadMode", 0, RegistryValueKind.DWord);
                    }

                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config"))
                    {
                        key?.SetValue("DODownloadMode", 0, RegistryValueKind.DWord);
                    }

                    _engine.Log("P2P update sharing disabled", LogLevel.Success);
                }
                catch (Exception ex)
                {
                    _engine.Log($"Failed to disable P2P updates: {ex.Message}", LogLevel.Error);
                }
            }

            public void DisableAutomaticRestarts()
            {
                try
                {
                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"))
                    {
                        key?.SetValue("NoAutoRebootWithLoggedOnUsers", 1, RegistryValueKind.DWord);
                        key?.SetValue("AUOptions", 3, RegistryValueKind.DWord); // Notify for download and install
                    }

                    _engine.Log("Automatic restart after updates disabled", LogLevel.Success);
                }
                catch (Exception ex)
                {
                    _engine.Log($"Failed to disable automatic restarts: {ex.Message}", LogLevel.Error);
                }
            }

            public void DelayFeatureUpdates(int days = 365)
            {
                try
                {
                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate"))
                    {
                        key?.SetValue("DeferFeatureUpdates", 1, RegistryValueKind.DWord);
                        key?.SetValue("DeferFeatureUpdatesPeriodInDays", days, RegistryValueKind.DWord);
                    }

                    _engine.Log($"Feature updates delayed by {days} days", LogLevel.Success);
                }
                catch (Exception ex)
                {
                    _engine.Log($"Failed to delay feature updates: {ex.Message}", LogLevel.Error);
                }
            }
        }

        #endregion

        #region Public Properties

        public UWPApps Apps { get; private set; }
        public WindowsFeatures Features { get; private set; }
        public TelemetryDebloat Telemetry { get; private set; }
        public ServicesDebloat Services { get; private set; }
        public OneDriveDebloat OneDrive { get; private set; }
        public EdgeDebloat Edge { get; private set; }
        public ContextMenuDebloat ContextMenu { get; private set; }
        public StartupDebloat Startup { get; private set; }
        public UpdateDebloat Updates { get; private set; }

        #endregion

        public DebloatEngine()
        {
            Apps = new UWPApps(this);
            Features = new WindowsFeatures(this);
            Telemetry = new TelemetryDebloat(this);
            Services = new ServicesDebloat(this);
            OneDrive = new OneDriveDebloat(this);
            Edge = new EdgeDebloat(this);
            ContextMenu = new ContextMenuDebloat(this);
            Startup = new StartupDebloat(this);
            Updates = new UpdateDebloat(this);
        }
    }

    #region Supporting Classes

    public class DebloatProgressEventArgs : EventArgs
    {
        public int Percentage { get; }
        public string Status { get; }

        public DebloatProgressEventArgs(int percentage, string status)
        {
            Percentage = percentage;
            Status = status;
        }
    }

    public class DebloatLogEventArgs : EventArgs
    {
        public string Message { get; }
        public LogLevel Level { get; }
        public DateTime Timestamp { get; }

        public DebloatLogEventArgs(string message, LogLevel level)
        {
            Message = message;
            Level = level;
            Timestamp = DateTime.Now;
        }
    }

    public enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class AppPackageInfo
    {
        public string PackageName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public AppCategory Category { get; set; }

        public AppPackageInfo(string packageName, string displayName, string description, AppCategory category)
        {
            PackageName = packageName;
            DisplayName = displayName;
            Description = description;
            Category = category;
        }
    }

    public enum AppCategory
    {
        Communication,
        Gaming,
        Entertainment,
        Productivity,
        Media,
        MixedReality,
        Navigation,
        Telemetry,
        Store,
        Utilities,
        Development,
        Widgets,
        Assistant,
        Search,
        Cloud,
        Security,      
        System,        
        Accessibility,
        OEM,          
        Shopping,     
        Education
    }


    public class FeatureInfo
    {
        public string DisplayName { get; set; }
        public string FeatureName { get; set; }
        public string Description { get; set; }
        public FeatureCategory Category { get; set; }

        public FeatureInfo(string displayName, string featureName, string description, FeatureCategory category)
        {
            DisplayName = displayName;
            FeatureName = featureName;
            Description = description;
            Category = category;
        }
    }

    public enum FeatureCategory
    {
        Legacy,
        Media,
        Enterprise,
        Biometrics,
        Input,
        Troubleshooting,
        Virtualization
    }

    public class TelemetryOption
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public TelemetryCategory Category { get; set; }

        public TelemetryOption(string name, string description, TelemetryCategory category)
        {
            Name = name;
            Description = description;
            Category = category;
        }
    }

    public enum TelemetryCategory
    {
        DataCollection,
        Advertising,
        Notifications,
        Cloud,
        Personalization,
        Location,
        Search,
        Assistant
    }

    public class ServiceInfo
    {
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ServiceCategory Category { get; set; }
        public ServiceRisk Risk { get; set; }

        public ServiceInfo(string serviceName, string displayName, string description, ServiceCategory category, ServiceRisk risk)
        {
            ServiceName = serviceName;
            DisplayName = displayName;
            Description = description;
            Category = category;
            Risk = risk;
        }
    }

    public enum ServiceCategory
    {
        Gaming,
        Telemetry,
        Store,
        Updates,
        Biometrics,
        Remote,
        Printing,
        Search,
        Performance,
        Virtualization,
        MixedReality,
        Location,
        Navigation
    }

    public enum ServiceRisk
    {
        Safe,
        Caution,
        Recommended,
        Dangerous
    }

    public class ContextMenuOption
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public ContextMenuOption(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    public class StartupInfo
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string Location { get; set; }
        public bool Enabled { get; set; }
    }

    public class UpdateOption
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public UpdateRisk Risk { get; set; }

        public UpdateOption(string name, string description, UpdateRisk risk)
        {
            Name = name;
            Description = description;
            Risk = risk;
        }
    }

    public enum UpdateRisk
    {
        Safe,
        Caution,
        Dangerous
    }

    #endregion
}
