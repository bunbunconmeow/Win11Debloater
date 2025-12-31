using Hardware.Info;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace SecVers_Debloat.UI.Pages
{
    public partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            InitializeComponent();
            LoadSystemInfo();
            _ = CheckRestorePointStatusAsync();
        }

        private void LoadSystemInfo()
        {
            try
            {
                var hardwareInfo = new HardwareInfo();
                hardwareInfo.RefreshOperatingSystem();
                TxtOSVersion.Text = hardwareInfo.OperatingSystem.Name;
                TxtBuildNumber.Text = hardwareInfo.OperatingSystem.VersionString;
                TxtArchitecture.Text = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            }
            catch
            {
                TxtOSVersion.Text = "Unable to detect";
                TxtBuildNumber.Text = "N/A";
                TxtArchitecture.Text = "N/A";
            }
        }

        private void BtnQuickStart_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will run a preset configuration. Create a restore point first!\n\nContinue?",
                "Quick Start",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NavigationService?.Navigate(new DebloatPage()); 
            }
        }

        private void BtnDocumentation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/bunbunconmeow/Win11Debloater/wiki",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open documentation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CheckRestorePointStatusAsync()
        {
            string statusText = "Checking...";

            await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ShadowCopy"))
                    {
                        int count = 0;
                        foreach (var obj in searcher.Get()) count++;

                        if (count > 0)
                            statusText = $"System Protection enabled ({count} points found)";
                        else
                            statusText = "No restore points found.";
                    }
                }
                catch
                {
                    statusText = "Could not check System Restore status.";
                }
            });

            TxtRestorePointStatus.Text = statusText;
        }

        private async void BtnCreateRestorePoint_Click(object sender, RoutedEventArgs e)
        {
            BtnCreateRestorePoint.IsEnabled = false;
            BtnCreateRestorePoint.Content = "Creating...";

            try
            {
                await Task.Run(() => CreateRestorePointLogic());
                MessageBox.Show(
                    "Restore point created successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await CheckRestorePointStatusAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to create restore point.\n\nError Details:\n{ex.Message}\n\n" +
                    "Note: If you are running this in a VM (Virtual Machine), System Restore might be disabled by the hyervisor.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                BtnCreateRestorePoint.IsEnabled = true;
                BtnCreateRestorePoint.Content = "Create Now";
            }
        }

        private void CreateRestorePointLogic()
        {

            try
            {
                string regPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue("SystemRestorePointCreationFrequency", 0, RegistryValueKind.DWord);
                    }
                }
            }
            catch
            {
               
            }


            var scope = new ManagementScope("\\\\localhost\\root\\default");
            var path = new ManagementPath("SystemRestore");
            var options = new ObjectGetOptions();

            using (var restoreClass = new ManagementClass(scope, path, options))
            {
                var parameters = restoreClass.GetMethodParameters("CreateRestorePoint");
                string description = $"SecVers Debloat - {DateTime.Now:yyyy-MM-dd HH:mm}";

                parameters["Description"] = description;
                parameters["RestorePointType"] = 12; 
                parameters["EventType"] = 100;

                var result = restoreClass.InvokeMethod("CreateRestorePoint", parameters, null);


                int statusCode = 0;
                if (result != null && result["ReturnValue"] != null)
                {
                    statusCode = Convert.ToInt32(result["ReturnValue"]);
                }

                if (statusCode != 0)
                {
                    throw new Exception($"WMI Error Code: {statusCode} (The system refused to create a restore point).");
                }
            }
        }
    }
}
