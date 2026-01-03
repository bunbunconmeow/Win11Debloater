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
            ChkEnableAntiScreenshot.IsChecked = true;
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
            ChkEnableAntiScreenshot.IsChecked = false;
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
                ChkDisableWin32kSystemCalls, ChkEnableExportAddressFiltering, ChkEnableAntiScreenshot
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

            bool useDEP = ChkEnableDEP.IsChecked == true;
            bool useSEHOP = ChkEnableSEHOP.IsChecked == true;
            bool useBottomUpASLR = ChkEnableBottomUpASLR.IsChecked == true;
            bool useHighEntropyASLR = ChkEnableHighEntropyASLR.IsChecked == true;
            bool useMandatoryASLR = ChkEnableMandatoryASLR.IsChecked == true;
            bool useCFG = ChkEnableCFG.IsChecked == true;
            bool useValidateExceptionChains = ChkEnableValidateExceptionChains.IsChecked == true;
            bool useValidateHeapIntegrity = ChkEnableValidateHeapIntegrity.IsChecked == true;
            bool blockUntrustedFonts = ChkBlockUntrustedFonts.IsChecked == true;
            bool useCodeIntegrityGuard = ChkEnableCodeIntegrityGuard.IsChecked == true;
            bool disableWin32kSystemCalls = ChkDisableWin32kSystemCalls.IsChecked == true;
            bool useExportAddressFiltering = ChkEnableExportAddressFiltering.IsChecked == true;
            bool enableAntiScreenshot = ChkEnableAntiScreenshot.IsChecked == true;

            // Network Hardening
            bool disableSMBCompression = ChkDisableSMBCompression.IsChecked == true;
            bool enableFirewallAllProfiles = ChkEnableFirewallAllProfiles.IsChecked == true;
            bool setFirewallDefaultBlockInbound = ChkSetFirewallDefaultBlockInbound.IsChecked == true;
            bool disableIPv6 = ChkDisableIPv6.IsChecked == true;

            // Account Security
            bool disableGuestAccount = ChkDisableGuestAccount.IsChecked == true;
            bool enablePasswordComplexity = ChkEnablePasswordComplexity.IsChecked == true;
            bool disableCredentialStorage = ChkDisableCredentialStorage.IsChecked == true;
            bool enableUACMaximum = ChkEnableUACMaximum.IsChecked == true;
            bool blockMicrosoftAccounts = ChkBlockMicrosoftAccounts.IsChecked == true;
            bool limitBlankPasswordConsoleOnly = ChkLimitBlankPasswordConsoleOnly.IsChecked == true;

            // Service Hardening
            bool disableRemoteRegistry = ChkDisableRemoteRegistry.IsChecked == true;
            bool disableRemoteDesktop = ChkDisableRemoteDesktop.IsChecked == true;
            bool disablePrintSpooler = ChkDisablePrintSpooler.IsChecked == true;
            bool disableWindowsScriptHost = ChkDisableWindowsScriptHost.IsChecked == true;
            bool disablePowerShellV2 = ChkDisablePowerShellV2.IsChecked == true;
            bool disableWindowsErrorReporting = ChkDisableWindowsErrorReporting.IsChecked == true;
            bool disableTelemetryServices = ChkDisableTelemetryServices.IsChecked == true;
            bool disableXboxServices = ChkDisableXboxServices.IsChecked == true;
            bool disableBluetooth = ChkDisableBluetooth.IsChecked == true;
            bool disableSSDPDiscovery = ChkDisableSSDPDiscovery.IsChecked == true;

            // System Integrity
            bool enableKernelModeCodeIntegrity = ChkEnableKernelModeCodeIntegrity.IsChecked == true;
            bool enableCredentialGuard = ChkEnableCredentialGuard.IsChecked == true;
            bool enableMemoryIntegrity = ChkEnableMemoryIntegrity.IsChecked == true;
            bool disableAutoRun = ChkDisableAutoRun.IsChecked == true;
            bool disable8Dot3Names = ChkDisable8Dot3Names.IsChecked == true;
            bool disableRemoteAssistance = ChkDisableRemoteAssistance.IsChecked == true;
            bool disableHibernation = ChkDisableHibernation.IsChecked == true;
            bool enableTamperProtection = ChkEnableTamperProtection.IsChecked == true;

            // Privacy Hardening
            bool disableTelemetryCompletely = ChkDisableTelemetryCompletely.IsChecked == true;
            bool disableCortana = ChkDisableCortana.IsChecked == true;
            bool disableWindowsFeedback = ChkDisableWindowsFeedback.IsChecked == true;
            bool disableTimeline = ChkDisableTimeline.IsChecked == true;
            bool disableCloudClipboard = ChkDisableCloudClipboard.IsChecked == true;
            bool disableInputPersonalization = ChkDisableInputPersonalization.IsChecked == true;
            bool disableOneDrive = ChkDisableOneDrive.IsChecked == true;

            // Attack Surface Reduction
            bool blockExecContentEmail = ChkBlockExecutableContentFromEmailWebmail.IsChecked == true;
            bool blockOfficeChildProc = ChkBlockOfficeChildProcesses.IsChecked == true;
            bool blockOfficeExecContent = ChkBlockOfficeExecutableContent.IsChecked == true;
            bool blockOfficeInjection = ChkBlockOfficeInjection.IsChecked == true;
            bool blockScriptExecDownload = ChkBlockScriptExecutableDownload.IsChecked == true;


            BtnApplyHardening.IsEnabled = false;
            BtnApplyHardening.Content = "Applying...";

            try
            {
               
                await Task.Run(() =>
                {
                    // Exploit
                    if (useDEP) _hardeningManager.Exploit.EnableDEPForAllProcesses();
                    if (useSEHOP) _hardeningManager.Exploit.EnableSEHOP();
                    if (useBottomUpASLR) _hardeningManager.Exploit.EnableBottomUpASLR();
                    if (useHighEntropyASLR) _hardeningManager.Exploit.EnableHighEntropyASLR();
                    if (useMandatoryASLR) _hardeningManager.Exploit.EnableMandatoryASLR();
                    if (useCFG) _hardeningManager.Exploit.EnableControlFlowGuard();
                    if (useValidateExceptionChains) _hardeningManager.Exploit.EnableValidateExceptionChains();
                    if (useValidateHeapIntegrity) _hardeningManager.Exploit.EnableValidateHeapIntegrity();
                    if (blockUntrustedFonts) _hardeningManager.Exploit.BlockUntrustedFonts();
                    if (useCodeIntegrityGuard) _hardeningManager.Exploit.EnableCodeIntegrityGuard();
                    if (disableWin32kSystemCalls) _hardeningManager.Exploit.DisableWin32kSystemCalls();
                    if (useExportAddressFiltering) _hardeningManager.Exploit.EnableExportAddressFiltering();
                    if(enableAntiScreenshot) DisableScreenshots.Execute();


                    // Network Hardening
                    if (disableSMBCompression) _hardeningManager.Network.DisableSMBCompression();
                    if (enableFirewallAllProfiles) _hardeningManager.Network.EnableFirewallAllProfiles();
                    if (setFirewallDefaultBlockInbound) _hardeningManager.Network.SetFirewallDefaultBlockInbound();
                    if (disableIPv6) _hardeningManager.Network.DisableIPv6();

                    // Account Security
                    if (disableGuestAccount) _hardeningManager.Accounts.DisableGuestAccount();
                    if (enablePasswordComplexity) _hardeningManager.Accounts.EnablePasswordComplexity();
                    if (disableCredentialStorage) _hardeningManager.Accounts.DisableCredentialStorage();
                    if (enableUACMaximum) _hardeningManager.Accounts.EnableUACMaximum();
                    if (blockMicrosoftAccounts) _hardeningManager.Accounts.BlockMicrosoftAccounts();
                    if (limitBlankPasswordConsoleOnly) _hardeningManager.Accounts.LimitBlankPasswordConsoleOnly();

                    // Service Hardening
                    if (disableRemoteRegistry) _hardeningManager.Services.DisableRemoteRegistry();
                    if (disableRemoteDesktop) _hardeningManager.Services.DisableRemoteDesktop();
                    if (disablePrintSpooler) _hardeningManager.Services.DisablePrintSpooler();
                    if (disableWindowsScriptHost) _hardeningManager.Services.DisableWindowsScriptHost();
                    if (disablePowerShellV2) _hardeningManager.Services.DisablePowerShellV2();
                    if (disableWindowsErrorReporting) _hardeningManager.Services.DisableWindowsErrorReporting();
                    if (disableTelemetryServices) _hardeningManager.Services.DisableTelemetryServices();
                    if (disableXboxServices) _hardeningManager.Services.DisableXboxServices();
                    if (disableBluetooth) _hardeningManager.Services.DisableBluetooth();
                    if (disableSSDPDiscovery) _hardeningManager.Services.DisableSSDPDiscovery();

                    // System Integrity
                    if (enableKernelModeCodeIntegrity) _hardeningManager.System.EnableKernelModeCodeIntegrity();
                    if (enableCredentialGuard) _hardeningManager.System.EnableCredentialGuard();
                    if (enableMemoryIntegrity) _hardeningManager.System.EnableMemoryIntegrity();
                    if (disableAutoRun) _hardeningManager.System.DisableAutoRun();
                    if (disable8Dot3Names) _hardeningManager.System.Disable8Dot3Names();
                    if (disableRemoteAssistance) _hardeningManager.System.DisableRemoteAssistance();
                    if (disableHibernation) _hardeningManager.System.DisableHibernation();
                    if (enableTamperProtection) _hardeningManager.System.EnableTamperProtection();

                    // Privacy Hardening
                    if (disableTelemetryCompletely) _hardeningManager.Privacy.DisableTelemetryCompletely();
                    if (disableCortana) _hardeningManager.Privacy.DisableCortana();
                    if (disableWindowsFeedback) _hardeningManager.Privacy.DisableWindowsFeedback();
                    if (disableTimeline) _hardeningManager.Privacy.DisableTimeline();
                    if (disableCloudClipboard) _hardeningManager.Privacy.DisableCloudClipboard();
                    if (disableInputPersonalization) _hardeningManager.Privacy.DisableInputPersonalization();
                    if (disableOneDrive) _hardeningManager.Privacy.DisableOneDrive();

                    // Attack Surface Reduction
                    if (blockExecContentEmail) _hardeningManager.ASR.BlockExecutableContentFromEmailWebmail();
                    if (blockOfficeChildProc) _hardeningManager.ASR.BlockOfficeChildProcesses();
                    if (blockOfficeExecContent) _hardeningManager.ASR.BlockOfficeExecutableContent();
                    if (blockOfficeInjection) _hardeningManager.ASR.BlockOfficeInjection();
                    if (blockScriptExecDownload) _hardeningManager.ASR.BlockScriptExecutableDownload();
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
