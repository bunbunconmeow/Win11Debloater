using Microsoft.Win32;
using SecVers_Debloat.Patches;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
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
    /// Interaktionslogik für AnonymizerPage.xaml
    /// </summary>
    public partial class AnonymizerPage : Page
    {
        private readonly SystemDataAnonymizer _anonymizer;

        public AnonymizerPage()
        {
            InitializeComponent();
            _anonymizer = new SystemDataAnonymizer();
        }

        // ================= BUTTON ACTIONS =================

        private void BtnApplySelected_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to randomize the selected identifiers?\n\nThis may affect software licenses.",
                                "Confirm Anonymization",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                // Network
                if (ChkRandomizeHostname.IsChecked == true) _anonymizer.RandomizeComputerName();
                if (ChkRandomizeMAC.IsChecked == true) _anonymizer.RandomizeAllMACAddresses();
                // if (ChkDisableIPv6.IsChecked == true) ... add logic if available in your class, or generic reg tweak

                // Hardware IDs
                if (ChkRandomizeMachineGUID.IsChecked == true) _anonymizer.RandomizeMachineGUID();
                if (ChkRandomizeBaseboard.IsChecked == true) _anonymizer.RandomizeBaseboardSerial();
                if (ChkSpoofProductInfo.IsChecked == true)
                {
                    _anonymizer.SpoofSystemProductName("System Product Name"); // Or random string
                    _anonymizer.SpoofSystemManufacturer("System Manufacturer");
                }
                if (ChkRandomizeBIOS.IsChecked == true) _anonymizer.RandomizeBIOSSerial();
                if (ChkRandomizeInstallID.IsChecked == true) _anonymizer.RandomizeInstallationID();
                if (ChkRandomizeProductID.IsChecked == true) _anonymizer.RandomizeProductID();
                if (ChkSpoofGPU.IsChecked == true) _anonymizer.SpoofGPUDeviceID("PCI\\VEN_10DE&DEV_1C03&SUBSYS_1C0310DE"); // Example generic ID

                // Storage
                if (ChkSpoofDiskSerial.IsChecked == true) _anonymizer.SpoofDiskSerial("S1D5-" + Guid.NewGuid().ToString().Substring(0, 8));
                if (ChkChangeVolumeSerial.IsChecked == true) _anonymizer.ChangeVolumeSerial("C", "1234-5678"); // Ideally implement a random hex generator here

                // Tracking
                if (ChkResetAdID.IsChecked == true) _anonymizer.DisableAndClearAdvertisingID();
                if (ChkClearTelemetryID.IsChecked == true) _anonymizer.ClearTelemetryID();
                if (ChkRandomizeOwner.IsChecked == true)
                {
                    _anonymizer.RandomizeRegisteredOwner();
                    _anonymizer.RandomizeRegisteredOrganization();
                }
                if (ChkRandomizeInstallDate.IsChecked == true) _anonymizer.RandomizeInstallDate();

                MessageBox.Show("Selected operations completed successfully.\nPlease restart your computer.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAnonymizeAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("WARNING: This will randomize EVERYTHING (Hostname, GUIDs, MACs, etc).\n\nThis is a destructive action for identities. Continue?",
                                "TOTAL ANONYMIZATION",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Stop) == MessageBoxResult.Yes)
            {
                try
                {
                    _anonymizer.AnonymizeEverything();
                    MessageBox.Show("System Anonymized. Restart immediately.", "Done", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }


        // Handles the "Select All" checkbox in the headers
        private void HeaderCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox headerBox)
            {
                // Determine which group we are in based on Name or mapping
                string tagToFind = "";
                if (headerBox == NetworkAllCheckBox) tagToFind = "Network";
                else if (headerBox == HwidAllCheckBox) tagToFind = "HWID";
                else if (headerBox == StorageAllCheckBox) tagToFind = "Storage";
                else if (headerBox == TrackAllCheckBox) tagToFind = "Track";

                if (string.IsNullOrEmpty(tagToFind)) return;

                bool newVal = headerBox.IsChecked ?? false;


                SetCheckboxesByTag(tagToFind, newVal);
            }
        }

        private void SetCheckboxesByTag(string tag, bool isChecked)
        {
            // Simple visual tree traversal or just iterate over known names if list is small.
            // For robustness, let's look at the logical children of the Expander content panels.
            var allCheckBoxes = FindVisualChildren<CheckBox>(this);
            foreach (var box in allCheckBoxes)
            {
                if (box.Tag != null && box.Tag.ToString() == tag)
                {
                    box.IsChecked = isChecked;
                }
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}