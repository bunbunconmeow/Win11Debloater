using iNKORE.UI.WPF.Modern.Controls;
using SecVers_Debloat.Patches;
using SecVers_Debloat.Patches.Debloater;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace SecVers_Debloat.UI.Pages
{
    public partial class DebloatPage : System.Windows.Controls.Page
    {
        private List<InstalledApp> _allApps;
        private ICollectionView _appsView; // Für das Filtern

        public DebloatPage()
        {
            InitializeComponent();
            Loaded += DebloatPage_Loaded;
        }

        private void DebloatPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_allApps == null)
            {
                BtnRefreshApps_Click(null, null);
            }
        }

        private async void BtnRefreshApps_Click(object sender, RoutedEventArgs e)
        {
            SetLoading(true); 

            await Task.Run(() =>
            {
                _allApps = AppManager.GetInstalledApps();
            });


            DgInstalledApps.ItemsSource = _allApps;
            SetLoading(false); 
        }

        private bool FilterApps(object item)
        {
            if (string.IsNullOrWhiteSpace(TxtSearch.Text)) return true;

            var app = item as InstalledApp;
            return (app.DisplayName?.IndexOf(TxtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (app.Publisher?.IndexOf(TxtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allApps == null) return;

            string filter = TxtSearch.Text.ToLower();

            var filtered = _allApps.Where(app =>
                (app.DisplayName != null && app.DisplayName.ToLower().Contains(filter)) ||
                (app.Publisher != null && app.Publisher.ToLower().Contains(filter))
            ).ToList();

            DgInstalledApps.ItemsSource = filtered;
        }

        private async void BtnUninstallSelected_Click(object sender, RoutedEventArgs e)
        {
            if (_allApps == null) return;
            var selected = _allApps.Where(a => a.IsSelected).ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show("Please select at least one application to uninstall.", "Info");
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to FORCE remove {selected.Count} apps?\n\nThis will terminate processes and delete files immediately.",
                                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                SetLoading(true);
                string logs = "";

                await Task.Run(() =>
                {
                    foreach (var app in selected)
                    {
                       
                        logs += AppManager.UninstallApp(app) + "\n------------------\n";
                    }
                });

                SetLoading(false);

                BtnRefreshApps_Click(null, null);

                if (MessageBox.Show("Operation complete. Show logs?", "Done", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    MessageBox.Show(logs, "Execution Logs");
                }
            }
        }

        private async void BtnExecuteCore_Click(object sender, RoutedEventArgs e)
        {
            bool edge = ChkEdge.IsChecked == true;
            bool oneDrive = ChkOneDrive.IsChecked == true;
            bool cortana = ChkCortana.IsChecked == true;

            if (!edge && !oneDrive && !cortana)
            {
                MessageBox.Show("Please select at least one component to remove.", "Selection Required");
                return;
            }

            var confirm = MessageBox.Show("Warning: Removing core components can lead to issues with dependent applications.\n\nContinue?",
                                          "Core Removal", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            if (confirm == MessageBoxResult.Yes)
            {
                SetLoading(true);
                string results = "";

                await Task.Run(() =>
                {
                    if (edge)
                    {
                        try { SpecialAppRemover.ForceRemoveEdge(); results += "Edge: Removed successfully.\n"; }
                        catch (Exception ex) { results += $"Edge: Error - {ex.Message}\n"; }
                    }

                    if (oneDrive)
                    {
                        try { SpecialAppRemover.ForceRemoveOneDrive(); results += "OneDrive: Removed successfully.\n"; }
                        catch (Exception ex) { results += $"OneDrive: Error - {ex.Message}\n"; }
                    }

                    if (cortana)
                    {
                        try { SpecialAppRemover.ForceRemoveCortana(); results += "Cortana: Removed successfully.\n"; }
                        catch (Exception ex) { results += $"Cortana: Error - {ex.Message}\n"; }
                    }
                });

                SetLoading(false);
                MessageBox.Show(results, "Execution Result");
                ChkEdge.IsChecked = false;
                ChkOneDrive.IsChecked = false;
                ChkCortana.IsChecked = false;
            }
        }


        private void SetLoading(bool isLoading)
        {
            LoadingLayer.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            BtnRefresh.IsEnabled = !isLoading;
            BtnUninstall.IsEnabled = !isLoading;
            DgInstalledApps.IsEnabled = !isLoading;
        }
    }
}
