using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVers_Debloat.Helper
{
    internal class AutostartHelper
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "LHE";

        public static void EnableAutostart(string exePath)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true))
            {
                if (key == null)
                    throw new InvalidOperationException("Unable to open Run key.");

                key.SetValue(AppName, $"\"{exePath}\"");
            }
        }

        public static void DisableAutostart()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true))
            {
                if (key == null)
                    return;

                if (key.GetValue(AppName) != null)
                    key.DeleteValue(AppName, throwOnMissingValue: false);
            }
        }

        public static bool IsAutostartEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false))
            {
                if (key == null)
                    return false;

                return key.GetValue(AppName) != null;
            }
        }
    }
}
