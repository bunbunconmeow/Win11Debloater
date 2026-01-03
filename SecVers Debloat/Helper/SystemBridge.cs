using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Windows;

namespace SecVers_Debloat.Helper
{
    public class SystemBridge
    {
        private Action<string> _logger;

        public SystemBridge(Action<string> logger)
        {
            _logger = logger;
        }


        public void log(string message)
        {
            _logger?.Invoke(message);
        }


        public string run(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        log($"[CMD-ERR] {fileName}: {error}");
                    }

                    log($"[CMD] {fileName} {arguments} -> ExitCode: {process.ExitCode}");
                    return output;
                }
            }
            catch (Exception ex)
            {
                log($"[ERROR] Run failed: {ex.Message}");
                return null;
            }
        }

        public string ps(string command)
        {
            return run("powershell", $"-NoProfile -NonInteractive -Command \"{command}\"");
        }


        public void killProcess(string processName)
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName(processName))
                {
                    proc.Kill();
                    log($"[PROC] Killed process: {processName} (PID: {proc.Id})");
                }
            }
            catch (Exception ex)
            {
                log($"[ERROR] Could not kill {processName}: {ex.Message}");
            }
        }


        private RegistryKey GetBaseKey(string hive)
        {
            switch (hive.ToUpper())
            {
                case "HKLM": return Registry.LocalMachine;
                case "HKCU": return Registry.CurrentUser;
                case "HKCR": return Registry.ClassesRoot;
                case "HKU": return Registry.Users;
                case "HKCC": return Registry.CurrentConfig;
                default: throw new ArgumentException($"Unknown Hive: {hive}");
            }
        }

        public void regSet(string hive, string path, string name, object value)
        {
            try
            {
                using (RegistryKey baseKey = GetBaseKey(hive))
                using (RegistryKey key = baseKey.CreateSubKey(path, true))
                {
                    if (key != null)
                    {
                      
                        if (value is double dVal)
                            key.SetValue(name, (int)dVal, RegistryValueKind.DWord);
                        else
                            key.SetValue(name, value);

                        log($"[REG-SET] {hive}\\{path}\\{name} = {value}");
                    }
                }
            }
            catch (Exception ex)
            {
                log($"[ERROR] Registry Set: {ex.Message}");
            }
        }

        public object regGet(string hive, string path, string name)
        {
            try
            {
                using (RegistryKey baseKey = GetBaseKey(hive))
                using (RegistryKey key = baseKey.OpenSubKey(path, false))
                {
                    return key?.GetValue(name);
                }
            }
            catch
            {
                return null;
            }
        }
        public void regDel(string hive, string path, string name)
        {
            try
            {
                using (RegistryKey baseKey = GetBaseKey(hive))
                using (RegistryKey key = baseKey.OpenSubKey(path, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(name, false);
                        log($"[REG-DEL] Deleted value {name} in {path}");
                    }
                }
            }
            catch (Exception ex) { log($"[ERROR] Reg Delete: {ex.Message}"); }
        }

        public void svcStop(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status != ServiceControllerStatus.Stopped && sc.Status != ServiceControllerStatus.StopPending)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                        log($"[SVC] Stopped {serviceName}");
                    }
                }
            }
            catch (Exception ex) { log($"[SVC-ERR] Stop {serviceName}: {ex.Message}"); }
        }


        public void svcConfig(string serviceName, string startMode)
        {
            run("cmd", $"/c sc config \"{serviceName}\" start= {startMode}");
        }

        public bool fsExists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        public void fsDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    log($"[FS] Deleted file: {path}");
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, true); // recursive
                    log($"[FS] Deleted folder: {path}");
                }
            }
            catch (Exception ex)
            {
                log($"[FS-ERR] Could not delete {path}: {ex.Message}");
            }
        }

        public void download(string url, string destPath)
        {
            try
            {
                log($"[NET] Downloading {url}...");
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(url, destPath);
                }
                log($"[NET] Download complete: {destPath}");
            }
            catch (Exception ex)
            {
                log($"[NET-ERR] Download failed: {ex.Message}");
            }
        }

        public void sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        public bool ask(string question)
        {
            var result = MessageBox.Show(question, "Script Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public string getOS()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
