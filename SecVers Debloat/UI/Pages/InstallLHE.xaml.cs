using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace SecVers_Debloat.UI.Pages
{
    public partial class InstallLHE : Page
    {
        private const string InstallFolder = @"C:\Program Files\LHE";
        private const string ExecutableName = "LHE.exe";
        private const string ProcessNameWithoutExtension = "LHE"; 
        private const string DownloadUrl =
            "https://api.secvers.org/v1/downloads/lhe";

        private const string TaskName = "LHE_Service";

        public InstallLHE()
        {
            InitializeComponent();
            Loaded += InstallLHE_Loaded;
        }

        private void InstallLHE_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshStatus();
        }

        private string GetInstallPath() =>
            Path.Combine(InstallFolder, ExecutableName);

        private void RefreshStatus()
        {
            try
            {
                string path = GetInstallPath();
                bool installed = File.Exists(path);

                TxtStatus.Text = installed ? "Installed" : "Not installed";
                TxtInstallPath.Text = path;

                bool autoStartEnabled = IsTaskExisting(TaskName);
                TxtAutostart.Text = autoStartEnabled
                    ? "Enabled (Scheduled Task)"
                    : "Disabled";

                BtnUninstall.IsEnabled = installed;
                BtnOpenFolder.IsEnabled = Directory.Exists(InstallFolder);
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Error checking status";
                TxtOperationStatus.Text = $"Error: {ex.Message}";
            }
        }

        #region Scheduled Task helper methods

        private bool IsTaskExisting(string taskName)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/Query /TN \"{taskName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    proc.WaitForExit(5000); 
                    return proc.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }


        private void ConfigureAutostartScheduledTask(string exePath)
        {
            string arguments =
                $"/Create /TN \"{TaskName}\" " +
                $"/TR \"\\\"{exePath}\\\"\" " +
                "/SC ONLOGON " +
                "/RL HIGHEST " +
                "/RU SYSTEM " +
                "/F";

            RunSchtasks(arguments, "creating scheduled task");
        }

        private void RemoveAutostartScheduledTask()
        {
            string arguments = $"/Delete /TN \"{TaskName}\" /F";
            RunSchtasks(arguments, "deleting scheduled task");
        }

     
        private void RunSchtasks(string arguments, string operationDescription)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using (var proc = Process.Start(psi))
            {
                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Error {operationDescription} (exit code {proc.ExitCode}): {error}\n{output}");
                }
            }
        }

        #endregion

        private async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            BtnInstall.IsEnabled = false;
            BtnUninstall.IsEnabled = false;
            BtnOpenFolder.IsEnabled = false;
            ProgressInstall.Visibility = Visibility.Visible;
            ProgressInstall.IsIndeterminate = true;
            TxtOperationStatus.Text = "Downloading LHE executable from GitHub...";

            try
            {
                string tempFile = Path.GetTempFileName();

                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromMinutes(5);

                    using (var response = await http.GetAsync(DownloadUrl))
                    {
                        response.EnsureSuccessStatusCode();

                        using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await response.Content.CopyToAsync(fs);
                        }
                    }
                }

                TxtOperationStatus.Text = "Download completed. Installing...";

                // Ensure install folder exists
                Directory.CreateDirectory(InstallFolder);

                string targetPath = GetInstallPath();

                // If already exists, try to warn if process is running
                if (File.Exists(targetPath))
                {
                    TxtOperationStatus.Text = "Existing installation detected. Attempting safe replacement...";

                    var processes = Process.GetProcessesByName(ProcessNameWithoutExtension);
                    if (processes.Length > 0)
                    {
                        MessageBox.Show(
                            "The LHE process appears to be running.\n\n" +
                            "To avoid potential system instability, close it from within the LHE application " +
                            "before continuing.",
                            "LHE Running",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }

                File.Copy(tempFile, targetPath, true);
                File.SetAttributes(targetPath, FileAttributes.Normal);

                TxtOperationStatus.Text = "Configuring autostart via Scheduled Task (requires administrator rights)...";
                ConfigureAutostartScheduledTask(targetPath);

                TxtOperationStatus.Text = "Installation completed successfully.";

                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = targetPath,
                        Verb = "runas",
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    TxtOperationStatus.Text += $"\nNote: Could not start application as admin: {ex.Message}";
                }
                MessageBox.Show(
                    "LHE has been installed successfully.\n" +
                    "It will start automatically with Windows using a Scheduled Task (no UAC prompt).",
                    "LHE Installed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

             
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Installation failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                TxtOperationStatus.Text = $"Installation failed: {ex.Message}";
            }
            finally
            {
                ProgressInstall.IsIndeterminate = false;
                ProgressInstall.Visibility = Visibility.Collapsed;

                BtnInstall.IsEnabled = true;
                BtnUninstall.IsEnabled = true;
                BtnOpenFolder.IsEnabled = true;

                RefreshStatus();
            }
        }

        private void BtnUninstall_Click(object sender, RoutedEventArgs e)
        {
            string path = GetInstallPath();

            if (!File.Exists(path))
            {
                MessageBox.Show("LHE is not installed at the configured path.", "Not Installed",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshStatus();
                return;
            }

            // Check if process is running
            var processes = Process.GetProcessesByName(ProcessNameWithoutExtension);
            if (processes.Length > 0)
            {
                var result = MessageBox.Show(
                    "The LHE process appears to be running.\n\n" +
                    "According to the specification, this component may be critical and could cause " +
                    "system instability or a blue screen if removed while active.\n\n" +
                    "Make sure you have properly stopped LHE from within its own UI before continuing.\n\n" +
                    "Do you still want to proceed with uninstall?",
                    "LHE Running - Proceed with Caution",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                BtnInstall.IsEnabled = false;
                BtnUninstall.IsEnabled = false;
                BtnOpenFolder.IsEnabled = false;

                TxtOperationStatus.Text = "Uninstalling LHE...";
                ProgressInstall.Visibility = Visibility.Visible;
                ProgressInstall.IsIndeterminate = true;

                // Remove scheduled task
                if (IsTaskExisting(TaskName))
                {
                    RemoveAutostartScheduledTask();
                }

                // Try to delete the file
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                // Try to delete the folder if empty
                if (Directory.Exists(InstallFolder) &&
                    Directory.GetFiles(InstallFolder).Length == 0 &&
                    Directory.GetDirectories(InstallFolder).Length == 0)
                {
                    Directory.Delete(InstallFolder);
                }

                TxtOperationStatus.Text = "Uninstall completed.";
                MessageBox.Show(
                    "LHE has been uninstalled.\n" +
                    "If the process was still running, please reboot your system.",
                    "Uninstall Completed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Uninstall failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                TxtOperationStatus.Text = $"Uninstall failed: {ex.Message}";
            }
            finally
            {
                ProgressInstall.IsIndeterminate = false;
                ProgressInstall.Visibility = Visibility.Collapsed;

                BtnInstall.IsEnabled = true;
                BtnUninstall.IsEnabled = true;
                BtnOpenFolder.IsEnabled = true;

                RefreshStatus();
            }
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Directory.Exists(InstallFolder))
                {
                    MessageBox.Show("The installation folder does not exist yet.", "Folder Not Found",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{InstallFolder}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open folder:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
