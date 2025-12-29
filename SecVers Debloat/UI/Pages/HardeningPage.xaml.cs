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
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;


namespace SecVers_Debloat.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für HardeningPage.xaml
    /// </summary>
    public partial class HardeningPage : Page
    {
        private readonly SystemHardeningManager _hardeningManager;
        private bool _isInitializing = false;

        public HardeningPage()
        {
            InitializeComponent();
            _hardeningManager = new SystemHardeningManager();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _isInitializing = false;
        }

        // Prevents checkbox in header from toggling expander
        private void HeaderCheckBox_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        #region Exploit Protection

        private void ExploitProtectionAll_Checked(object sender, RoutedEventArgs e)
        {
            ChkEnableDEP.IsChecked = true;
            ChkEnableSEHOP.IsChecked = true;
            ChkEnableBottomUpASLR.IsChecked = true;
            ChkEnableHighEntropyASLR.IsChecked = true;
            ChkEnableMandatoryASLR.IsChecked = true;
            ChkEnableCFG.IsChecked = true;
            ChkEnableValidateExceptionChains.IsChecked = true;
            ChkEnableValidateHeapIntegrity.IsChecked = true;
            ChkBlockUntrustedFonts.IsChecked = true;
            ChkEnableCodeIntegrityGuard.IsChecked = true;
            ChkDisableWin32kSystemCalls.IsChecked = true;
            ChkEnableExportAddressFiltering.IsChecked = true;
        }

        private void ExploitProtectionAll_Unchecked(object sender, RoutedEventArgs e)
        {
            ChkEnableDEP.IsChecked = false;
            ChkEnableSEHOP.IsChecked = false;
            ChkEnableBottomUpASLR.IsChecked = false;
            ChkEnableHighEntropyASLR.IsChecked = false;
            ChkEnableMandatoryASLR.IsChecked = false;
            ChkEnableCFG.IsChecked = false;
            ChkEnableValidateExceptionChains.IsChecked = false;
            ChkEnableValidateHeapIntegrity.IsChecked = false;
            ChkBlockUntrustedFonts.IsChecked = false;
            ChkEnableCodeIntegrityGuard.IsChecked = false;
            ChkDisableWin32kSystemCalls.IsChecked = false;
            ChkEnableExportAddressFiltering.IsChecked = false;
        }

        private void ExploitProtectionAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(GetExploitProtectionCheckBoxes()))
            {
                ExploitProtectionAllCheckBox.IsChecked = false;
            }
        }

        private void ExploitProtectionOption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(ExploitProtectionAllCheckBox, GetExploitProtectionCheckBoxes());
        }

        private void ExploitProtectionOption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(ExploitProtectionAllCheckBox, GetExploitProtectionCheckBoxes());
        }

        private List<CheckBox> GetExploitProtectionCheckBoxes()
        {
            return new List<CheckBox>
            {
                ChkEnableDEP, ChkEnableSEHOP, ChkEnableBottomUpASLR, ChkEnableHighEntropyASLR,
                ChkEnableMandatoryASLR, ChkEnableCFG, ChkEnableValidateExceptionChains,
                ChkEnableValidateHeapIntegrity, ChkBlockUntrustedFonts, ChkEnableCodeIntegrityGuard,
                ChkDisableWin32kSystemCalls, ChkEnableExportAddressFiltering
            };
        }

        #endregion

        #region Network Hardening

        private void NetworkHardeningAll_Checked(object sender, RoutedEventArgs e)
        {
          
            ChkDisableSMBCompression.IsChecked = true;
            ChkEnableFirewallAllProfiles.IsChecked = true;
            ChkSetFirewallDefaultBlockInbound.IsChecked = true;
            ChkDisableIPv6.IsChecked = true;
        }

        private void NetworkHardeningAll_Unchecked(object sender, RoutedEventArgs e)
        {
            ChkDisableSMBCompression.IsChecked = false;
            ChkEnableFirewallAllProfiles.IsChecked = false;
            ChkSetFirewallDefaultBlockInbound.IsChecked = false;
            ChkDisableIPv6.IsChecked = false;
        }

        private void NetworkHardeningAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(GetNetworkHardeningCheckBoxes()))
            {
                NetworkHardeningAllCheckBox.IsChecked = false;
            }
        }

        private void NetworkHardeningOption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(NetworkHardeningAllCheckBox, GetNetworkHardeningCheckBoxes());
        }

        private void NetworkHardeningOption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(NetworkHardeningAllCheckBox, GetNetworkHardeningCheckBoxes());
        }

        private List<CheckBox> GetNetworkHardeningCheckBoxes()
        {
            return new List<CheckBox>
            {
                ChkDisableSMBCompression, ChkEnableFirewallAllProfiles,
                ChkSetFirewallDefaultBlockInbound, ChkDisableIPv6
            };
        }

        #endregion

        #region Account Security

        private void AccountSecurityAll_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ChkDisableGuestAccount.IsChecked = true;
            ChkEnablePasswordComplexity.IsChecked = true;
            ChkDisableCredentialStorage.IsChecked = true;
            ChkEnableUACMaximum.IsChecked = true;
            ChkBlockMicrosoftAccounts.IsChecked = true;
            ChkLimitBlankPasswordConsoleOnly.IsChecked = true;
        }

        private void AccountSecurityAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ChkDisableGuestAccount.IsChecked = false;
            ChkEnablePasswordComplexity.IsChecked = false;
            ChkDisableCredentialStorage.IsChecked = false;
            ChkEnableUACMaximum.IsChecked = false;
            ChkBlockMicrosoftAccounts.IsChecked = false;
            ChkLimitBlankPasswordConsoleOnly.IsChecked = false;
        }

        private void AccountSecurityAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(GetAccountSecurityCheckBoxes()))
            {
                AccountSecurityAllCheckBox.IsChecked = false;
            }
        }

        private void AccountSecurityOption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(AccountSecurityAllCheckBox, GetAccountSecurityCheckBoxes());
        }

        private void AccountSecurityOption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(AccountSecurityAllCheckBox, GetAccountSecurityCheckBoxes());
        }

        private List<CheckBox> GetAccountSecurityCheckBoxes()
        {
            return new List<CheckBox>
            {
                ChkDisableGuestAccount, ChkEnablePasswordComplexity, ChkDisableCredentialStorage,
                ChkEnableUACMaximum, ChkBlockMicrosoftAccounts, ChkLimitBlankPasswordConsoleOnly
            };
        }

        #endregion

        #region Service Hardening

        private void ServiceHardeningAll_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ChkDisableRemoteRegistry.IsChecked = true;
            ChkDisableRemoteDesktop.IsChecked = true;
            ChkDisablePrintSpooler.IsChecked = true;
            ChkDisableWindowsScriptHost.IsChecked = true;
            ChkDisablePowerShellV2.IsChecked = true;
            ChkDisableWindowsErrorReporting.IsChecked = true;
            ChkDisableTelemetryServices.IsChecked = true;
            ChkDisableXboxServices.IsChecked = true;
            ChkDisableBluetooth.IsChecked = true;
            ChkDisableSSDPDiscovery.IsChecked = true;
        }

        private void ServiceHardeningAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ChkDisableRemoteRegistry.IsChecked = false;
            ChkDisableRemoteDesktop.IsChecked = false;
            ChkDisablePrintSpooler.IsChecked = false;
            ChkDisableWindowsScriptHost.IsChecked = false;
            ChkDisablePowerShellV2.IsChecked = false;
            ChkDisableWindowsErrorReporting.IsChecked = false;
            ChkDisableTelemetryServices.IsChecked = false;
            ChkDisableXboxServices.IsChecked = false;
            ChkDisableBluetooth.IsChecked = false;
            ChkDisableSSDPDiscovery.IsChecked = false;
        }

        private void ServiceHardeningAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(GetServiceHardeningCheckBoxes()))
            {
                ServiceHardeningAllCheckBox.IsChecked = false;
            }
        }

        private void ServiceHardeningOption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(ServiceHardeningAllCheckBox, GetServiceHardeningCheckBoxes());
        }

        private void ServiceHardeningOption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(ServiceHardeningAllCheckBox, GetServiceHardeningCheckBoxes());
        }

        private List<CheckBox> GetServiceHardeningCheckBoxes()
        {
            return new List<CheckBox>
            {
                ChkDisableRemoteRegistry, ChkDisableRemoteDesktop, ChkDisablePrintSpooler,
                ChkDisableWindowsScriptHost, ChkDisablePowerShellV2, ChkDisableWindowsErrorReporting,
                ChkDisableTelemetryServices, ChkDisableXboxServices, ChkDisableBluetooth,
                ChkDisableSSDPDiscovery
            };
        }

        #endregion

        #region System Integrity

        private void SystemIntegrityAll_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ChkEnableKernelModeCodeIntegrity.IsChecked = true;
            ChkEnableCredentialGuard.IsChecked = true;
            ChkEnableMemoryIntegrity.IsChecked = true;
            ChkDisableAutoRun.IsChecked = true;
            ChkDisable8Dot3Names.IsChecked = true;
            ChkDisableRemoteAssistance.IsChecked = true;
            ChkDisableHibernation.IsChecked = true;
            ChkEnableTamperProtection.IsChecked = true;
        }

        private void SystemIntegrityAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ChkEnableKernelModeCodeIntegrity.IsChecked = false;
            ChkEnableCredentialGuard.IsChecked = false;
            ChkEnableMemoryIntegrity.IsChecked = false;
            ChkDisableAutoRun.IsChecked = false;
            ChkDisable8Dot3Names.IsChecked = false;
            ChkDisableRemoteAssistance.IsChecked = false;
            ChkDisableHibernation.IsChecked = false;
            ChkEnableTamperProtection.IsChecked = false;
        }

        private void SystemIntegrityAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(GetSystemIntegrityCheckBoxes()))
            {
                SystemIntegrityAllCheckBox.IsChecked = false;
            }
        }

        private void SystemIntegrityOption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(SystemIntegrityAllCheckBox, GetSystemIntegrityCheckBoxes());
        }

        private void SystemIntegrityOption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(SystemIntegrityAllCheckBox, GetSystemIntegrityCheckBoxes());
        }

        private List<CheckBox> GetSystemIntegrityCheckBoxes()
        {
            return new List<CheckBox>
            {
                ChkEnableKernelModeCodeIntegrity, ChkEnableCredentialGuard, ChkEnableMemoryIntegrity,
                ChkDisableAutoRun, ChkDisable8Dot3Names, ChkDisableRemoteAssistance,
                ChkDisableHibernation, ChkEnableTamperProtection
            };
        }

        #endregion

        #region Privacy Hardening

        private void PrivacyHardeningAll_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ChkDisableTelemetryCompletely.IsChecked = true;
            ChkDisableCortana.IsChecked = true;
            ChkDisableWindowsFeedback.IsChecked = true;
            ChkDisableTimeline.IsChecked = true;
            ChkDisableCloudClipboard.IsChecked = true;
            ChkDisableInputPersonalization.IsChecked = true;
            ChkDisableOneDrive.IsChecked = true;
        }

        private void PrivacyHardeningAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ChkDisableTelemetryCompletely.IsChecked = false;
            ChkDisableCortana.IsChecked = false;
            ChkDisableWindowsFeedback.IsChecked = false;
            ChkDisableTimeline.IsChecked = false;
            ChkDisableCloudClipboard.IsChecked = false;
            ChkDisableInputPersonalization.IsChecked = false;
            ChkDisableOneDrive.IsChecked = false;
        }

        private void PrivacyHardeningAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(GetPrivacyHardeningCheckBoxes()))
            {
                PrivacyHardeningAllCheckBox.IsChecked = false;
            }
        }

        private void PrivacyHardeningOption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(PrivacyHardeningAllCheckBox, GetPrivacyHardeningCheckBoxes());
        }

        private void PrivacyHardeningOption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(PrivacyHardeningAllCheckBox, GetPrivacyHardeningCheckBoxes());
        }

        private List<CheckBox> GetPrivacyHardeningCheckBoxes()
        {
            return new List<CheckBox>
            {
                ChkDisableTelemetryCompletely, ChkDisableCortana, ChkDisableWindowsFeedback,
                ChkDisableTimeline, ChkDisableCloudClipboard, ChkDisableInputPersonalization,
                ChkDisableOneDrive
            };
        }

        #endregion

        #region Attack Surface Reduction

        private void ASRAll_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ChkBlockExecutableContentFromEmailWebmail.IsChecked = true;
            ChkBlockOfficeChildProcesses.IsChecked = true;
            ChkBlockOfficeExecutableContent.IsChecked = true;
            ChkBlockOfficeInjection.IsChecked = true;
            ChkBlockScriptExecutableDownload.IsChecked = true;
        }

        private void ASRAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ChkBlockExecutableContentFromEmailWebmail.IsChecked = false;
            ChkBlockOfficeChildProcesses.IsChecked = false;
            ChkBlockOfficeExecutableContent.IsChecked = false;
            ChkBlockOfficeInjection.IsChecked = false;
            ChkBlockScriptExecutableDownload.IsChecked = false;
        }

        private void ASRAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(GetASRCheckBoxes()))
            {
                ASRAllCheckBox.IsChecked = false;
            }
        }

        private void ASROption_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(ASRAllCheckBox, GetASRCheckBoxes());
        }

        private void ASROption_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryState(ASRAllCheckBox, GetASRCheckBoxes());
        }

        private List<CheckBox> GetASRCheckBoxes()
        {
            return new List<CheckBox>
            {
                ChkBlockExecutableContentFromEmailWebmail, ChkBlockOfficeChildProcesses,
                ChkBlockOfficeExecutableContent, ChkBlockOfficeInjection,
                ChkBlockScriptExecutableDownload
            };
        }

        #endregion

        #region Helper Methods

        private bool AreAllChecked(List<CheckBox> checkBoxes)
        {
            return checkBoxes?.All(cb => cb.IsChecked == true) ?? false;
        }

        private bool AreAllUnchecked(List<CheckBox> checkBoxes)
        {
            return checkBoxes?.All(cb => cb.IsChecked == false) ?? false;
        }

        private void UpdateCategoryState(CheckBox categoryCheckBox, List<CheckBox> childCheckBoxes)
        {
            if (_isInitializing || categoryCheckBox == null) return;

            if (AreAllChecked(childCheckBoxes))
            {
                categoryCheckBox.IsChecked = true;
            }
            else if (AreAllUnchecked(childCheckBoxes))
            {
                categoryCheckBox.IsChecked = false;
            }
            else
            {
                categoryCheckBox.IsChecked = null;
            }
        }

        #endregion



        #region Apply Actions

        private async void BtnApplyHardening_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to apply the selected hardening options?\n\n" +
                "This will modify system settings and may require a restart.\n" +
                "It is highly recommended to create a restore point first.",
                "Confirm Hardening",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            BtnApplyHardening.IsEnabled = false;
            BtnApplyHardening.Content = "Applying...";

            try
            {
                await Task.Run(() =>
                {
                    // Exploit Protection
                    if (ChkEnableDEP.IsChecked == true)
                        _hardeningManager.Exploit.EnableDEPForAllProcesses();
                    if (ChkEnableSEHOP.IsChecked == true)
                        _hardeningManager.Exploit.EnableSEHOP();
                    if (ChkEnableBottomUpASLR.IsChecked == true)
                        _hardeningManager.Exploit.EnableBottomUpASLR();
                    if (ChkEnableHighEntropyASLR.IsChecked == true)
                        _hardeningManager.Exploit.EnableHighEntropyASLR();
                    if (ChkEnableMandatoryASLR.IsChecked == true)
                        _hardeningManager.Exploit.EnableMandatoryASLR();
                    if (ChkEnableCFG.IsChecked == true)
                        _hardeningManager.Exploit.EnableControlFlowGuard();
                    if (ChkEnableValidateExceptionChains.IsChecked == true)
                        _hardeningManager.Exploit.EnableValidateExceptionChains();
                    if (ChkEnableValidateHeapIntegrity.IsChecked == true)
                        _hardeningManager.Exploit.EnableValidateHeapIntegrity();
                    if (ChkBlockUntrustedFonts.IsChecked == true)
                        _hardeningManager.Exploit.BlockUntrustedFonts();
                    if (ChkEnableCodeIntegrityGuard.IsChecked == true)
                        _hardeningManager.Exploit.EnableCodeIntegrityGuard();
                    if (ChkDisableWin32kSystemCalls.IsChecked == true)
                        _hardeningManager.Exploit.DisableWin32kSystemCalls();
                    if (ChkEnableExportAddressFiltering.IsChecked == true)
                        _hardeningManager.Exploit.EnableExportAddressFiltering();

                    // Network Hardening
                    if (ChkDisableSMBCompression.IsChecked == true)
                        _hardeningManager.Network.DisableSMBCompression();
                    if (ChkEnableFirewallAllProfiles.IsChecked == true)
                        _hardeningManager.Network.EnableFirewallAllProfiles();
                    if (ChkSetFirewallDefaultBlockInbound.IsChecked == true)
                        _hardeningManager.Network.SetFirewallDefaultBlockInbound();
                    if (ChkDisableIPv6.IsChecked == true)
                        _hardeningManager.Network.DisableIPv6();

                    // Account Security
                    if (ChkDisableGuestAccount.IsChecked == true)
                        _hardeningManager.Accounts.DisableGuestAccount();
                    if (ChkEnablePasswordComplexity.IsChecked == true)
                        _hardeningManager.Accounts.EnablePasswordComplexity();
                    if (ChkDisableCredentialStorage.IsChecked == true)
                        _hardeningManager.Accounts.DisableCredentialStorage();
                    if (ChkEnableUACMaximum.IsChecked == true)
                        _hardeningManager.Accounts.EnableUACMaximum();
                    if (ChkBlockMicrosoftAccounts.IsChecked == true)
                        _hardeningManager.Accounts.BlockMicrosoftAccounts();
                    if (ChkLimitBlankPasswordConsoleOnly.IsChecked == true)
                        _hardeningManager.Accounts.LimitBlankPasswordConsoleOnly();

                    // Service Hardening
                    if (ChkDisableRemoteRegistry.IsChecked == true)
                        _hardeningManager.Services.DisableRemoteRegistry();
                    if (ChkDisableRemoteDesktop.IsChecked == true)
                        _hardeningManager.Services.DisableRemoteDesktop();
                    if (ChkDisablePrintSpooler.IsChecked == true)
                        _hardeningManager.Services.DisablePrintSpooler();
                    if (ChkDisableWindowsScriptHost.IsChecked == true)
                        _hardeningManager.Services.DisableWindowsScriptHost();
                    if (ChkDisablePowerShellV2.IsChecked == true)
                        _hardeningManager.Services.DisablePowerShellV2();
                    if (ChkDisableWindowsErrorReporting.IsChecked == true)
                        _hardeningManager.Services.DisableWindowsErrorReporting();
                    if (ChkDisableTelemetryServices.IsChecked == true)
                        _hardeningManager.Services.DisableTelemetryServices();
                    if (ChkDisableXboxServices.IsChecked == true)
                        _hardeningManager.Services.DisableXboxServices();
                    if (ChkDisableBluetooth.IsChecked == true)
                        _hardeningManager.Services.DisableBluetooth();
                    if (ChkDisableSSDPDiscovery.IsChecked == true)
                        _hardeningManager.Services.DisableSSDPDiscovery();

                    // System Integrity
                    if (ChkEnableKernelModeCodeIntegrity.IsChecked == true)
                        _hardeningManager.System.EnableKernelModeCodeIntegrity();
                    if (ChkEnableCredentialGuard.IsChecked == true)
                        _hardeningManager.System.EnableCredentialGuard();
                    if (ChkEnableMemoryIntegrity.IsChecked == true)
                        _hardeningManager.System.EnableMemoryIntegrity();
                    if (ChkDisableAutoRun.IsChecked == true)
                        _hardeningManager.System.DisableAutoRun();
                    if (ChkDisable8Dot3Names.IsChecked == true)
                        _hardeningManager.System.Disable8Dot3Names();
                    if (ChkDisableRemoteAssistance.IsChecked == true)
                        _hardeningManager.System.DisableRemoteAssistance();
                    if (ChkDisableHibernation.IsChecked == true)
                        _hardeningManager.System.DisableHibernation();
                    if (ChkEnableTamperProtection.IsChecked == true)
                        _hardeningManager.System.EnableTamperProtection();

                    // Privacy Hardening
                    if (ChkDisableTelemetryCompletely.IsChecked == true)
                        _hardeningManager.Privacy.DisableTelemetryCompletely();
                    if (ChkDisableCortana.IsChecked == true)
                        _hardeningManager.Privacy.DisableCortana();
                    if (ChkDisableWindowsFeedback.IsChecked == true)
                        _hardeningManager.Privacy.DisableWindowsFeedback();
                    if (ChkDisableTimeline.IsChecked == true)
                        _hardeningManager.Privacy.DisableTimeline();
                    if (ChkDisableCloudClipboard.IsChecked == true)
                        _hardeningManager.Privacy.DisableCloudClipboard();
                    if (ChkDisableInputPersonalization.IsChecked == true)
                        _hardeningManager.Privacy.DisableInputPersonalization();
                    if (ChkDisableOneDrive.IsChecked == true)
                        _hardeningManager.Privacy.DisableOneDrive();

                    // Attack Surface Reduction
                    if (ChkBlockExecutableContentFromEmailWebmail.IsChecked == true)
                        _hardeningManager.ASR.BlockExecutableContentFromEmailWebmail();
                    if (ChkBlockOfficeChildProcesses.IsChecked == true)
                        _hardeningManager.ASR.BlockOfficeChildProcesses();
                    if (ChkBlockOfficeExecutableContent.IsChecked == true)
                        _hardeningManager.ASR.BlockOfficeExecutableContent();
                    if (ChkBlockOfficeInjection.IsChecked == true)
                        _hardeningManager.ASR.BlockOfficeInjection();
                    if (ChkBlockScriptExecutableDownload.IsChecked == true)
                        _hardeningManager.ASR.BlockScriptExecutableDownload();
                });

                MessageBox.Show(
                    "Hardening options applied successfully!\n\n" +
                    "Some changes may require a system restart to take effect.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while applying hardening options:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                BtnApplyHardening.IsEnabled = true;
                BtnApplyHardening.Content = "Apply Selected Hardening";
            }
        }

        #endregion
    }
}
