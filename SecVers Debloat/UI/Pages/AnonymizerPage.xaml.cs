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
    /// Interaktionslogik für AnonymizerPage.xaml
    /// </summary>
    public partial class AnonymizerPage : Page
    {
        private readonly SystemDataAnonymizer _anonymizer;
        private readonly Dictionary<string, List<CheckBox>> _categoryCheckBoxes;

        public AnonymizerPage()
        {
            InitializeComponent();
            _anonymizer = new SystemDataAnonymizer();
            _categoryCheckBoxes = new Dictionary<string, List<CheckBox>>();
            InitializeCategoryCheckBoxes();
        }

        // ==================== INITIALIZATION ====================

        private void InitializeCategoryCheckBoxes()
        {
            _categoryCheckBoxes["NetworkIdentifiers"] = new List<CheckBox>
            {
                ChkRandomizeMAC,
                ChkRandomizeHostname,
                ChkRandomizeComputerName
            };

            _categoryCheckBoxes["SystemIdentifiers"] = new List<CheckBox>
            {
                ChkRandomizeMachineGUID,
                ChkRandomizeProductID,
                ChkRandomizeInstallationID,
                ChkRandomizeInstallDate,
                ChkRandomizeRegisteredOwner,
                ChkRandomizeRegisteredOrg
            };

            _categoryCheckBoxes["HardwareIdentifiers"] = new List<CheckBox>
            {
                ChkRandomizeBIOSSerial,
                ChkRandomizeMotherboardSerial,
                ChkRandomizeDiskSerial,
                ChkSpoofSystemProductName,
                ChkSpoofSystemManufacturer,
                ChkSpoofGPUDeviceID
            };

            _categoryCheckBoxes["PrivacyIdentifiers"] = new List<CheckBox>
            {
                ChkDisableAdvertisingID,
                ChkClearTelemetryID,
                ChkDisableWindowsID,
                ChkClearActivityHistory
            };

            _categoryCheckBoxes["AdvancedOperations"] = new List<CheckBox>
            {
                ChkClearTPM,
                ChkChangeVolumeSerial
            };
        }

        // ==================== HEADER CHECKBOX LOGIC ====================

        private void HeaderCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox headerCheckBox)
            {
                // Prevent infinite loop
                headerCheckBox.Click -= HeaderCheckBox_Click;

                // Determine which category
                string category = headerCheckBox.Name.Replace("AllCheckBox", "");

                if (_categoryCheckBoxes.ContainsKey(category))
                {
                    bool? isChecked = headerCheckBox.IsChecked;
                    foreach (var checkBox in _categoryCheckBoxes[category])
                    {
                        checkBox.IsChecked = isChecked ?? false;
                    }
                }

                headerCheckBox.Click += HeaderCheckBox_Click;
            }
        }

        // ==================== NETWORK IDENTIFIERS ====================

        private void NetworkIdentifiersOption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateHeaderCheckBox(NetworkIdentifiersAllCheckBox, _categoryCheckBoxes["NetworkIdentifiers"]);
        }

        private void NetworkIdentifiersOption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateHeaderCheckBox(NetworkIdentifiersAllCheckBox, _categoryCheckBoxes["NetworkIdentifiers"]);
        }

        // ==================== SYSTEM IDENTIFIERS ====================

        private void SystemIdentifiersOption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateHeaderCheckBox(SystemIdentifiersAllCheckBox, _categoryCheckBoxes["SystemIdentifiers"]);
        }

        private void SystemIdentifiersOption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateHeaderCheckBox(SystemIdentifiersAllCheckBox, _categoryCheckBoxes["SystemIdentifiers"]);
        }

        // ==================== HARDWARE IDENTIFIERS ====================

        private void HardwareIdentifiersOption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateHeaderCheckBox(HardwareIdentifiersAllCheckBox, _categoryCheckBoxes["HardwareIdentifiers"]);
        }

        private void HardwareIdentifiersOption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateHeaderCheckBox(HardwareIdentifiersAllCheckBox, _categoryCheckBoxes["HardwareIdentifiers"]);
        }

        // ==================== PRIVACY IDENTIFIERS ====================

        private void PrivacyIdentifiersOption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateHeaderCheckBox(PrivacyIdentifiersAllCheckBox, _categoryCheckBoxes["PrivacyIdentifiers"]);
        }

        private void PrivacyIdentifiersOption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateHeaderCheckBox(PrivacyIdentifiersAllCheckBox, _categoryCheckBoxes["PrivacyIdentifiers"]);
        }

        // ==================== ADVANCED OPERATIONS ====================

        private void AdvancedOperationsOption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateHeaderCheckBox(AdvancedOperationsAllCheckBox, _categoryCheckBoxes["AdvancedOperations"]);
        }

        private void AdvancedOperationsOption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateHeaderCheckBox(AdvancedOperationsAllCheckBox, _categoryCheckBoxes["AdvancedOperations"]);
        }

        // ==================== HELPER: UPDATE HEADER CHECKBOX ====================

        private void UpdateHeaderCheckBox(CheckBox headerCheckBox, List<CheckBox> checkBoxes)
        {
            if (headerCheckBox == null || checkBoxes == null) return;

            headerCheckBox.Click -= HeaderCheckBox_Click;

            int checkedCount = checkBoxes.Count(cb => cb.IsChecked == true);

            if (checkedCount == 0)
                headerCheckBox.IsChecked = false;
            else if (checkedCount == checkBoxes.Count)
                headerCheckBox.IsChecked = true;
            else
                headerCheckBox.IsChecked = null;

            headerCheckBox.Click += HeaderCheckBox_Click;
        }

        // ==================== PRESET BUTTONS ====================

        private void BtnPresetBasic_Click(object sender, RoutedEventArgs e)
        {
            // Basic: Only safe privacy options
            ResetAllCheckboxes();
            ChkDisableAdvertisingID.IsChecked = true;
            ChkClearTelemetryID.IsChecked = true;
            ChkRandomizeProductID.IsChecked = true;
            ChkClearActivityHistory.IsChecked = true;

            MessageBox.Show(
                "Basic Preset selected:\n\n" +
                "- Disable Advertising ID\n" +
                "- Clear Telemetry ID\n" +
                "- Disable Windows ID\n" +
                "- Clear Activity History\n\n" +
                "This preset is safe for most users.",
                "Basic Anonymization",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnPresetGaming_Click(object sender, RoutedEventArgs e)
        {
            // Gaming: Network + some system IDs
            ResetAllCheckboxes();
            ChkRandomizeAllMAC.IsChecked = true;
            ChkRandomizeComputerName.IsChecked = true;
            ChkRandomizeMachineGUID.IsChecked = true;
            ChkRandomizeBIOSSerial.IsChecked = true;
            ChkRandomizeBaseboardSerial.IsChecked = true;
            ChkDisableAdvertisingID.IsChecked = true;
            ChkClearTelemetryID.IsChecked = true;

            MessageBox.Show(
                "Gaming/Anti-Cheat Bypass Preset selected:\n\n" +
                "- Randomize MAC Address\n" +
                "- Randomize Computer Name\n" +
                "- Randomize Machine GUID\n" +
                "- Randomize BIOS Serial\n" +
                "- Randomize Motherboard Serial\n" +
                "- Privacy options\n\n" +
                "WARNING: May trigger anti-cheat systems or require re-authentication!",
                "Gaming Preset",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private void BtnPresetExtreme_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "EXTREME WARNING\n\n" +
                "This will randomize ALL identifiers including:\n" +
                "- Network identifiers (MAC, hostname)\n" +
                "- System IDs (GUID, Product ID, Installation)\n" +
                "- Hardware serials (BIOS, motherboard, disk)\n" +
                "- Privacy identifiers\n\n" +
                "This WILL:\n" +
                "- Require Windows re-activation\n" +
                "- Break software licenses\n" +
                "- Require network reconfiguration\n" +
                "- Possibly trigger anti-tamper systems\n\n" +
                "Create a system restore point first!\n\n" +
                "Continue?",
                "Extreme Anonymization",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Select everything except TPM and Volume Serial (most dangerous)
                foreach (var category in _categoryCheckBoxes.Values)
                {
                    foreach (var cb in category)
                    {
                        if (cb.Name != "ChkClearTPM" && cb.Name != "ChkChangeVolumeSerial")
                        {
                            cb.IsChecked = true;
                        }
                    }
                }
            }
        }

        private void ResetAllCheckboxes()
        {
            foreach (var category in _categoryCheckBoxes.Values)
            {
                foreach (var cb in category)
                {
                    cb.IsChecked = false;
                }
            }
        }

        // ==================== ACTION BUTTONS ====================

        private void BtnShowCurrentIDs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string info = "Current System Identifiers:\n\n";

                info += "--- Network ---\n";
                info += $"Computer Name: {Environment.MachineName}\n";
                info += $"Domain: {Environment.UserDomainName}\n";

                info += "\n--- System ---\n";
                info += $"Machine GUID: {GetRegistryValue(@"SOFTWARE\Microsoft\Cryptography", "MachineGuid")}\n";
                info += $"Product ID: {GetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductId")}\n";
                info += $"Installation ID: {GetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallationID")}\n";

                info += "\n--- Hardware ---\n";
                info += $"BIOS Serial: {GetRegistryValue(@"HARDWARE\DESCRIPTION\System\BIOS", "SystemSerialNumber")}\n";
                info += $"Baseboard Serial: {GetRegistryValue(@"HARDWARE\DESCRIPTION\System\BIOS", "BaseboardSerialNumber")}\n";

                info += "\n--- Privacy ---\n";
                info += $"Advertising ID Disabled: {GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled") == "0"}\n";

                MessageBox.Show(info, "Current System IDs", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading system information:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCreateRestorePoint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "This will create a System Restore Point.\n\n" +
                    "This may take a few minutes.\n\n" +
                    "Continue?",
                    "Create Restore Point",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    string script = @"
                        Checkpoint-Computer -Description 'SecVers_Debloat - Before Anonymization' -RestorePointType 'MODIFY_SETTINGS'
                    ";

                    ExecutePowerShellAsAdmin(script);

                    MessageBox.Show(
                        "System Restore Point created successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create restore point:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnApplyAnonymization_Click(object sender, RoutedEventArgs e)
        {
            // Check if any options selected
            bool anySelected = _categoryCheckBoxes.Values.Any(list => list.Any(cb => cb.IsChecked == true));

            if (!anySelected)
            {
                MessageBox.Show("Please select at least one anonymization option.", "No Options Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Build warning message
            string warningMessage = "WARNING\n\n" +
                                   "You are about to anonymize the following:\n\n";

            var selectedOptions = new List<string>();

            foreach (var category in _categoryCheckBoxes)
            {
                var selected = category.Value.Where(cb => cb.IsChecked == true).ToList();
                if (selected.Any())
                {
                    warningMessage += $"- {category.Key}: {selected.Count} option(s)\n";
                    selectedOptions.AddRange(selected.Select(cb => cb.Name));
                }
            }

            warningMessage += "\n" +
                            "This operation:\n" +
                            "- Cannot be easily undone\n" +
                            "- May require Windows re-activation\n" +
                            "- May break software licenses\n" +
                            "- Requires a system restart\n\n" +
                            "Have you created a restore point?\n\n" +
                            "Continue?";

            var result = MessageBox.Show(warningMessage, "Confirm Anonymization", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ApplySelectedAnonymization(selectedOptions);
            }
        }

        // ==================== APPLY ANONYMIZATION ====================

        private void ApplySelectedAnonymization(List<string> selectedOptions)
        {
            try
            {
                int successCount = 0;
                int failCount = 0;
                var errors = new List<string>();

                // Network Identifiers
                if (selectedOptions.Contains("ChkRandomizeMAC"))
                {
                    try
                    {
                        _anonymizer.RandomizeAllMACAddresses();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"MAC: {ex.Message}"); }
                }


                if (selectedOptions.Contains("ChkRandomizeComputerName"))
                {
                    try
                    {
                        _anonymizer.RandomizeComputerName();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Computer Name: {ex.Message}"); }
                }

                // System Identifiers
                if (selectedOptions.Contains("ChkRandomizeMachineGUID"))
                {
                    try
                    {
                        _anonymizer.RandomizeMachineGUID();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Machine GUID: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkRandomizeProductID"))
                {
                    try
                    {
                        _anonymizer.RandomizeProductID();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Product ID: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkRandomizeInstallationID"))
                {
                    try
                    {
                        _anonymizer.RandomizeInstallationID();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Installation ID: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkRandomizeInstallDate"))
                {
                    try
                    {
                        _anonymizer.RandomizeInstallDate();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Install Date: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkRandomizeRegisteredOwner"))
                {
                    try
                    {
                        _anonymizer.RandomizeRegisteredOwner();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Registered Owner: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkRandomizeRegisteredOrg"))
                {
                    try
                    {
                        _anonymizer.RandomizeRegisteredOrganization();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Registered Org: {ex.Message}"); }
                }

                // Hardware Identifiers
                if (selectedOptions.Contains("ChkRandomizeBIOSSerial"))
                {
                    try
                    {
                        _anonymizer.RandomizeBIOSSerial();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"BIOS Serial: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkRandomizeMotherboardSerial"))
                {
                    try
                    {
                        _anonymizer.RandomizeBaseboardSerial();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Motherboard Serial: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkRandomizeDiskSerial"))
                {
                    try
                    {
                        _anonymizer.SpoofDiskSerial("DISK-" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper());
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Disk Serial: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkSpoofSystemProductName"))
                {
                    try
                    {
                        _anonymizer.SpoofSystemProductName("Generic Computer");
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Product Name: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkSpoofSystemManufacturer"))
                {
                    try
                    {
                        _anonymizer.SpoofSystemManufacturer("Generic Manufacturer");
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Manufacturer: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkSpoofGPUDeviceID"))
                {
                    try
                    {
                        _anonymizer.SpoofGPUDeviceID("Generic VGA Adapter");
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"GPU Device ID: {ex.Message}"); }
                }

                // Privacy Identifiers
                if (selectedOptions.Contains("ChkDisableAdvertisingID"))
                {
                    try
                    {
                        _anonymizer.DisableAndClearAdvertisingID();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Advertising ID: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkClearTelemetryID"))
                {
                    try
                    {
                        _anonymizer.ClearTelemetryID();
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Telemetry ID: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkDisableWindowsID"))
                {
                    try
                    {
                        SetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                                       "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord);
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Windows ID: {ex.Message}"); }
                }

                if (selectedOptions.Contains("ChkClearActivityHistory"))
                {
                    try
                    {
                        SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\System",
                                       "PublishUserActivities", 0, RegistryValueKind.DWord);
                        SetRegistryValue(@"SOFTWARE\Policies\Microsoft\Windows\System",
                                       "UploadUserActivities", 0, RegistryValueKind.DWord);
                        successCount++;
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Activity History: {ex.Message}"); }
                }

                // Advanced Operations (most dangerous)
                if (selectedOptions.Contains("ChkClearTPM"))
                {
                    var tpmResult = MessageBox.Show(
                        "FINAL WARNING\n\n" +
                        "Clearing the TPM will:\n" +
                        "- Disable BitLocker (data will be inaccessible!)\n" +
                        "- Remove all TPM-stored keys\n" +
                        "- Break Windows Hello\n" +
                        "- Require physical presence confirmation\n\n" +
                        "This is IRREVERSIBLE without backup!\n\n" +
                        "Continue?",
                        "Clear TPM",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Stop);

                    if (tpmResult == MessageBoxResult.Yes)
                    {
                        try
                        {
                            _anonymizer.ClearTPM();
                            successCount++;
                        }
                        catch (Exception ex) { failCount++; errors.Add($"TPM: {ex.Message}"); }
                    }
                }

                if (selectedOptions.Contains("ChkChangeVolumeSerial"))
                {
                    try
                    {
                        // This requires VolumeID.exe - show message if not available
                        MessageBox.Show(
                            "Volume Serial Number change requires VolumeID.exe from Sysinternals.\n\n" +
                            "Download from: https://docs.microsoft.com/en-us/sysinternals/downloads/volumeid\n\n" +
                            "Place in system PATH or application directory.",
                            "VolumeID Required",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex) { failCount++; errors.Add($"Volume Serial: {ex.Message}"); }
                }

                // Show results
                string resultMessage = $"Anonymization Complete!\n\n" +
                                     $"Successful: {successCount}\n" +
                                     $"Failed: {failCount}\n\n";

                if (errors.Any())
                {
                    resultMessage += "Errors:\n" + string.Join("\n", errors.Take(5));
                    if (errors.Count > 5)
                        resultMessage += $"\n... and {errors.Count - 5} more";
                }

                resultMessage += "\n\nRESTART REQUIRED for changes to take effect!";

                MessageBox.Show(resultMessage, "Anonymization Results", MessageBoxButton.OK,
                              failCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

                // Ask to restart
                var restartResult = MessageBox.Show(
                    "Restart now to apply changes?",
                    "Restart Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (restartResult == MessageBoxResult.Yes)
                {
                    Process.Start("shutdown", "/r /t 5 /c \"SecVers Debloat - Restarting to apply anonymization\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical error during anonymization:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetRegistryValue(string path, string name)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path))
                {
                    return key?.GetValue(name)?.ToString() ?? "N/A";
                }
            }
            catch
            {
                return "N/A";
            }
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
                throw;
            }
        }

        private void ExecutePowerShellAsAdmin(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                UseShellExecute = true,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using (Process process = Process.Start(psi))
            {
                process?.WaitForExit();
            }
        }
    }
}