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
    internal class SpecialAppRemover
    {
        public static void ForceRemoveEdge()
        {
            KillProcess("msedge");
            KillProcess("edgeupdate");
            KillProcess("msedgewebview2");
            RunCmd("sc stop edgeupdate");
            RunCmd("sc delete edgeupdate");
            RunCmd("sc stop edgeupdatem");
            RunCmd("sc delete edgeupdatem");
            string[] edgePaths = {
                @"C:\Program Files (x86)\Microsoft\Edge",
                @"C:\Program Files (x86)\Microsoft\EdgeCore",
                @"C:\Program Files (x86)\Microsoft\EdgeWebView2",
                @"C:\Program Files (x86)\Microsoft\Temp\Edge"
            };

            foreach (var path in edgePaths)
            {
                NukeFolder(path);
            }
            RemoveShortcuts("Microsoft Edge");
            BlockExeExecution("msedge.exe");
        }

        public static void ForceRemoveOneDrive()
        {
            KillProcess("OneDrive");
            string sysRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string installer64 = Path.Combine(sysRoot, @"SysWOW64\OneDriveSetup.exe");

            if (File.Exists(installer64))
                RunCmd($"\"{installer64}\" /uninstall");

            string localData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\OneDrive");
            NukeFolder(localData);
            NukeFolder(@"C:\ProgramData\Microsoft OneDrive");
            RunCmd("reg delete \"HKEY_CLASSES_ROOT\\CLSID\\{018D5C66-4533-4307-9B53-224DE2ED1FE6}\" /f");
            RunCmd("reg delete \"HKEY_CLASSES_ROOT\\Wow6432Node\\CLSID\\{018D5C66-4533-4307-9B53-224DE2ED1FE6}\" /f");
        }
        public static void ForceRemoveCortana()
        {
            KillProcess("SearchUI");
            KillProcess("Cortana");
            RunCmd("reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Search\" /v AllowCortana /t REG_DWORD /d 0 /f");
            string systemApps = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SystemApps");
            if (Directory.Exists(systemApps))
            {
                foreach (var dir in Directory.GetDirectories(systemApps, "*Cortana*"))
                {
                    NukeFolder(dir);
                }
            }
        }

        private static void KillProcess(string name)
        {
            try
            {
                foreach (var p in Process.GetProcessesByName(name))
                {
                    p.Kill();
                    p.WaitForExit(1000);
                }
            }
            catch { }
        }

        private static void NukeFolder(string path)
        {
            if (!Directory.Exists(path)) return;
            RunCmd($"takeown /f \"{path}\" /a /r /d y");
            RunCmd($"icacls \"{path}\" /grant Administrators:F /t");
            RunCmd($"rd /s /q \"{path}\"");
        }

        private static void RemoveShortcuts(string nameContent)
        {
            string commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
            string userDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string commonStart = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);

            string[] paths = { commonDesktop, userDesktop, commonStart };

            foreach (var root in paths)
            {
                try
                {
                    string[] files = Directory.GetFiles(root, "*.lnk", SearchOption.AllDirectories);
                    foreach (var f in files)
                    {
                        if (Path.GetFileName(f).IndexOf(nameContent, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            File.Delete(f);
                        }
                    }
                }
                catch { }
            }
        }

        private static void BlockExeExecution(string exeName)
        {
            string keyPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{exeName}";
            using (var key = Registry.LocalMachine.CreateSubKey(keyPath))
            {
                key.SetValue("Debugger", "systray.exe");
            }
        }

        public static void RunCmd(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {arguments}",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };
            try
            {
                Process.Start(psi)?.WaitForExit();
            }
            catch { }
        }

        public static void ForceRemoveRecall()
        {
            RunCmd("reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI\" /v DisableAIDataAnalysis /t REG_DWORD /d 1 /f");
            RunCmd("reg add \"HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsAI\" /v DisableAIDataAnalysis /t REG_DWORD /d 1 /f");
            RunCmd("reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI\" /v DisableRecall /t REG_DWORD /d 1 /f");
            RunCmd("reg add \"HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsAI\" /v DisableRecall /t REG_DWORD /d 1 /f");
            RunCmd("reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI\" /v AllowSnapshotting /t REG_DWORD /d 0 /f");
            KillProcess("AIHost");
            RunCmd("sc stop WSearch");

            string sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string aiHostPath = Path.Combine(sys32, "AIHost.exe");

            if (File.Exists(aiHostPath))
            {
                RunCmd($"takeown /f \"{aiHostPath}\" /a");
                RunCmd($"icacls \"{aiHostPath}\" /grant Administrators:F");
                RunCmd($"del /f /q \"{aiHostPath}\"");
                if (File.Exists(aiHostPath))
                {
                    RunCmd($"ren \"{aiHostPath}\" \"AIHost.exe.bak\"");
                }
            }

            BlockExeExecution("AIHost.exe");
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

       
            NukeFolder(Path.Combine(localAppData, @"Microsoft\Windows\Recall"));
            NukeFolder(Path.Combine(localAppData, @"Microsoft\Windows\AI"));
            NukeFolder(Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Edge Copilot"));
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); 
            NukeFolder(Path.Combine(programData, @"Microsoft\Search\Data\Applications\Windows\GatherLogs\SystemIndex\Sites\Recall"));
        }
    }
}
