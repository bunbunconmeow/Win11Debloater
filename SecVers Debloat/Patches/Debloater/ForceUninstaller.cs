using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Patches.Debloater
{
    internal class ForceUninstaller
    {
        public static string RemoveAppAggressively(InstalledApp app)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(app.InstallLocation) || !Directory.Exists(app.InstallLocation))
                {
                    return $"SKIPPED: Could not find install path for '{app.DisplayName}'. Standard uninstall required.";
                }

                KillRunningProcesses(app.InstallLocation);
                CleanupShortcuts(app.DisplayName);
                try
                {
                    Directory.Delete(app.InstallLocation, true);
                }
                catch (Exception ex)
                {
                    return $"ERROR removing files for '{app.DisplayName}': {ex.Message}";
                }

              
                RemoveRegistryEntry(app.RegistryPath);

                return $"SUCCESS: '{app.DisplayName}' was forcefully removed.";
            }
            catch (Exception ex)
            {
                return $"CRITICAL: Unexpected error removing '{app.DisplayName}': {ex.Message}";
            }
        }

        private static void KillRunningProcesses(string installPath)
        {
            var processes = Process.GetProcesses();
            foreach (var p in processes)
            {
                try
                {
                    if (p.MainModule != null && p.MainModule.FileName.StartsWith(installPath, StringComparison.OrdinalIgnoreCase))
                    {
                        p.Kill();
                        p.WaitForExit(3000);
                    }
                }
                catch { }
            }
        }

        private static void RemoveRegistryEntry(string fullRegistryPath)
        {
            if (string.IsNullOrEmpty(fullRegistryPath)) return;
            DeleteKey(Registry.LocalMachine, fullRegistryPath);
            DeleteKey(Registry.CurrentUser, fullRegistryPath);
        }

        private static void DeleteKey(RegistryKey rootKey, string path)
        {
            try
            {
                // Pfad aufteilen in Parent und KeyName
                int lastSlash = path.LastIndexOf('\\');
                if (lastSlash > 0)
                {
                    string parentPath = path.Substring(0, lastSlash);
                    string keyName = path.Substring(lastSlash + 1);

                    using (RegistryKey parent = rootKey.OpenSubKey(parentPath, true))
                    {
                        if (parent != null)
                        {
                            parent.DeleteSubKeyTree(keyName, false);
                        }
                    }
                }
            }
            catch {}
        }

        private static void CleanupShortcuts(string appName)
        {

            string[] locations = {
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.Startup)
            };

            string cleanName = string.Join("", appName.Split(Path.GetInvalidFileNameChars()));

            foreach (string dir in locations)
            {
                if (!Directory.Exists(dir)) continue;
                try
                {
                    var files = Directory.GetFiles(dir, "*.lnk", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        if (fileName.IndexOf(cleanName, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            try { File.Delete(file); } catch { }
                        }
                    }
                }
                catch { }
            }
        }

    }
}
