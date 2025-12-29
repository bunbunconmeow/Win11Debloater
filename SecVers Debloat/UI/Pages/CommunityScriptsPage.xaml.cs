using iNKORE.UI.WPF.Modern.Common;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;
using Newtonsoft.Json;
using SecVers_Debloat.Schemas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
using JsonSerializer = System.Text.Json.JsonSerializer;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using Path = System.IO.Path;

namespace SecVers_Debloat.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für CommunityScriptsPage.xaml
    /// </summary>
    public partial class CommunityScriptsPage : System.Windows.Controls.Page
    {
        private ObservableCollection<ScriptAddon> Scripts { get; set; }
        private const string ScriptsFile = "Data/community_scripts.json";
        private const string ScriptsFolder = "Data/CommunityScripts";
        private static readonly HttpClient _httpClient = new HttpClient();

        public CommunityScriptsPage()
        {
            InitializeComponent();
            Scripts = new ObservableCollection<ScriptAddon>();
            ScriptsListView.ItemsSource = Scripts;

            // Create scripts folder
            EnsureDirectoriesExist();

            LoadScripts();
            UpdateEmptyState();
        }

        private void EnsureDirectoriesExist()
        {
            try
            {
                var dataDir = Path.GetDirectoryName(ScriptsFile);
                if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }

                if (!Directory.Exists(ScriptsFolder))
                {
                    Directory.CreateDirectory(ScriptsFolder);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to create directories: {ex.Message}");
            }
        }

      

        // ==================== ADD SINGLE SCRIPT ====================
        private async void AddScriptButton_Click(object sender, RoutedEventArgs e)
        {
            var url = UrlInputBox.Text?.Trim();
            if (string.IsNullOrEmpty(url))
            {
                ShowError("Please enter a valid URL");
                return;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                ShowError("Invalid URL format");
                return;
            }

            // Check if already exists
            if (Scripts.Any(s => s.Url == url))
            {
                ShowError("Script already exists in library");
                return;
            }

            try
            {
                ShowStatus("Adding script...", false);

                // Try to get filename from URL
                var fileName = Path.GetFileName(new Uri(url).LocalPath);
                if (string.IsNullOrEmpty(fileName))
                    fileName = "script.ps1";

                var script = new ScriptAddon
                {
                    Title = fileName,
                    Url = url,
                    Sha = string.Empty,
                    Downloaded = false
                };

                Scripts.Add(script);
                SaveScripts();
                UpdateEmptyState();

                UrlInputBox.Clear();
                ShowStatus($"✓ Script '{fileName}' added successfully", false);
            }
            catch (Exception ex)
            {
                ShowError($"Error adding script: {ex.Message}");
            }
        }

        // ==================== IMPORT FROM JSON ====================
        private async void AddFromJsonButton_Click(object sender, RoutedEventArgs e)
        {
            var jsonUrl = JsonUrlInputBox.Text?.Trim();
            if (string.IsNullOrEmpty(jsonUrl))
            {
                ShowError("Please enter a valid JSON URL");
                return;
            }

            try
            {
                ShowStatus("Loading JSON library...", false);

                var response = await _httpClient.GetStringAsync(jsonUrl);
                var collection = JsonSerializer.Deserialize<ScriptAddonsCollection>(response);

                if (collection?.Scripts == null || collection.Scripts.Count == 0)
                {
                    ShowError("No scripts found in JSON");
                    return;
                }

                int addedCount = 0;
                foreach (var script in collection.Scripts)
                {
                    // Skip if already exists
                    if (Scripts.Any(s => s.Url == script.Url))
                        continue;

                    Scripts.Add(new ScriptAddon
                    {
                        Title = script.Title ?? "Unknown Script",
                        Url = script.Url,
                        Sha = script.Sha ?? string.Empty,
                        Downloaded = false
                    });
                    addedCount++;
                }

                SaveScripts();
                UpdateEmptyState();
                JsonUrlInputBox.Clear();

                ShowStatus($"✓ Added {addedCount} new scripts from JSON", false);
            }
            catch (Exception ex)
            {
                ShowError($"Error loading JSON: {ex.Message}");
            }
        }

        // ==================== DOWNLOAD SCRIPT ====================
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ScriptAddon script)
            {
                try
                {
                    ShowStatus($"Downloading {script.Title}...", false);

                    var content = await _httpClient.GetStringAsync(script.Url);
                    var fileName = SanitizeFileName(script.Title);
                    var filePath = Path.Combine(ScriptsFolder, fileName);

                    File.WriteAllText(filePath, content, Encoding.UTF8);

                    // Verify hash if provided
                    if (!string.IsNullOrEmpty(script.Sha))
                    {
                        var hash = ComputeSha256(content);
                        if (!hash.Equals(script.Sha, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(filePath);
                            ShowError("⚠️ Hash verification failed! Script not saved.");
                            return;
                        }
                    }

                    script.Downloaded = true;
                    SaveScripts();
                    ScriptsListView.Items.Refresh();

                    ShowStatus($"✓ {script.Title} downloaded successfully", false);
                }
                catch (Exception ex)
                {
                    ShowError($"Download failed: {ex.Message}");
                }
            }
        }

        // ==================== RUN SCRIPT ====================
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ScriptAddon script)
            {
                var fileName = SanitizeFileName(script.Title);
                var filePath = Path.Combine(ScriptsFolder, fileName);

                if (!File.Exists(filePath))
                {
                    ShowError("Script file not found. Please download it first.");
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to run:\n\n{script.Title}\n\n⚠️ Only run scripts you trust!",
                    "Confirm Script Execution",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-ExecutionPolicy Bypass -File \"{filePath}\"",
                            Verb = "runas",
                            UseShellExecute = true
                        };

                        Process.Start(psi);
                        ShowStatus($"✓ Running {script.Title}...", false);
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Failed to run script: {ex.Message}");
                    }
                }
            }
        }

        // ==================== VIEW SCRIPT ====================
        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ScriptAddon script)
            {
                var fileName = SanitizeFileName(script.Title);
                var filePath = Path.Combine(ScriptsFolder, fileName);

                if (File.Exists(filePath))
                {
                    try
                    {
                        Process.Start("notepad.exe", filePath);
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Failed to open script: {ex.Message}");
                    }
                }
                else
                {
                    ShowError("Script file not found");
                }
            }
        }

        // ==================== DELETE SCRIPT ====================
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ScriptAddon script)
            {
                var result = MessageBox.Show(
                    $"Delete '{script.Title}' from your library?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Delete file if downloaded
                    if (script.Downloaded)
                    {
                        var fileName = SanitizeFileName(script.Title);
                        var filePath = Path.Combine(ScriptsFolder, fileName);
                        if (File.Exists(filePath))
                        {
                            try
                            {
                                File.Delete(filePath);
                            }
                            catch { }
                        }
                    }

                    Scripts.Remove(script);
                    SaveScripts();
                    UpdateEmptyState();
                    ShowStatus($"✓ Deleted {script.Title}", false);
                }
            }
        }

        // ==================== HELPER METHODS ====================
        private void LoadScripts()
        {
            try
            {
                // Debug: Show absolute path
                var absolutePath = Path.GetFullPath(ScriptsFile);
                Debug.WriteLine($"Looking for scripts at: {absolutePath}");

                if (File.Exists(ScriptsFile))
                {
                    var json = File.ReadAllText(ScriptsFile);
                    Debug.WriteLine($"Loaded JSON: {json}");

                    var collection = JsonSerializer.Deserialize<ScriptAddonsCollection>(json);

                    if (collection?.Scripts != null)
                    {
                        Scripts.Clear();
                        foreach (var script in collection.Scripts)
                        {
                            Scripts.Add(script);
                        }
                        Debug.WriteLine($"Loaded {Scripts.Count} scripts");
                    }
                    else
                    {
                        Debug.WriteLine("JSON deserialized but Scripts collection is null");
                    }
                }
                else
                {
                    Debug.WriteLine($"Scripts file not found at: {absolutePath}");
                    // Create empty file for next time
                    SaveScripts();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load scripts: {ex.Message}");
                Debug.WriteLine($"LoadScripts Exception: {ex}");
            }
        }


        private void SaveScripts()
        {
            try
            {
                var collection = new ScriptAddonsCollection { Scripts = Scripts.ToList() };
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(collection, options);

                // Ensure directory exists before writing
                var directory = Path.GetDirectoryName(ScriptsFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(ScriptsFile, json, Encoding.UTF8);
                Debug.WriteLine($"Saved {Scripts.Count} scripts to {ScriptsFile}");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to save scripts: {ex.Message}");
                Debug.WriteLine($"SaveScripts Exception: {ex}");
            }
        }


        private void UpdateEmptyState()
        {
            bool hasScripts = Scripts.Count > 0;
            EmptyStatePanel.Visibility = hasScripts ? Visibility.Collapsed : Visibility.Visible;
            ScriptsListView.Visibility = hasScripts ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowStatus(string message, bool isError)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = isError 
                ? System.Windows.Media.Brushes.Red 
                : System.Windows.Media.Brushes.Green;
            StatusTextBlock.Visibility = Visibility.Visible;
        }

        private void ShowError(string message)
        {
            ShowStatus(message, true);
        }

        private string SanitizeFileName(string fileName)
        {

            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalid));
            
            if (!sanitized.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                sanitized += ".ps1";
                
            return sanitized;
        }

        private string ComputeSha256(string content)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

    }
}
