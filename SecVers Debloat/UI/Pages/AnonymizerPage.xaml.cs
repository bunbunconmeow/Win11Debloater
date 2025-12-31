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
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace SecVers_Debloat.UI.Pages
{

    public partial class AnonymizerPage : Page
    {
        private readonly SystemDataAnonymizer _anonymizer;

        public AnonymizerPage()
        {
            InitializeComponent();
            _anonymizer = new SystemDataAnonymizer();
        }

        private async void BtnApplySelected_Click(object sender, RoutedEventArgs e)
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
                if (ChkRandomizeHostname.IsChecked == true) _anonymizer.RandomizeComputerName();
                if (ChkRandomizeMAC.IsChecked == true) _anonymizer.RandomizeAllMACAddresses();
                if (ChkRandomizeMachineGUID.IsChecked == true) _anonymizer.RandomizeMachineGUID();
                if (ChkRandomizeBaseboard.IsChecked == true) _anonymizer.RandomizeBaseboardSerial();
                if (ChkSpoofProductInfo.IsChecked == true)
                {

                    _anonymizer.SpoofSystemProductName(GenerateRandomString(10)); 
                    _anonymizer.SpoofSystemManufacturer(GenerateRandomString(10));
                }
                if (ChkRandomizeBIOS.IsChecked == true) _anonymizer.RandomizeBIOSSerial();
                if (ChkRandomizeInstallID.IsChecked == true) _anonymizer.RandomizeInstallationID();
                if (ChkRandomizeProductID.IsChecked == true) _anonymizer.RandomizeProductID();
                if (ChkSpoofGPU.IsChecked == true) _anonymizer.SpoofGPUDeviceID("PCI\\VEN_10DE&DEV_1C03&SUBSYS_1C0310DE"); 
                if (ChkSpoofDiskSerial.IsChecked == true) _anonymizer.SpoofDiskSerial("S1D5-" + Guid.NewGuid().ToString().Substring(0, 8));
                if (ChkChangeVolumeSerial.IsChecked == true) new Task(() => _anonymizer.ChangeVolumeSerial("C", GenerateRandomIntString(4) + "-" + GenerateRandomIntString(4)));

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


        private void HeaderCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox headerBox)
            {
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

        // Roandom String Generator
        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //Raodoom Int-string Generator
        private string GenerateRandomIntString(int length)
        {
            const string chars = "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}