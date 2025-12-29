using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SecVers_Debloat.Patches;
using iNKORE.UI.WPF.Modern.Controls;

namespace SecVers_Debloat.UI.Pages
{
    public partial class RegistryTweaksPage : System.Windows.Controls.Page
    {
        // Dictionary to store all tweaks
        private Dictionary<CheckBox, Action> _tweakActions = new Dictionary<CheckBox, Action>();

        // Category checkbox lists for batch operations
        private List<CheckBox> _performanceCheckBoxes = new List<CheckBox>();
        private List<CheckBox> _gamingCheckBoxes = new List<CheckBox>();
        private List<CheckBox> _privacyCheckBoxes = new List<CheckBox>();
        private List<CheckBox> _win11CheckBoxes = new List<CheckBox>();
        private List<CheckBox> _powerCheckBoxes = new List<CheckBox>();
        private List<CheckBox> _diskCheckBoxes = new List<CheckBox>();
        private List<CheckBox> _experimentalCheckBoxes = new List<CheckBox>();

        public RegistryTweaksPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTweakMappings();
        }

        private void InitializeTweakMappings()
        {
            // ============ PERFORMANCE TWEAKS ============
            _performanceCheckBoxes.AddRange(new[]
            {
                ChkDisableSearchIndexing,
                ChkDisableSuperfetch,
                ChkDisableWindowsTips,
                ChkDisableHibernation,
                ChkDisableFastStartup,
                ChkDisableLastAccessTime,
                ChkDisableErrorReporting,
                ChkDisableCompatAssistant,
                ChkVisualEffectsBestPerf,
                ChkDisableTransparency,
                ChkDisableAnimations
            });

            _tweakActions[ChkDisableSearchIndexing] = RegistryTweaks.DisableSearchIndexing;
            _tweakActions[ChkDisableSuperfetch] = RegistryTweaks.DisableSuperfetch;
            _tweakActions[ChkDisableWindowsTips] = RegistryTweaks.DisableWindowsTips;
            _tweakActions[ChkDisableHibernation] = RegistryTweaks.DisableHibernation;
            _tweakActions[ChkDisableFastStartup] = RegistryTweaks.DisableFastStartup;
            _tweakActions[ChkDisableLastAccessTime] = RegistryTweaks.DisableLastAccessTimeStamp;
            _tweakActions[ChkDisableErrorReporting] = RegistryTweaks.DisableErrorReporting;
            _tweakActions[ChkDisableCompatAssistant] = RegistryTweaks.DisableCompatibilityAssistant;
            _tweakActions[ChkVisualEffectsBestPerf] = RegistryTweaks.SetVisualEffectsBestPerformance;
            _tweakActions[ChkDisableTransparency] = RegistryTweaks.DisableTransparency;
            _tweakActions[ChkDisableAnimations] = RegistryTweaks.DisableAnimations;

            // ============ GAMING TWEAKS ============
            _gamingCheckBoxes.AddRange(new[]
            {
                ChkDisableGameDVR,
                ChkEnableGameMode,
                ChkOptimizeBackgroundServices,
                ChkDisableFullscreenOpt,
                ChkSetHighGPUPriority,
                ChkOptimizeNetworkLatency,
                ChkDisableNaglesAlgorithm
            });

            _tweakActions[ChkDisableGameDVR] = RegistryTweaks.DisableGameDVR;
            _tweakActions[ChkEnableGameMode] = RegistryTweaks.EnableGameMode;
            _tweakActions[ChkOptimizeBackgroundServices] = RegistryTweaks.OptimizeForBackgroundServices;
            _tweakActions[ChkDisableFullscreenOpt] = RegistryTweaks.DisableFullscreenOptimizations;
            _tweakActions[ChkSetHighGPUPriority] = RegistryTweaks.SetHighGPUPriority;
            _tweakActions[ChkOptimizeNetworkLatency] = RegistryTweaks.OptimizeNetworkLatency;
            _tweakActions[ChkDisableNaglesAlgorithm] = RegistryTweaks.DisableNaglesAlgorithm;

            // ============ PRIVACY TWEAKS ============
            _privacyCheckBoxes.AddRange(new[]
            {
                ChkDisableTelemetry,
                ChkDisableActivityHistory,
                ChkDisableAdvertisingID,
                ChkDisableLocationTracking,
                ChkDisableFeedback,
                ChkDisableCortana,
                ChkDisableWebSearchInStart
            });

            _tweakActions[ChkDisableTelemetry] = RegistryTweaks.DisableTelemetry;
            _tweakActions[ChkDisableActivityHistory] = RegistryTweaks.DisableActivityHistory;
            _tweakActions[ChkDisableAdvertisingID] = RegistryTweaks.DisableAdvertisingID;
            _tweakActions[ChkDisableLocationTracking] = RegistryTweaks.DisableLocationTracking;
            _tweakActions[ChkDisableFeedback] = RegistryTweaks.DisableFeedback;
            _tweakActions[ChkDisableCortana] = RegistryTweaks.DisableCortana;
            _tweakActions[ChkDisableWebSearchInStart] = RegistryTweaks.DisableWebSearchInStart;

            // ============ WINDOWS 11 TWEAKS ============
            _win11CheckBoxes.AddRange(new[]
            {
                ChkRestoreOldContextMenu,
                ChkDisableWidgets,
                ChkDisableChatIcon,
                ChkAlignTaskbarLeft,
                ChkDisableSnapAssist,
                ChkShowFileExtensions,
                ChkShowHiddenFiles
            });

            _tweakActions[ChkRestoreOldContextMenu] = RegistryTweaks.RestoreOldContextMenu;
            _tweakActions[ChkDisableWidgets] = RegistryTweaks.DisableWidgets;
            _tweakActions[ChkDisableChatIcon] = RegistryTweaks.DisableChatIcon;
            _tweakActions[ChkAlignTaskbarLeft] = RegistryTweaks.AlignTaskbarLeft;
            _tweakActions[ChkDisableSnapAssist] = RegistryTweaks.DisableSnapAssist;
            _tweakActions[ChkShowFileExtensions] = RegistryTweaks.ShowFileExtensions;
            _tweakActions[ChkShowHiddenFiles] = RegistryTweaks.ShowHiddenFiles;

            // ============ POWER TWEAKS ============
            _powerCheckBoxes.AddRange(new[]
            {
                ChkDisableUSBSelectiveSuspend,
                ChkEnableUltimatePerf,
                ChkDisableCPUParking,
                ChkSetCPUMaxPerf,
                ChkDisablePCIExpressPM
            });

            _tweakActions[ChkDisableUSBSelectiveSuspend] = RegistryTweaks.DisableUSBSelectiveSuspend;
            _tweakActions[ChkEnableUltimatePerf] = RegistryTweaks.EnableUltimatePerformancePlan;
            _tweakActions[ChkDisableCPUParking] = RegistryTweaks.DisableCPUParkingAllCores;
            _tweakActions[ChkSetCPUMaxPerf] = RegistryTweaks.SetCPUMaxPerformance;
            _tweakActions[ChkDisablePCIExpressPM] = RegistryTweaks.DisablePCIExpressPowerManagement;

            // ============ DISK TWEAKS ============
            _diskCheckBoxes.AddRange(new[]
            {
                ChkDisableWriteCacheFlush,
                ChkOptimizeNTFSForSSD,
                ChkDisableAutoDefrag
            });

            _tweakActions[ChkDisableWriteCacheFlush] = RegistryTweaks.DisableWriteCacheBufferFlushing;
            _tweakActions[ChkOptimizeNTFSForSSD] = RegistryTweaks.OptimizeNTFSForSSD;
            _tweakActions[ChkDisableAutoDefrag] = RegistryTweaks.DisableAutoDefrag;

            // ============ EXPERIMENTAL TWEAKS ============
            _experimentalCheckBoxes.AddRange(new[]
            {
                ChkDisableMemoryCompression,
                ChkDisablePagingExecutive,
                ChkDisableSpectreMeltdown,
                ChkDisablePrefetcherCompletely,
                ChkDisableCStates
            });

            _tweakActions[ChkDisableMemoryCompression] = RegistryTweaks.DisableMemoryCompression;
            _tweakActions[ChkDisablePagingExecutive] = RegistryTweaks.DisablePagingExecutive;
            _tweakActions[ChkDisableSpectreMeltdown] = RegistryTweaks.DisableSpectreMeltdownMitigations;
            _tweakActions[ChkDisablePrefetcherCompletely] = RegistryTweaks.DisablePrefetcherCompletely;
            _tweakActions[ChkDisableCStates] = RegistryTweaks.DisableCStates;
        }

