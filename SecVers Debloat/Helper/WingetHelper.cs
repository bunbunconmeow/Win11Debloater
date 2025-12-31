using SecVers_Debloat.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecVers_Debloat.Helpers
{
    public class WingetHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string AppInstallerUrl = "https://aka.ms/getwinget";
        private readonly string _localWingetPath;

        public bool IsWingetAvailable { get; private set; }

        public WingetHelper()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _localWingetPath = Path.Combine(localAppData, @"Microsoft\WindowsApps\winget.exe");

            IsWingetAvailable = CheckWingetAvailability();

            // Versuche Installation, falls nicht vorhanden
            if (!IsWingetAvailable)
            {
                try
                {
                    Task.Run(async () => await InstallWingetAsync()).Wait();
                    IsWingetAvailable = CheckWingetAvailability();
                }
                catch
                {
                    IsWingetAvailable = false;
                }
            }
        }

        private bool CheckWingetAvailability()
        {
            if (File.Exists(_localWingetPath)) return true;
            return RunCommandBoolSync("winget", "--version"); // Benutze optimierte Sync Methode
        }

        public string GetWingetVersion()
        {
            string exe = File.Exists(_localWingetPath) ? _localWingetPath : "winget";
            string output = RunCommandStringSync(exe, "--version"); // Benutze die String-Methode
            return string.IsNullOrWhiteSpace(output) ? "Unknown" : output.Trim();
        }

        public async Task<int> InstallPackagesAsync(string[] packageIds, bool silent = true)
        {
            int successCount = 0;
            // Parallele Installationen vermeiden, Winget sperrt oft die DB. Sequential ist sicherer.
            foreach (var packageId in packageIds)
            {
                bool success = await InstallPackageAsync(packageId, silent);
                if (success)
                {
                    successCount++;
                }
            }
            return successCount;
        }

        public async Task<bool> InstallPackageAsync(string packageId, bool silent = true)
        {
            if (!IsWingetAvailable) return false;

            string exe = File.Exists(_localWingetPath) ? _localWingetPath : "winget";

            // WICHTIG: --force und --disable-interactivity verhindern Hänger bei Lizenzfragen
            string args = silent
                ? $"install --id {packageId} --silent --accept-package-agreements --accept-source-agreements --force --disable-interactivity --source winget"
                : $"install --id {packageId} --accept-package-agreements --accept-source-agreements --force --disable-interactivity --source winget";

            return await RunCommandBoolAsync(exe, args);
        }

        private async Task InstallWingetAsync()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "Microsoft.DesktopAppInstaller.msixbundle");

            try
            {
                using (var response = await _httpClient.GetAsync(AppInstallerUrl))
                {
                    response.EnsureSuccessStatusCode();
                    using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }

                string psCommand = $"Add-AppxPackage -Path '{tempPath}' -ForceApplicationShutdown -ForceUpdateFromAnyVersion";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -NonInteractive -Command \"{psCommand}\"",
                    RedirectStandardOutput = false, // Hier nicht nötig
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = startInfo };
                process.Start();
                await process.WaitForExitAsync();

                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
            catch
            {
               
            }
        }


        private async Task<bool> RunCommandBoolAsync(string fileName, string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = false, // WICHTIGER FIX
                    RedirectStandardError = false,  // WICHTIGER FIX
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = startInfo };
                process.Start();
                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        // Hilfsmethode: Sync check (nur false/true)
        private bool RunCommandBoolSync(string fileName, string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch { return false; }
        }

        private string RunCommandStringSync(string fileName, string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true, 
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8 
                };

                using (var process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return output;
                }
            }
            catch { return string.Empty; }
        }
    }
}
