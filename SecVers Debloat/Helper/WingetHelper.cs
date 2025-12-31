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
            return RunCommand("winget", "--version", out _);
        }

        public string GetWingetVersion()
        {
            string exe = File.Exists(_localWingetPath) ? _localWingetPath : "winget";
            if (RunCommand(exe, "--version", out string output))
            {
                return output.Trim();
            }
            return "Unknown";
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
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = startInfo };
                process.Start();
                await process.WaitForExitAsync();

                if (File.Exists(tempPath)) File.Delete(tempPath);

                if (process.ExitCode != 0)
                {
                    throw new Exception("Installation failed.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Winget Install Exception: {ex.Message}");
            }
        }

        public async Task<bool> InstallPackageAsync(string packageId, bool silent = true)
        {
            if (!IsWingetAvailable) return false;

            string exe = File.Exists(_localWingetPath) ? _localWingetPath : "winget";
            string args = silent
                ? $"install --id {packageId} --silent --accept-package-agreements --accept-source-agreements --force"
                : $"install --id {packageId} --accept-package-agreements --accept-source-agreements";

            return await RunCommandAsync(exe, args);
        }

        public async Task<int> InstallPackagesAsync(string[] packageIds, bool silent = true)
        {
            int successCount = 0;

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

        private async Task<bool> RunCommandAsync(string fileName, string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8
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

        private bool RunCommand(string fileName, string arguments, out string output)
        {
            output = "";
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
                    output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