        #region Category Select All Logic

        // ============ PERFORMANCE ============
        private void PerformanceAll_Checked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_performanceCheckBoxes, true);
            UpdateSelectedCount();
        }

        private void PerformanceAll_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_performanceCheckBoxes, false);
            UpdateSelectedCount();
        }

        private void PerformanceAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(_performanceCheckBoxes))
            {
                ChkPerformanceAll.IsChecked = false;
            }
        }

        private void Performance_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkPerformanceAll, _performanceCheckBoxes);
            UpdateSelectedCount();
        }

        private void Performance_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkPerformanceAll, _performanceCheckBoxes);
            UpdateSelectedCount();
        }

        // ============ GAMING ============
        private void GamingAll_Checked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_gamingCheckBoxes, true);
            UpdateSelectedCount();
        }

        private void GamingAll_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_gamingCheckBoxes, false);
            UpdateSelectedCount();
        }

        private void GamingAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(_gamingCheckBoxes))
            {
                ChkGamingAll.IsChecked = false;
            }
        }

        private void Gaming_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkGamingAll, _gamingCheckBoxes);
            UpdateSelectedCount();
        }

        private void Gaming_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkGamingAll, _gamingCheckBoxes);
            UpdateSelectedCount();
        }

        // ============ PRIVACY ============
        private void PrivacyAll_Checked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_privacyCheckBoxes, true);
            UpdateSelectedCount();
        }

        private void PrivacyAll_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_privacyCheckBoxes, false);
            UpdateSelectedCount();
        }

        private void PrivacyAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(_privacyCheckBoxes))
            {
                ChkPrivacyAll.IsChecked = false;
            }
        }

        private void Privacy_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkPrivacyAll, _privacyCheckBoxes);
            UpdateSelectedCount();
        }

        private void Privacy_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkPrivacyAll, _privacyCheckBoxes);
            UpdateSelectedCount();
        }

        // ============ WINDOWS 11 ============
        private void Win11All_Checked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_win11CheckBoxes, true);
            UpdateSelectedCount();
        }

        private void Win11All_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_win11CheckBoxes, false);
            UpdateSelectedCount();
        }

        private void Win11All_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(_win11CheckBoxes))
            {
                ChkWin11All.IsChecked = false;
            }
        }

        private void Win11_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkWin11All, _win11CheckBoxes);
            UpdateSelectedCount();
        }

        private void Win11_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkWin11All, _win11CheckBoxes);
            UpdateSelectedCount();
        }

        // ============ POWER ============
        private void PowerAll_Checked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_powerCheckBoxes, true);
            UpdateSelectedCount();
        }

        private void PowerAll_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_powerCheckBoxes, false);
            UpdateSelectedCount();
        }

        private void PowerAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(_powerCheckBoxes))
            {
                ChkPowerAll.IsChecked = false;
            }
        }

        private void Power_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkPowerAll, _powerCheckBoxes);
            UpdateSelectedCount();
        }

        private void Power_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkPowerAll, _powerCheckBoxes);
            UpdateSelectedCount();
        }

        // ============ DISK ============
        private void DiskAll_Checked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_diskCheckBoxes, true);
            UpdateSelectedCount();
        }

        private void DiskAll_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_diskCheckBoxes, false);
            UpdateSelectedCount();
        }

        private void DiskAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(_diskCheckBoxes))
            {
                ChkDiskAll.IsChecked = false;
            }
        }

        private void Disk_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkDiskAll, _diskCheckBoxes);
            UpdateSelectedCount();
        }

        private void Disk_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkDiskAll, _diskCheckBoxes);
            UpdateSelectedCount();
        }

        // ============ EXPERIMENTAL ============
        private void ExperimentalAll_Checked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_experimentalCheckBoxes, true);
            UpdateSelectedCount();
        }

        private void ExperimentalAll_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCategoryCheckState(_experimentalCheckBoxes, false);
            UpdateSelectedCount();
        }

        private void ExperimentalAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (AreAllChecked(_experimentalCheckBoxes))
            {
                ChkExperimentalAll.IsChecked = false;
            }
        }

        private void Experimental_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkExperimentalAll, _experimentalCheckBoxes);
            UpdateSelectedCount();
        }

        private void Experimental_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCategoryCheckState(ChkExperimentalAll, _experimentalCheckBoxes);
            UpdateSelectedCount();
        }

        #endregion

        #region Helper Methods

        private void SetCategoryCheckState(List<CheckBox> checkBoxes, bool isChecked)
        {
            foreach (var cb in checkBoxes)
            {
                cb.IsChecked = isChecked;
            }
        }

        private bool AreAllChecked(List<CheckBox> checkBoxes)
        {
            return checkBoxes.All(cb => cb.IsChecked == true);
        }

        private bool AreAllUnchecked(List<CheckBox> checkBoxes)
        {
            return checkBoxes.All(cb => cb.IsChecked == false);
        }

        private void UpdateCategoryCheckState(CheckBox categoryCheckBox, List<CheckBox> childCheckBoxes)
        {
            if (childCheckBoxes.Any(cb => cb != null))
            {
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
                    categoryCheckBox.IsChecked = null; // Indeterminate
                }
            }
        }

        private void UpdateSelectedCount()
        {
            int count = _tweakActions.Keys.Count(cb => cb.IsChecked == true);
            TxtSelectedCount.Text = count.ToString();
        }

        #endregion

        #region Action Buttons

        private void BtnShowScript_Click(object sender, RoutedEventArgs e)
        {
            var selectedTweaks = _tweakActions
                .Where(kvp => kvp.Key.IsChecked == true)
                .Select(kvp => kvp.Key.Content.ToString())
                .ToList();

            if (selectedTweaks.Count == 0)
            {
                ShowInfoDialog("No Tweaks Selected", "Please select at least one tweak to view the script.");
                return;
            }

            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine("# Selected Registry Tweaks");
            scriptBuilder.AppendLine("# WARNING: These changes will modify your Windows registry!");
            scriptBuilder.AppendLine();

            foreach (var tweak in selectedTweaks)
            {
                scriptBuilder.AppendLine($"- {tweak}");
            }

            ShowScriptDialog("Selected Tweaks Preview", scriptBuilder.ToString());
        }

        private async void BtnApplyTweaks_Click(object sender, RoutedEventArgs e)
        {
            var selectedActions = _tweakActions
                .Where(kvp => kvp.Key.IsChecked == true)
                .ToList();

            if (selectedActions.Count == 0)
            {
                ShowInfoDialog("No Tweaks Selected", "Please select at least one tweak to apply.");
                return;
            }

            // Confirmation dialog
            var result = await ShowConfirmationDialog(
                "Apply Registry Tweaks",
                $"You are about to apply {selectedActions.Count} registry tweaks.\n\n" +
                "This will modify system settings and may require a restart.\n\n" +
                "Do you want to continue?");

            if (result != ContentDialogResult.Primary)
                return;

            // Apply tweaks
            int successCount = 0;
            int failCount = 0;
            StringBuilder errorLog = new StringBuilder();

            foreach (var kvp in selectedActions)
            {
                try
                {
                    kvp.Value.Invoke();
                    successCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    errorLog.AppendLine($"Failed: {kvp.Key.Content} - {ex.Message}");
                }
            }

            // Show results
            if (failCount == 0)
            {
                ShowSuccessDialog("Tweaks Applied Successfully",
                    $"{successCount} tweaks were applied successfully!\n\n" +
                    "Some changes may require a system restart to take effect.");
            }
            else
            {
                ShowWarningDialog("Tweaks Applied with Errors",
                    $"Success: {successCount}\nFailed: {failCount}\n\n" +
                    $"Errors:\n{errorLog}");
            }
        }

        #endregion

        #region Dialog Helpers

        private void ShowInfoDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };
            _ = dialog.ShowAsync();
        }

        private void ShowScriptDialog(string title, string content)
        {
            var textBox = new TextBox
            {
                Text = content,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 300,
                FontFamily = new System.Windows.Media.FontFamily("Consolas")
            };

            var dialog = new ContentDialog
            {
                Title = title,
                Content = textBox,
                CloseButtonText = "Close"
            };
            _ = dialog.ShowAsync();
        }

        private Task<ContentDialogResult> ShowConfirmationDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Yes, Apply",
                CloseButtonText = "Cancel"
            };
            return dialog.ShowAsync();
        }

        private void ShowSuccessDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };
            _ = dialog.ShowAsync();
        }

        private void ShowWarningDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };
            _ = dialog.ShowAsync();
        }

        #endregion
    }
}
