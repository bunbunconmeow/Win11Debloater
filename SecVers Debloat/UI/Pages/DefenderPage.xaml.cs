using Microsoft.Win32;
using SecVers_Debloat.Patches;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SecVers_Debloat.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für DefenderPage.xaml
    /// </summary>
    public partial class DefenderPage : Page
    {
        private Defender defenderPatches;
        public DefenderPage()
        {
            InitializeComponent();
            defenderPatches = new Defender();
            LoadCurrentSettings();
        }

        #region Preset Buttons

        private void BtnPresetDefault_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Apply recommended default Windows Defender settings?\n\nThis will enable all standard protection features.",
                "Apply Default Preset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                ApplyDefaultPreset();
                MessageBox.Show("Default preset applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnPresetGaming_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Apply gaming-optimized settings?\n\nThis will reduce real-time protection overhead while maintaining essential security.",
                "Apply Gaming Preset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                ApplyGamingPreset();
                MessageBox.Show("Gaming preset applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnPresetDisabled_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Apply minimal protection settings?\n\nWARNING: This will significantly reduce system security. Only use if you have alternative security measures.",
                "Apply Minimal Preset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                ApplyMinimalPreset();
                MessageBox.Show("Minimal preset applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region Apply Presets

        private void ApplyDefaultPreset()
        {
            // Real-Time Protection
            ToggleRealTimeProtection.IsOn = true;
            defenderPatches.SetRealTimeProtection(true);

            ToggleBehaviorMonitoring.IsOn = true;
            defenderPatches.SetBehaviorMonitoring(true);

            ToggleOnAccessProtection.IsOn = true;
            defenderPatches.SetOnAccessProtection(true);

            ToggleScriptScanning.IsOn = true;
            defenderPatches.SetScriptScanning(true);

            // Cloud Protection
            ToggleCloudProtection.IsOn = true;
            defenderPatches.SetCloudProtection(true);

            ToggleAutomaticSampleSubmission.IsOn = true;
            defenderPatches.SetAutomaticSampleSubmission(true);

            ComboCloudBlockLevel.SelectedIndex = 0;
            defenderPatches.SetCloudBlockLevel(0);

            // Scanning
            ToggleArchiveScanning.IsOn = true;
            defenderPatches.SetArchiveScanning(true);

            ToggleEmailScanning.IsOn = true;
            defenderPatches.SetEmailScanning(true);

            ToggleRemovableDriveScanning.IsOn = true;
            defenderPatches.SetRemovableDriveScanning(true);

            ToggleNetworkScanning.IsOn = false;
            defenderPatches.SetNetworkScanning(false);

            // Additional Protection
            ToggleControlledFolderAccess.IsOn = false;
            defenderPatches.SetControlledFolderAccess(false);

            TogglePUAProtection.IsOn = true;
            defenderPatches.SetPUAProtection(true);

            ToggleNetworkProtection.IsOn = true;
            defenderPatches.SetNetworkProtection(true);

            ToggleExploitProtection.IsOn = true;
            defenderPatches.SetExploitProtection(true);

            // Windows Security
            ToggleSmartScreen.IsOn = true;
            defenderPatches.SetSmartScreen(true);

            ToggleSmartScreenApps.IsOn = true;
            defenderPatches.SetSmartScreenApps(true);

            ToggleTamperProtection.IsOn = true;
            defenderPatches.SetTamperProtection(true);
        }

        private void ApplyGamingPreset()
        {
            // Real-Time Protection - Reduced
            ToggleRealTimeProtection.IsOn = true;
            defenderPatches.SetRealTimeProtection(true);

            ToggleBehaviorMonitoring.IsOn = false;
            defenderPatches.SetBehaviorMonitoring(false);

            ToggleOnAccessProtection.IsOn = true;
            defenderPatches.SetOnAccessProtection(true);

            ToggleScriptScanning.IsOn = false;
            defenderPatches.SetScriptScanning(false);

            // Cloud Protection - Minimal
            ToggleCloudProtection.IsOn = false;
            defenderPatches.SetCloudProtection(false);

            ToggleAutomaticSampleSubmission.IsOn = false;
            defenderPatches.SetAutomaticSampleSubmission(false);

            ComboCloudBlockLevel.SelectedIndex = 0;
            defenderPatches.SetCloudBlockLevel(0);

            // Scanning - Selective
            ToggleArchiveScanning.IsOn = false;
            defenderPatches.SetArchiveScanning(false);

            ToggleEmailScanning.IsOn = false;
            defenderPatches.SetEmailScanning(false);

            ToggleRemovableDriveScanning.IsOn = true;
            defenderPatches.SetRemovableDriveScanning(true);

            ToggleNetworkScanning.IsOn = false;
            defenderPatches.SetNetworkScanning(false);

            // Additional Protection
            ToggleControlledFolderAccess.IsOn = false;
            defenderPatches.SetControlledFolderAccess(false);

            TogglePUAProtection.IsOn = false;
            defenderPatches.SetPUAProtection(false);

            ToggleNetworkProtection.IsOn = false;
            defenderPatches.SetNetworkProtection(false);

            ToggleExploitProtection.IsOn = true;
            defenderPatches.SetExploitProtection(true);

            // Windows Security
            ToggleSmartScreen.IsOn = false;
            defenderPatches.SetSmartScreen(false);

            ToggleSmartScreenApps.IsOn = false;
            defenderPatches.SetSmartScreenApps(false);

            ToggleTamperProtection.IsOn = false;
            defenderPatches.SetTamperProtection(false);
        }

        private void ApplyMinimalPreset()
        {
            // Disable most features
            ToggleRealTimeProtection.IsOn = false;
            defenderPatches.SetRealTimeProtection(false);

            ToggleBehaviorMonitoring.IsOn = false;
            defenderPatches.SetBehaviorMonitoring(false);

            ToggleOnAccessProtection.IsOn = false;
            defenderPatches.SetOnAccessProtection(false);

            ToggleScriptScanning.IsOn = false;
            defenderPatches.SetScriptScanning(false);

            ToggleCloudProtection.IsOn = false;
            defenderPatches.SetCloudProtection(false);

            ToggleAutomaticSampleSubmission.IsOn = false;
            defenderPatches.SetAutomaticSampleSubmission(false);

            ToggleArchiveScanning.IsOn = false;
            defenderPatches.SetArchiveScanning(false);

            ToggleEmailScanning.IsOn = false;
            defenderPatches.SetEmailScanning(false);

            ToggleRemovableDriveScanning.IsOn = false;
            defenderPatches.SetRemovableDriveScanning(false);

            ToggleNetworkScanning.IsOn = false;
            defenderPatches.SetNetworkScanning(false);

            ToggleControlledFolderAccess.IsOn = false;
            defenderPatches.SetControlledFolderAccess(false);

            TogglePUAProtection.IsOn = false;
            defenderPatches.SetPUAProtection(false);

            ToggleNetworkProtection.IsOn = false;
            defenderPatches.SetNetworkProtection(false);

            ToggleExploitProtection.IsOn = false;
            defenderPatches.SetExploitProtection(false);

            ToggleSmartScreen.IsOn = false;
            defenderPatches.SetSmartScreen(false);

            ToggleSmartScreenApps.IsOn = false;
            defenderPatches.SetSmartScreenApps(false);

            ToggleTamperProtection.IsOn = false;
            defenderPatches.SetTamperProtection(false);
        }

        #endregion

        #region Exclusions Management

        private void BtnAddFileExclusion_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select File to Exclude";
            dialog.Filter = "All Files (*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    defenderPatches.AddFileExclusion(dialog.FileName);
                    ListFileExclusions.Items.Add(dialog.FileName);
                    MessageBox.Show("File exclusion added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding exclusion: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnAddFolderExclusion_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void BtnRemoveFileExclusion_Click(object sender, RoutedEventArgs e)
        {
            if (ListFileExclusions.SelectedItem != null)
            {
                try
                {
                    string path = ListFileExclusions.SelectedItem.ToString();
                    defenderPatches.RemoveFileExclusion(path);
                    ListFileExclusions.Items.Remove(ListFileExclusions.SelectedItem);
                    MessageBox.Show("Exclusion removed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error removing exclusion: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an exclusion to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAddExtensionExclusion_Click(object sender, RoutedEventArgs e)
        {
            string extension = TxtFileExtension.Text.Trim();

            if (string.IsNullOrEmpty(extension))
            {
                MessageBox.Show("Please enter a file extension.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            try
            {
                defenderPatches.AddExtensionExclusion(extension);
                ListExtensionExclusions.Items.Add(extension);
                TxtFileExtension.Clear();
                MessageBox.Show("Extension exclusion added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding exclusion: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemoveExtensionExclusion_Click(object sender, RoutedEventArgs e)
        {
            if (ListExtensionExclusions.SelectedItem != null)
            {
                try
                {
                    string extension = ListExtensionExclusions.SelectedItem.ToString();
                    defenderPatches.RemoveExtensionExclusion(extension);
                    ListExtensionExclusions.Items.Remove(ListExtensionExclusions.SelectedItem);
                    MessageBox.Show("Exclusion removed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error removing exclusion: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an exclusion to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAddProcessExclusion_Click(object sender, RoutedEventArgs e)
        {
            string processName = TxtProcessName.Text.Trim();

            if (string.IsNullOrEmpty(processName))
            {
                MessageBox.Show("Please enter a process name.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                defenderPatches.AddProcessExclusion(processName);
                ListProcessExclusions.Items.Add(processName);
                TxtProcessName.Clear();
                MessageBox.Show("Process exclusion added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding exclusion: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemoveProcessExclusion_Click(object sender, RoutedEventArgs e)
        {
            if (ListProcessExclusions.SelectedItem != null)
            {
                try
                {
                    string processName = ListProcessExclusions.SelectedItem.ToString();
                    defenderPatches.RemoveProcessExclusion(processName);
                    ListProcessExclusions.Items.Remove(ListProcessExclusions.SelectedItem);
                    MessageBox.Show("Exclusion removed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error removing exclusion: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an exclusion to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Action Buttons

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadCurrentSettings();
            MessageBox.Show("Settings refreshed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Apply current Defender settings?\n\nThis will modify Windows Defender configuration.",
                "Apply Settings",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ApplyAllSettings();
                    MessageBox.Show("Settings applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error applying settings: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnOpenWindowsSecurity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("windowsdefender://threat");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Windows Security: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Settings Management

        private void LoadCurrentSettings()
        {
            try
            {
                // Real-Time Protection
                ToggleRealTimeProtection.IsOn = defenderPatches.GetRealTimeProtection();
                ToggleBehaviorMonitoring.IsOn = defenderPatches.GetBehaviorMonitoring();
                ToggleOnAccessProtection.IsOn = defenderPatches.GetOnAccessProtection();
                ToggleScriptScanning.IsOn = defenderPatches.GetScriptScanning();

                // Cloud Protection
                ToggleCloudProtection.IsOn = defenderPatches.GetCloudProtection();
                ToggleAutomaticSampleSubmission.IsOn = defenderPatches.GetAutomaticSampleSubmission();
                ComboCloudBlockLevel.SelectedIndex = defenderPatches.GetCloudBlockLevel();

                // Scanning
                ToggleArchiveScanning.IsOn = defenderPatches.GetArchiveScanning();
                ToggleEmailScanning.IsOn = defenderPatches.GetEmailScanning();
                ToggleRemovableDriveScanning.IsOn = defenderPatches.GetRemovableDriveScanning();
                ToggleNetworkScanning.IsOn = defenderPatches.GetNetworkScanning();

                // Additional Protection
                ToggleControlledFolderAccess.IsOn = defenderPatches.GetControlledFolderAccess();
                TogglePUAProtection.IsOn = defenderPatches.GetPUAProtection();
                ToggleNetworkProtection.IsOn = defenderPatches.GetNetworkProtection();
                ToggleExploitProtection.IsOn = defenderPatches.GetExploitProtection();

                // Windows Security
                ToggleSmartScreen.IsOn = defenderPatches.GetSmartScreen();
                ToggleSmartScreenApps.IsOn = defenderPatches.GetSmartScreenApps();
                ToggleTamperProtection.IsOn = defenderPatches.GetTamperProtection();

                // Load Exclusions
                LoadExclusions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading settings: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyAllSettings()
        {
            // Real-Time Protection
            defenderPatches.SetRealTimeProtection(ToggleRealTimeProtection.IsOn);
            defenderPatches.SetBehaviorMonitoring(ToggleBehaviorMonitoring.IsOn);
            defenderPatches.SetOnAccessProtection(ToggleOnAccessProtection.IsOn);
            defenderPatches.SetScriptScanning(ToggleScriptScanning.IsOn);

            // Cloud Protection
            defenderPatches.SetCloudProtection(ToggleCloudProtection.IsOn);
            defenderPatches.SetAutomaticSampleSubmission(ToggleAutomaticSampleSubmission.IsOn);
            defenderPatches.SetCloudBlockLevel(ComboCloudBlockLevel.SelectedIndex);

            // Scanning
            defenderPatches.SetArchiveScanning(ToggleArchiveScanning.IsOn);
            defenderPatches.SetEmailScanning(ToggleEmailScanning.IsOn);
            defenderPatches.SetRemovableDriveScanning(ToggleRemovableDriveScanning.IsOn);
            defenderPatches.SetNetworkScanning(ToggleNetworkScanning.IsOn);

            // Additional Protection
            defenderPatches.SetControlledFolderAccess(ToggleControlledFolderAccess.IsOn);
            defenderPatches.SetPUAProtection(TogglePUAProtection.IsOn);
            defenderPatches.SetNetworkProtection(ToggleNetworkProtection.IsOn);
            defenderPatches.SetExploitProtection(ToggleExploitProtection.IsOn);

            // Windows Security
            defenderPatches.SetSmartScreen(ToggleSmartScreen.IsOn);
            defenderPatches.SetSmartScreenApps(ToggleSmartScreenApps.IsOn);
            defenderPatches.SetTamperProtection(ToggleTamperProtection.IsOn);
        }

        private void LoadExclusions()
        {
            // Clear existing
            ListFileExclusions.Items.Clear();
            ListExtensionExclusions.Items.Clear();
            ListProcessExclusions.Items.Clear();

            // Load from registry/PowerShell
            var fileExclusions = defenderPatches.GetFileExclusions();
            foreach (var exclusion in fileExclusions)
            {
                ListFileExclusions.Items.Add(exclusion);
            }

            var extensionExclusions = defenderPatches.GetExtensionExclusions();
            foreach (var exclusion in extensionExclusions)
            {
                ListExtensionExclusions.Items.Add(exclusion);
            }

            var processExclusions = defenderPatches.GetProcessExclusions();
            foreach (var exclusion in processExclusions)
            {
                ListProcessExclusions.Items.Add(exclusion);
            }
        }

        #endregion
    }
}
