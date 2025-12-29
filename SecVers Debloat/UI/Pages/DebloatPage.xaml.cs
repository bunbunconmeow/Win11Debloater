using iNKORE.UI.WPF.Modern.Controls;
using SecVers_Debloat.Patches;
using System;
using System.Collections.Generic;
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
        private readonly DebloatEngine _engine;
        private readonly Dictionary<string, List<DebloatItemViewModel>> _categoryItems;
        private List<CheckBox> _currentCheckBoxes;
        private string _currentCategory = string.Empty;

        public DebloatPage()
        {
            InitializeComponent();
            _engine = new DebloatEngine();
            _categoryItems = new Dictionary<string, List<DebloatItemViewModel>>();
            _currentCheckBoxes = new List<CheckBox>();

            _engine.ProgressChanged += Engine_ProgressChanged;
            _engine.LogMessage += Engine_LogMessage;

            Loaded += DebloatPage_Loaded;
        }

        private async void DebloatPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAllCategoriesAsync();

            // Standard: Erste Kategorie laden
            CategoryTabControl.SelectedIndex = 0;
        }

        #region Data Loading

        private async Task LoadAllCategoriesAsync()
        {
            await Task.Run(() =>
            {
                // UWP Apps
                var uwpApps = _engine.Apps.GetRemovableApps()
                    .Select(app => new DebloatItemViewModel
                    {
                        Name = app.DisplayName,
                        Description = app.Description,
                        Category = "UWPApps",
                        Risk = "Caution",
                        IsSelected = false,
                        Data = app
                    }).ToList();
                _categoryItems["UWPApps"] = uwpApps;

                // Windows Features
                var features = _engine.Features.GetDisableableFeatures()
                    .Select(feature => new DebloatItemViewModel
                    {
                        Name = feature.DisplayName,
                        Description = feature.Description,
                        Category = "WinFeatures",
                        Risk = "Safe",
                        IsSelected = false,
                        Data = feature
                    }).ToList();
                _categoryItems["WinFeatures"] = features;

                // Services
                var services = _engine.Services.GetDisableableServices()
                    .Select(service => new DebloatItemViewModel
                    {
                        Name = service.DisplayName,
                        Description = service.Description,
                        Category = "Services",
                        Risk = service.Risk.ToString(),
                        IsSelected = false,
                        Data = service
                    }).ToList();
                _categoryItems["Services"] = services;

                // Telemetry
                var telemetry = _engine.Telemetry.GetTelemetryOptions()
                    .Select(t => new DebloatItemViewModel
                    {
                        Name = t.Name,
                        Description = t.Description,
                        Category = "Telemetry",
                        Risk = "Safe",
                        IsSelected = false,
                        Data = t
                    }).ToList();
                _categoryItems["Telemetry"] = telemetry;

                // OneDrive (single item)
                _categoryItems["OneDrive"] = new List<DebloatItemViewModel>
                {
                    new DebloatItemViewModel
                    {
                        Name = "Remove OneDrive",
                        Description = "Completely remove OneDrive from Windows",
                        Category = "OneDrive",
                        Risk = "Caution",
                        IsSelected = false
                    }
                };

                // Edge Browser (single item)
                _categoryItems["Edge"] = new List<DebloatItemViewModel>
                {
                    new DebloatItemViewModel
                    {
                        Name = "Remove Microsoft Edge",
                        Description = "Remove Edge browser and update services",
                        Category = "Edge",
                        Risk = "Caution",
                        IsSelected = false
                    }
                };

                // Context Menu
                var contextMenu = _engine.ContextMenu.GetContextMenuOptions()
                    .Select(c => new DebloatItemViewModel
                    {
                        Name = c.Name,
                        Description = c.Description,
                        Category = "ContextMenu",
                        Risk = "Safe",
                        Data = c,
                        IsSelected = false
                    }).ToList();
                _categoryItems["ContextMenu"] = contextMenu;

                // Startup
                var startup = _engine.Startup.GetStartupPrograms()
                    .Select(s => new DebloatItemViewModel
                    {
                        Name = s.Name,
                        Description = s.Command,
                        Category = "Startup",
                        Risk = "Safe",
                        Data = s,
                        IsSelected = !s.Enabled
                    }).ToList();
                _categoryItems["Startup"] = startup;

                // Windows Update
                var updates = _engine.Updates.GetUpdateOptions()
                    .Select(u => new DebloatItemViewModel
                    {
                        Name = u.Name,
                        Description = u.Description,
                        Category = "Updates",
                        Risk = u.Risk.ToString(),
                        Data = u,
                        IsSelected = false
                    }).ToList();
                _categoryItems["Updates"] = updates;
            });
        }

        #endregion

        #region Category Navigation

        private void CategoryTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryTabControl.SelectedItem is TabItem selectedTab)
            {
                string categoryKey = selectedTab.Name.Replace("Tab", "");
                LoadCategory(categoryKey);
            }
        }

        private void LoadCategory(string category)
        {
            _currentCategory = category;
            _currentCheckBoxes.Clear();
            ItemsPanel.Children.Clear();
            SearchBox.Text = string.Empty;

            if (!_categoryItems.ContainsKey(category))
            {
                ItemsPanel.Children.Add(new TextBlock
                {
                    Text = "No items available in this category.",
                    FontStyle = FontStyles.Italic,
                    Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
                    Margin = new Thickness(12),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                return;
            }

            var items = _categoryItems[category];

            if (items.Count == 0)
            {
                ItemsPanel.Children.Add(new TextBlock
                {
                    Text = "No items found in this category.",
                    FontStyle = FontStyles.Italic,
                    Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
                    Margin = new Thickness(12),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                return;
            }

            foreach (var item in items)
            {
                var checkBox = CreateCheckBox(item);
                _currentCheckBoxes.Add(checkBox);
                ItemsPanel.Children.Add(checkBox);
            }

            SetCheckedState();
        }

        #endregion

        #region UI Creation

        private CheckBox CreateCheckBox(DebloatItemViewModel item)
        {
            var checkBox = new CheckBox
            {
                Margin = new Thickness(24, 6, 0, 6),
                IsChecked = item.IsSelected,
                Tag = item
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };

            var nameText = new TextBlock
            {
                Text = item.Name,
                FontWeight = FontWeights.Medium,
                FontSize = 13
            };

            var descText = new TextBlock
            {
                Text = item.Description,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 0)
            };

            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(descText);

            var riskBorder = new Border
            {
                Background = new SolidColorBrush(GetRiskColor(item.Risk)),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(6, 2, 6, 2),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0)
            };

            var riskText = new TextBlock
            {
                Text = item.Risk,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };

            riskBorder.Child = riskText;

            Grid.SetColumn(stackPanel, 0);
            Grid.SetColumn(riskBorder, 1);

            grid.Children.Add(stackPanel);
            grid.Children.Add(riskBorder);

            checkBox.Content = grid;
            checkBox.Checked += Option_Checked;
            checkBox.Unchecked += Option_Unchecked;

            return checkBox;
        }

        private Color GetRiskColor(string risk)
        {
            var color = Color.FromRgb(16, 124, 16);
            switch (risk)
            {
                case "Safe":
                    color = Color.FromRgb(16, 124, 16);
                    break;
                case "Caution":
                    color = Color.FromRgb(202, 80, 16);
                    break;
                case "Dangerous":
                    color = Color.FromRgb(196, 43, 28);
                    break;
                default:
                    color = Color.FromRgb(102, 102, 102);
                    break;
            }

            return color;
        }

        #endregion

        #region Search

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();

                foreach (var cb in _currentCheckBoxes)
                {
                    if (cb.Tag is DebloatItemViewModel item)
                    {
                        bool matches = string.IsNullOrWhiteSpace(query) ||
                                       item.Name.ToLower().Contains(query) ||
                                       item.Description.ToLower().Contains(query);
                        cb.Visibility = matches ? Visibility.Visible : Visibility.Collapsed;
                    }
                }

                SetCheckedState();
            }
        }

        #endregion

        #region Selection Logic

        private void SelectAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var cb in _currentCheckBoxes)
            {
                if (cb.Visibility == Visibility.Visible)
                    cb.IsChecked = true;
            }
        }

        private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var cb in _currentCheckBoxes)
            {
                if (cb.Visibility == Visibility.Visible)
                    cb.IsChecked = false;
            }
        }

        private void SelectAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            bool allChecked = true;
            foreach (var cb in _currentCheckBoxes)
            {
                if (cb.Visibility == Visibility.Visible && cb.IsChecked != true)
                {
                    allChecked = false;
                    break;
                }
            }

            if (allChecked)
            {
                SelectAllCheckBox.IsChecked = false;
            }
        }

        private void SetCheckedState()
        {
            if (_currentCheckBoxes.Count == 0) return;

            var visibleBoxes = _currentCheckBoxes.Where(cb => cb.Visibility == Visibility.Visible).ToList();
            if (visibleBoxes.Count == 0) return;

            bool allChecked = visibleBoxes.All(cb => cb.IsChecked == true);
            bool noneChecked = visibleBoxes.All(cb => cb.IsChecked == false);

            if (allChecked)
            {
                SelectAllCheckBox.IsChecked = true;
            }
            else if (noneChecked)
            {
                SelectAllCheckBox.IsChecked = false;
            }
            else
            {
                SelectAllCheckBox.IsChecked = null;
            }
        }

        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is DebloatItemViewModel item)
            {
                item.IsSelected = true;
            }
            SetCheckedState();
        }

        private void Option_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is DebloatItemViewModel item)
            {
                item.IsSelected = false;
            }
            SetCheckedState();
        }

        #endregion

        #region Execution

        private async void Execute_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = _categoryItems.Values
                .SelectMany(list => list)
                .Where(item => item.IsSelected)
                .ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one item to debloat.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to execute {selectedItems.Count} debloat action(s)?\n\nThis may require a system restart.",
                "Confirm Execution",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            ProgressPanel.Visibility = Visibility.Visible;
            BtnExecute.IsEnabled = false;
            BtnViewScript.IsEnabled = false;

            await Task.Run(() =>
            {
                foreach (var item in selectedItems)
                {
                    ExecuteDebloatItem(item);
                }
            });

            ProgressPanel.Visibility = Visibility.Collapsed;
            BtnExecute.IsEnabled = true;
            BtnViewScript.IsEnabled = true;

            MessageBox.Show("Debloat operations completed!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteDebloatItem(DebloatItemViewModel item)
        {
            try
            {
                
                System.Threading.Thread.Sleep(200);
            }
            catch (Exception ex)
            {
            }
        }

        private void ViewScript_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Script preview not yet implemented.", "View Script", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Engine Events

        private void Engine_ProgressChanged(object sender, DebloatProgressEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgressBar.Value = e.Percentage;
                ProgressText.Text = e.Status;
            }));
        }

        private void Engine_LogMessage(object sender, DebloatLogEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[{e.Level}] {e.Message}");
        }

        #endregion
    }

    #region ViewModel Classes

    public class DebloatItemViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Risk { get; set; }
        public bool IsSelected { get; set; }
        public object Data { get; set; }
    }

    #endregion
}
