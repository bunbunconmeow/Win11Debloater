using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace SecVers_Debloat.Helpers
{
    public class WingetHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string WingetCheckCommand = "winget --version";
        private const string AppInstallerUrl = "https://aka.ms/getwinget";

        public bool IsWingetAvailable { get; private set; }

        public WingetHelper()
        {
            IsWingetAvailable = CheckWingetAvailability();

            if (!IsWingetAvailable)
            {
                Console.WriteLine("Winget not found. Attempting to install...");
                InstallWingetAsync().GetAwaiter().GetResult();
                IsWingetAvailable = CheckWingetAvailability();

                if (!IsWingetAvailable)
                {
                    throw new Exception("Failed to install Winget. Please install manually from Microsoft Store.");
                }
            }

            Console.WriteLine($"Winget is available. Version: {GetWingetVersion()}");
        }

        private bool CheckWingetAvailability()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {WingetCheckCommand}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output);
            }
            catch
            {
                return false;
            }
        }

        private string GetWingetVersion()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {WingetCheckCommand}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string version = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                return version;
            }
            catch
            {
                return "Unknown";
            }
        }

        private async Task InstallWingetAsync()
        {
            try
            {
                Console.WriteLine("Downloading Winget installer...");

                string tempPath = Path.Combine(Path.GetTempPath(), "Microsoft.DesktopAppInstaller.msixbundle");

                using (var response = await _httpClient.GetAsync(AppInstallerUrl))
                {
                    response.EnsureSuccessStatusCode();
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    await WriteAllBytesAsync(tempPath, fileBytes); // Eigene Methode für .NET Framework
                }

                Console.WriteLine("Installing Winget...");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"Add-AppxPackage -Path '{tempPath}'\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };

                process.Start();
                string output = await ReadToEndAsync(process.StandardOutput);
                string error = await ReadToEndAsync(process.StandardError);
                await Task.Run(() => process.WaitForExit()); // .NET Framework kompatibel

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Installation failed: {error}");
                }

                File.Delete(tempPath);
                Console.WriteLine("Winget installed successfully.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to install Winget: {ex.Message}", ex);
            }
        }

        public async Task<bool> InstallPackageAsync(string packageId, bool silent = true)
        {
            if (!IsWingetAvailable)
            {
                throw new InvalidOperationException("Winget is not available.");
            }

            try
            {
                string arguments = silent
                    ? $"install --id {packageId} --silent --accept-package-agreements --accept-source-agreements"
                    : $"install --id {packageId} --accept-package-agreements --accept-source-agreements";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await ReadToEndAsync(process.StandardOutput);
                string error = await ReadToEndAsync(process.StandardError);
                await Task.Run(() => process.WaitForExit());

                Console.WriteLine(output);

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Error installing {packageId}: {error}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during installation of {packageId}: {ex.Message}");
                return false;
            }
        }

        public async Task<int> InstallPackagesAsync(string[] packageIds, bool silent = true)
        {
            int successCount = 0;

            foreach (var packageId in packageIds)
            {
                Console.WriteLine($"Installing {packageId}...");
                bool success = await InstallPackageAsync(packageId, silent);

                if (success)
                {
                    successCount++;
                    Console.WriteLine($"✓ {packageId} installed successfully.");
                }
                else
                {
                    Console.WriteLine($"✗ {packageId} installation failed.");
                }
            }

            return successCount;
        }

        public async Task<string> SearchPackagesAsync(string query)
        {
            if (!IsWingetAvailable)
            {
                throw new InvalidOperationException("Winget is not available.");
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "winget",
                    Arguments = $"search {query}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await ReadToEndAsync(process.StandardOutput);
            await Task.Run(() => process.WaitForExit());

            return output;
        }

        public async Task<bool> UninstallPackageAsync(string packageId, bool silent = true)
        {
            if (!IsWingetAvailable)
            {
                throw new InvalidOperationException("Winget is not available.");
            }

            try
            {
                string arguments = silent
                    ? $"uninstall --id {packageId} --silent"
                    : $"uninstall --id {packageId}";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await Task.Run(() => process.WaitForExit());

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<string> ReadToEndAsync(StreamReader reader)
        {
            return await Task.Run(() => reader.ReadToEnd());
        }

        private static async Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            await Task.Run(() => File.WriteAllBytes(path, bytes));
        }
    }
}