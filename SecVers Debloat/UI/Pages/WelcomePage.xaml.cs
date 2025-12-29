using Hardware.Info;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
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
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace SecVers_Debloat.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {
      
        public WelcomePage()
        {
            InitializeComponent();
            LoadSystemInfo();
        }
        private void LoadSystemInfo()
        {
            try
            {
                var hardwareInfo = new HardwareInfo();
                hardwareInfo.RefreshOperatingSystem();
                TxtOSVersion.Text = hardwareInfo.OperatingSystem.Name.ToString();

                TxtBuildNumber.Text = hardwareInfo.OperatingSystem.VersionString.ToString();
                TxtArchitecture.Text = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            }
            catch (Exception ex)
            {

                TxtOSVersion.Text = "Unable to detect";
                TxtBuildNumber.Text = "N/A";
                TxtArchitecture.Text = "N/A";
            }
        }
        private void BtnQuickStart_Click(object sender, RoutedEventArgs e)
        {
            // Show confirmation dialog
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
                MessageBox.Show($"Could not open documentation: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void CheckRestorePointStatus()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ShadowCopy"))
                {
                    var count = 0;
                    foreach (var obj in searcher.Get())
                        count++;

                    if (count > 0)
                    {
                        TxtRestorePointStatus.Text = $"System Protection enabled ({count} restore points available)";
                    }
                    else
                    {
                        TxtRestorePointStatus.Text = "No restore points found. Create one now for safety.";
                    }
                }
            }
            catch
            {
                TxtRestorePointStatus.Text = "Create a restore point before making changes";
            }
        }

        private async void BtnCreateRestorePoint_Click(object sender, RoutedEventArgs e)
        {
            BtnCreateRestorePoint.IsEnabled = false;
            BtnCreateRestorePoint.Content = "Creating...";

            try
            {
                await Task.Run(() => CreateRestorePoint());

                MessageBox.Show(
                    "Restore point created successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                CheckRestorePointStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to create restore point:\n\n{ex.Message}\n\nMake sure:\n• You have administrator rights\n• System Protection is enabled\n• Your C: drive has enough space",
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

        private void CreateRestorePoint()
        {
            try
            {
                var scope = new ManagementScope("\\\\localhost\\root\\default");
                var path = new ManagementPath("SystemRestore");
                var options = new ObjectGetOptions();

                using (var mClass = new ManagementClass(scope, path, options))
                {
                    var inParams = mClass.GetMethodParameters("CreateRestorePoint");
                    inParams["Description"] = $"SecVers Debloat - {DateTime.Now:yyyy-MM-dd HH:mm}";
                    inParams["RestorePointType"] = 12; 
                    inParams["EventType"] = 100; 

                    var outParams = mClass.InvokeMethod("CreateRestorePoint", inParams, null);

                    if (outParams == null)
                        throw new Exception("Failed to invoke CreateRestorePoint method");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Checkpoint-Computer -Description 'SecVers Debloat - {DateTime.Now:yyyy-MM-dd HH:mm}' -RestorePointType MODIFY_SETTINGS\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        Verb = "runas"
                    };

                    using (var process = Process.Start(psi))
                    {
                        process.WaitForExit(30000);

                        if (process.ExitCode != 0)
                        {
                            var error = process.StandardError.ReadToEnd();
                            throw new Exception($"PowerShell execution failed: {error}");
                        }
                    }
                }
                catch (Exception psEx)
                {
                    throw new Exception($"Primary method failed: {ex.Message}\nFallback method failed: {psEx.Message}");
                }
            }
        }
    }
}
