using iNKORE.UI.WPF.Modern.Common;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using SecVers_Debloat.Helper;
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

        // JS Executor Instance
        private readonly ScriptExecutor _jsExecutor;

        public CommunityScriptsPage()
        {
            InitializeComponent();
            Scripts = new ObservableCollection<ScriptAddon>();
            ScriptsListView.ItemsSource = Scripts;

            // Init JS Executor
            _jsExecutor = new ScriptExecutor();
            _jsExecutor.OnLogMessage += (s, msg) => Dispatcher.Invoke(() => ShowStatus(msg, false));

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
                    Directory.CreateDirectory(dataDir);

                if (!Directory.Exists(ScriptsFolder))
                    Directory.CreateDirectory(ScriptsFolder);
            }
            catch (Exception ex)
            {
                ShowError($"Init Error: {ex.Message}");
            }
        }

        // ==================== IMPORT LOCAL FILE ====================
        private void BrowseLocalButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Script Files (*.ps1;*.js)|*.ps1;*.js|All files (*.*)|*.*",
                Title = "Select a script"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string sourcePath = openFileDialog.FileName;
                    string fileName = Path.GetFileName(sourcePath);
                    string destPath = Path.Combine(ScriptsFolder, fileName);

                    // Prevent overwrite check for simplicity or handle it
                    if (File.Exists(destPath))
                    {
                        fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now.Ticks}{Path.GetExtension(fileName)}";
                        destPath = Path.Combine(ScriptsFolder, fileName);
                    }

                    File.Copy(sourcePath, destPath);

                    var script = new ScriptAddon
                    {
                        Title = fileName,
                        Url = "Local File",
                        Sha = "",
                        Downloaded = true // Treat local as downloaded
                    };

                    Scripts.Add(script);
                    SaveScripts();
                    UpdateEmptyState();
                    ShowStatus($"✓ Import successful: {fileName}", false);
                }
                catch (Exception ex)
                {
                    ShowError($"Import failed: {ex.Message}");
                }
            }
        }

        // ==================== ADD URL ====================
        private void AddScriptButton_Click(object sender, RoutedEventArgs e)
        {
            var url = UrlInputBox.Text?.Trim();
            if (string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                ShowError("Invalid URL");
                return;
            }

            if (Scripts.Any(s => s.Url == url))
            {
                ShowError("Script already exists");
                return;
            }

            try
            {
                var fileName = Path.GetFileName(new Uri(url).LocalPath);
                // Basic cleanup
                if (string.IsNullOrEmpty(fileName) || (!fileName.EndsWith(".js") && !fileName.EndsWith(".ps1")))
                    fileName = "script_download.ps1";

                var script = new ScriptAddon
                {
                    Title = fileName,
                    Url = url,
                    Downloaded = false
                };

                Scripts.Add(script);
                SaveScripts();
                UpdateEmptyState();
                UrlInputBox.Clear();
                ShowStatus("✓ Script added to library", false);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        // ==================== JSON IMPORT ====================
        private async void AddFromJsonButton_Click(object sender, RoutedEventArgs e)
        {
            var jsonUrl = JsonUrlInputBox.Text?.Trim();
            if (string.IsNullOrEmpty(jsonUrl)) return;

            try
            {
                ShowStatus("Fetching JSON...", false);
                var response = await _httpClient.GetStringAsync(jsonUrl);
                var collection = JsonSerializer.Deserialize<ScriptAddonsCollection>(response);

                if (collection?.Scripts != null)
                {
                    int count = 0;
                    foreach (var s in collection.Scripts)
                    {
                        if (Scripts.Any(ex => ex.Url == s.Url)) continue;

                        // Default logic if title missing
                        if (string.IsNullOrEmpty(s.Title))
                            s.Title = Path.GetFileName(s.Url);

                        s.Downloaded = false;
                        Scripts.Add(s);
                        count++;
                    }
                    SaveScripts();
                    UpdateEmptyState();
                    JsonUrlInputBox.Clear();
                    ShowStatus($"✓ Imported {count} scripts", false);
                }
            }
            catch (Exception ex)
            {
                ShowError($"JSON Error: {ex.Message}");
            }
        }

        // ==================== DOWNLOAD ====================
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ScriptAddon script)
            {
                try
                {
                    ShowStatus($"Downloading {script.Title}...", false);
                    string extension = GetExtension(script.Title);

                    var content = await _httpClient.GetStringAsync(script.Url);
                    string fileName = SanitizeFileName(script.Title, extension);
                    string destPath = Path.Combine(ScriptsFolder, fileName);

                    // Update Title to match file if sanitized changed it
                    script.Title = fileName;

                    File.WriteAllText(destPath, content, Encoding.UTF8);

                    if (!string.IsNullOrEmpty(script.Sha))
                    {
                        var hash = ComputeSha256(content);
                        if (!hash.Equals(script.Sha, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(destPath);
                            ShowError("Hash Mismatch! File deleted.");
                            return;
                        }
                    }

                    script.Downloaded = true;
                    SaveScripts();
                    ScriptsListView.Items.Refresh();
                    ShowStatus("✓ Download Complete", false);
                }
                catch (Exception ex)
                {
                    ShowError($"Download Failed: {ex.Message}");
                }
            }
        }

        // ==================== EXECUTE LOGIC ====================
        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ScriptAddon script)
            {
                string filePath = Path.Combine(ScriptsFolder, script.Title); // Title matches filename

                if (!File.Exists(filePath))
                {
                    ShowError("File missing. Try deleting and re-adding.");
                    return;
                }

                var confirmed = MessageBox.Show(
                    $"Run: {script.Title}\n\n⚠️ Caution: Scripts run with system privileges.",
                    "Confirm Execution", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirmed != MessageBoxResult.Yes) return;

                // CHECK TYPE
                string ext = Path.GetExtension(filePath).ToLower();

                if (ext == ".js")
                {
                    // === JAVASCRIPT (JINT) ===
                    ShowStatus("Running JavaScript Engine...", false);
                    try
                    {
                        // The executor fires OnLogMessage, which updates UI
                        await _jsExecutor.ExecuteScriptFileAsync(filePath);
                    }
                    catch (Exception ex)
                    {
                        ShowError($"JS Runtime Error: {ex.Message}");
                    }
                }
                else if (ext == ".ps1")
                {
                    // === POWERSHELL ===
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{filePath}\"",
                            Verb = "runas",
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                        ShowStatus("✓ PowerShell Triggered", false);
                    }
                    catch (Exception ex)
                    {
                        ShowError($"PS Error: {ex.Message}");
                    }
                }
                else
                {
                    ShowError("Unknown file type. Cannot execute.");
                }
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ScriptAddon script)
            {
                string filePath = Path.Combine(ScriptsFolder, script.Title);
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo("notepad.exe", filePath) { UseShellExecute = true });
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ScriptAddon script)
            {
                if (MessageBox.Show("Delete script?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (script.Downloaded)
                    {
                        string p = Path.Combine(ScriptsFolder, script.Title);
                        if (File.Exists(p)) File.Delete(p);
                    }
                    Scripts.Remove(script);
                    SaveScripts();
                    UpdateEmptyState();
                }
            }
        }

        // ==================== HELPERS ====================

        private string GetExtension(string title)
        {
            if (title.EndsWith(".js", StringComparison.OrdinalIgnoreCase)) return ".js";
            return ".ps1";
        }

        private string SanitizeFileName(string fileName, string extension)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalid));

            // Remove existing extensions to avoid script.js.ps1
            if (sanitized.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                sanitized = sanitized.Substring(0, sanitized.Length - 4);
            if (sanitized.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                sanitized = sanitized.Substring(0, sanitized.Length - 3);

            return sanitized + extension;
        }

        private void LoadScripts()
        {
            if (!File.Exists(ScriptsFile)) return;
            try
            {
                var json = File.ReadAllText(ScriptsFile);
                var col = JsonSerializer.Deserialize<ScriptAddonsCollection>(json);
                if (col?.Scripts != null)
                {
                    Scripts.Clear();
                    foreach (var s in col.Scripts)
                    {
                       
                        Scripts.Add(s);
                    }
                }
            }
            catch (Exception ex) { ShowError("Load Error: " + ex.Message); }
        }

        private void SaveScripts()
        {
            var col = new ScriptAddonsCollection { Scripts = Scripts.ToList() };
            var json = JsonSerializer.Serialize(col, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ScriptsFile, json);
        }

        private void UpdateEmptyState()
        {
            bool has = Scripts.Count > 0;
            EmptyStatePanel.Visibility = has ? Visibility.Collapsed : Visibility.Visible;
            ScriptsListView.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowStatus(string msg, bool err)
        {
            StatusTextBlock.Text = msg;
            StatusTextBlock.Foreground = err ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;
            StatusTextBlock.Visibility = Visibility.Visible;
        }

        private void ShowError(string msg) => ShowStatus("❌ " + msg, true);

        private string ComputeSha256(string content)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
