using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SecVers_Debloat.Helpers;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace SecVers_Debloat.UI.Pages
{
    public partial class SoftwareInstallerPage : Page
    {
        private WingetHelper _wingetHelper;
        private bool _isInstalling = false;

        public SoftwareInstallerPage()
        {
            InitializeComponent();
            Loaded += SoftwareInstallerPage_Loaded;
        }

        private void SoftwareInstallerPage_Loaded(object sender, RoutedEventArgs e)
        {
            CheckWingetStatus();
        }

        private async void CheckWingetStatus()
        {
            // Set visuals to loading
            StatusIcon.Text = "⏳";
            InfoText.Text = "Checking Winget availability...";

            try
            {
                await Task.Run(() =>
                {
                    try { _wingetHelper = new WingetHelper(); }
                    catch { _wingetHelper = null; }
                });

                if (_wingetHelper != null && _wingetHelper.IsWingetAvailable)
                {
                    StatusBorder.Background = new SolidColorBrush(Color.FromRgb(223, 246, 221)); // Success Green
                    StatusBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(16, 124, 16));
                    StatusIcon.Text = "✅";
                    StatusText.Text = "Winget is ready.";
                    StatusText.Foreground = new SolidColorBrush(Color.FromRgb(16, 124, 16));
                    BtnInstall.IsEnabled = true;
                    InfoText.Text = "Ready to install.";
                }
                else
                {
                    throw new Exception("Check failed.");
                }
            }
            catch
            {
                StatusBorder.Background = new SolidColorBrush(Color.FromRgb(253, 231, 233)); // Error Red
                StatusBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(209, 52, 56));
                StatusIcon.Text = "❌";
                StatusText.Text = "Winget Missing / Error";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(164, 38, 44));
                BtnInstall.IsEnabled = false;
                InfoText.Text = "Error: Winget is required.";
            }
        }

        private void HeaderCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox headerCk && headerCk.Tag is string panelName)
            {
                var targetPanel = this.FindName(panelName) as StackPanel;
                if (targetPanel != null)
                {
                    bool isChecked = headerCk.IsChecked ?? false;
                    foreach (var child in targetPanel.Children)
                    {
                        if (child is CheckBox itemCk)
                            itemCk.IsChecked = isChecked;
                    }
                }
            }
        }

        private async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            if (_isInstalling || _wingetHelper == null) return;

            List<string> packagesToInstall = GetAllSelectedPackages();

            if (packagesToInstall.Count == 0)
            {
                MessageBox.Show("Please select at least one application.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

           
            _isInstalling = true;
            BtnInstall.IsEnabled = false;
            InstallProgress.Visibility = Visibility.Visible;
            StatusText.Text = "Installing...";

            try
            {
                string[] packageArray = packagesToInstall.ToArray();
                InfoText.Text = $"Installing {packageArray.Length} applications...";
                int successCount = await Task.Run(() => _wingetHelper.InstallPackagesAsync(packageArray, silent: true));
                MessageBox.Show($"Installation Finished.\nSuccess: {successCount} / {packageArray.Length}", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                InfoText.Text = "Done.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Installation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                InfoText.Text = "Failed.";
            }
            finally
            {
                _isInstalling = false;
                BtnInstall.IsEnabled = true;
                InstallProgress.Visibility = Visibility.Collapsed;
                StatusText.Text = "Winget is ready.";
            }
        }


        private List<string> GetAllSelectedPackages()
        {
            List<string> list = new List<string>();

            if (MainContainer == null) return list;

            foreach (var child in MainContainer.Children)
            {

                if (child is Expander exp && exp.Content is StackPanel innerPanel)
                {
                    foreach (var innerChild in innerPanel.Children)
                    {
                        if (innerChild is CheckBox ck && ck.IsChecked == true && ck.Tag != null)
                        {
                            string id = ck.Tag.ToString();
                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                list.Add(id);
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
