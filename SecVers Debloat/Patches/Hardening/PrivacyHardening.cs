using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Patches.Hardening
{
    public class PrivacyHardening
    {
        // Disable Telemetry completely
        public void DisableTelemetryCompletely()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                "AllowTelemetry", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection",
                "AllowTelemetry", 0, RegistryValueKind.DWord);
        }

        // Disable Cortana
        public void DisableCortana()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                "AllowCortana", 0, RegistryValueKind.DWord);
        }

        // Disable Windows Feedback
        public void DisableWindowsFeedback()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                "DoNotShowFeedbackNotifications", 1, RegistryValueKind.DWord);
        }

        // Disable Activity History
        public void DisableActivityHistory()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\System",
                "EnableActivityFeed", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\System",
                "PublishUserActivities", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\System",
                "UploadUserActivities", 0, RegistryValueKind.DWord);
        }

        // Disable Location Tracking
        public void DisableLocationTracking()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location",
                "Value", "Deny", RegistryValueKind.String);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Sensor\Overrides\{BFA794E4-F964-4FDB-90F6-51056BFE4B44}",
                "SensorPermissionState", 0, RegistryValueKind.DWord);
        }

        // Disable Camera Access
        public void DisableCameraAccess()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam",
                "Value", "Deny", RegistryValueKind.String);
        }

        // Disable Microphone Access
        public void DisableMicrophoneAccess()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone",
                "Value", "Deny", RegistryValueKind.String);
        }

        // Disable Advertising ID
        public void DisableAdvertisingID()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo",
                "DisabledByGroupPolicy", 1, RegistryValueKind.DWord);
        }

        // Disable Suggested Content
        public void DisableSuggestedContent()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                "SubscribedContent-338393Enabled", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                "SubscribedContent-353694Enabled", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                "SubscribedContent-353696Enabled", 0, RegistryValueKind.DWord);
        }

        // Disable App Suggestions in Start Menu
        public void DisableAppSuggestions()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord);
        }

        // Disable Timeline
        public void DisableTimeline()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\System",
                "EnableActivityFeed", 0, RegistryValueKind.DWord);
        }

        // Disable Cloud Clipboard
        public void DisableCloudClipboard()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\System",
                "AllowClipboardHistory", 0, RegistryValueKind.DWord);
        }

        // Disable Input Personalization (Typing insights)
        public void DisableInputPersonalization()
        {
            SetRegistryValue(@"SOFTWARE\Microsoft\Personalization\Settings",
                "AcceptedPrivacyPolicy", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\InputPersonalization",
                "RestrictImplicitTextCollection", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"SOFTWARE\Microsoft\InputPersonalization",
                "RestrictImplicitInkCollection", 1, RegistryValueKind.DWord);
        }

        // Disable OneDrive
        public void DisableOneDrive()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\OneDrive",
                "DisableFileSyncNGSC", 1, RegistryValueKind.DWord);
        }

        // Disable Windows Spotlight
        public void DisableWindowsSpotlight()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\CloudContent",
                "DisableWindowsSpotlightFeatures", 1, RegistryValueKind.DWord);
        }

        // Disable Tailored Experiences
        public void DisableTailoredExperiences()
        {
            SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\CloudContent",
                "DisableTailoredExperiencesWithDiagnosticData", 1, RegistryValueKind.DWord);
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
    }
}
