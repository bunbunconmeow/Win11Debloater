using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SecVers_Debloat.Patches.Debloater
{
    public class InstalledApp
    {
        public string DisplayName { get; set; }
        public string Publisher { get; set; }
        public string InstallDate { get; set; }
        public string UninstallString { get; set; }
        public string InstallLocation { get; set; }
        public string RegistryPath { get; set; }
        public bool IsSelected { get; set; }
    }

    public static class AppManager
    {
        public static List<InstalledApp> GetInstalledApps()
        {
            var apps = new List<InstalledApp>();
            string[] registryKeys = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var keyPath in registryKeys)
            {
                ReadRegistryLocation(Registry.LocalMachine, keyPath, apps);
                ReadRegistryLocation(Registry.CurrentUser, keyPath, apps);
            }

            apps.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName));
            return apps;
        }

        private static void ReadRegistryLocation(RegistryKey root, string keyPath, List<InstalledApp> list)
        {
            using (RegistryKey key = root.OpenSubKey(keyPath))
            {
                if (key == null) return;

                foreach (string subkeyName in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                    {
                        if (subkey == null) continue;

                        string name = subkey.GetValue("DisplayName") as string;
                        string uninstallString = subkey.GetValue("UninstallString") as string;
                        string installLoc = subkey.GetValue("InstallLocation") as string;

                        if (!string.IsNullOrEmpty(name))
                        {
                            if (list.Exists(x => x.DisplayName == name)) continue;

                            list.Add(new InstalledApp
                            {
                                DisplayName = name,
                                Publisher = subkey.GetValue("Publisher") as string ?? "",
                                InstallDate = subkey.GetValue("InstallDate") as string ?? "",
                                UninstallString = uninstallString ?? "",
                                InstallLocation = installLoc, 
                                RegistryPath = $"{keyPath}\\{subkeyName}",
                                IsSelected = false
                            });
                        }
                    }
                }
            }
        }

        public static string UninstallApp(InstalledApp app)
        {
            if (string.IsNullOrEmpty(app.InstallLocation))
            {
                return $"Cannot removed '{app.DisplayName}' automatically because the Install Location is missing in Registry.\\n\\nPlease uninstall manually.\", \"Path Error\";";
            }

            string result = ForceUninstaller.RemoveAppAggressively(app);

            return result;
        }
    }
}
