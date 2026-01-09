using SecVerseLHE.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using SecVerseLHE.Helper;

namespace SecVerseLHE.Core
{
    // Should intercept Python, Java, NodeJS interpreters from running unapproved scripts
    internal class IntercepterGuard
    {
        private ManagementEventWatcher _startWatch;
        private TrayManager _ui;
        private bool _useWhitelist = false;

        #region HashSets
        private readonly HashSet<string> _watchedBinaries = new HashSet<string>
        {
            "python.exe", "pythonw.exe",        // Python
            "java.exe", "javaw.exe",            // Java
            "node.exe",                         // NodeJS
            "cmd.exe",                          // Command Prompt
            "powershell.exe", "pwsh.exe",       // PowerShell Core
            "wscript.exe", "cscript.exe",       // Windows Script Host
            "mshta.exe",                        // HTML Apps
            "curl.exe", "wget.exe",             // Commandline Downloader
            "bitsadmin.exe", "certutil.exe"     // Windows LOLBins
        };

        // Can be Bypassed if launched from these paths. So its optional but by default disabled.
        private readonly List<string> _whitelistPaths = new List<string>
        {
            "steamapps",
            @".minecraft",
            @"program files",
            @"windows\system32"
        };
        #endregion HashSets

        public void SetWhitelistEnabled(bool enable) => _useWhitelist = enable;

        public void StartMonitoring(TrayManager ui)
        {
            _ui = ui;
            try
            {
                var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
                _startWatch = new ManagementEventWatcher(query);
                _startWatch.EventArrived += OnProcessStarted;
                _startWatch.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InterpreterGuard] Init Error: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (_startWatch != null)
            {
                try
                {
                    _startWatch.Stop();
                    _startWatch.Dispose();
                    _startWatch = null;
                }
                catch { }
            }
        }

        private void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    string procName = e.NewEvent.Properties["ProcessName"].Value?.ToString().ToLower();
                    if (procName == null || !_watchedBinaries.Contains(procName)) return;

                    int pid = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                    ScanProcess(pid, procName);
                }
                catch { }
            });
        }


        private void ScanProcess(int pid, string procName)
        {
            try
            {
                string commandLine = GetCommandLine(pid).ToLower();
                string exePath = GetProcessPath(pid).ToLower();

                if (string.IsNullOrEmpty(exePath)) return;

                bool kill = false;
                string reason = "";

                if (IsPathRisky(exePath) && !CheckWhitelist(exePath))
                {
                    kill = true;
                    reason = $"Untrusted Interpreter Location: {exePath}";
                }

                if (!kill && IsNetworkCommand(commandLine))
                {
                    kill = true;
                    reason = $"Blocked Console Network Request: {commandLine}";
                }

                if (!kill && IsPathRisky(commandLine) && !CheckWhitelist(commandLine))
                {
                    kill = true;
                    reason = $"Execution of Temp/AppData Script detected.";
                }

                if (!kill && IsObfuscated(commandLine))
                {
                    kill = true;
                    reason = $"Obfuscated Command detected.";
                }

                if (kill)
                {
                    Process.GetProcessById(pid).Kill();
                    _ui.ShowAlert("SCRIPT BLOCKED", $"{reason}\nProcess: {procName}\n(Whitelist: {_useWhitelist})");
                }
            }
            catch { }
        }

        private bool CheckWhitelist(string path)
        {
          
            if (!_useWhitelist) return false;

            foreach (var w in _whitelistPaths)
            {
                if (path.Contains(w.ToLower())) return true;
            }
            return false;
        }

        private bool IsPathRisky(string path)
        {
            return path.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToLower()) ||
                   path.Contains(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToLower()) ||
                   path.Contains(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).ToLower()) ||
                   path.Contains(@"\programdata\") ||
                   path.Contains("appdata") ||
                   path.Contains("LocalLow");
        }

        private bool IsNetworkCommand(string cmd)
        {
            return cmd.Contains("http:") ||
                   cmd.Contains("https:") ||
                   cmd.Contains("curl ") ||
                   cmd.Contains("wget ") ||
                   cmd.Contains("bitsadmin ") ||
                   cmd.Contains("Invoke-WebRequest") ||
                   cmd.Contains("Invoke-RestMethod") ||
                   cmd.Contains("jar") ||
                   cmd.Contains("certutil ") ||
                   cmd.Contains("iwr ") ||
                   (cmd.Contains("download") && cmd.Contains("net.webclient"));
        }

        private bool IsObfuscated(string cmd)
        {
            return cmd.Contains(" -enc ") ||
                   cmd.Contains(" -encodedcommand ") ||
                     cmd.Contains("%00") ||
                     cmd.Contains("\\u") ||
                        cmd.Contains("\\x") ||
                        cmd.Contains("--startup") ||
                   (cmd.Contains(" -windowstyle hidden") && !cmd.Contains("system32"));
        }

        private string GetCommandLine(int pid)
        {
            try
            {
                using (var s = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {pid}"))
                using (var c = s.Get()) { foreach (var o in c) return o["CommandLine"]?.ToString() ?? ""; }
            }
            catch { }
            return "";
        }

        private string GetProcessPath(int pid)
        {
            try { return Process.GetProcessById(pid).MainModule.FileName; }
            catch
            {
                try
                {
                    using (var s = new ManagementObjectSearcher($"SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = {pid}"))
                    using (var c = s.Get()) { foreach (var o in c) return o["ExecutablePath"]?.ToString() ?? ""; }
                }
                catch { }
            }
            return "";
        }
    }
}
