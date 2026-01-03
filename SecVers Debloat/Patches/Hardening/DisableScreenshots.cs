using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

internal class DisableScreenshots
{
    public static void Execute()
    {
        try
        {
            DisableHardwarePrintScreenKey();
            SetAntiScreenshotPolicies();
            DisableClipboardHistory();
            NukeScreenshotApps();
            BlockScreenshotProcesses();
        }
        catch {}
    }

    private static void DisableHardwarePrintScreenKey()
    {
        string keyPath = @"SYSTEM\CurrentControlSet\Control\Keyboard Layout";

        byte[] scancodeMap = new byte[] {
                0x00, 0x00, 0x00, 0x00,     // Header Version
                0x00, 0x00, 0x00, 0x00,     // Flags
                0x02, 0x00, 0x00, 0x00,     // Count (2)
                0x00, 0x00, 0x37, 0xE0,     // Mapping: PrtScn -> Null
                0x00, 0x00, 0x00, 0x00      // Terminator
            };

        try
        {
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath))
            {
                if (key != null)
                {
                    key.SetValue("Scancode Map", scancodeMap, RegistryValueKind.Binary);
                }
            }
        }
        catch {  }
    }

    private static void SetAntiScreenshotPolicies()
    {
        SetRegValue(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\System", "DisableScreenCapture", 1);
        SetRegValue(@"HKLM\SOFTWARE\Policies\Microsoft\TabletPC", "DisableSnippingTool", 1);
        SetRegValue(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0);
        SetRegValue(@"HKCU\System\GameConfigStore", "GameDVR_Enabled", 0);
    }

    private static void DisableClipboardHistory()
    {
        SetRegValue(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\System", "AllowClipboardHistory", 0);
        SetRegValue(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\System", "AllowCrossDeviceClipboard", 0);
    }

    private static void NukeScreenshotApps()
    {
        string[] appsToRemove = new string[]
        {
                "*ScreenSketch*",
                "*SnippingTool*",
                "*XboxGamingOverlay*",
                "*Microsoft.Windows.Photos*"
        };

        foreach (var app in appsToRemove)
        {
            RunPowerShell($"Get-AppxPackage {app} | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue");
        }
        Console.WriteLine(" - [OK] Screenshot-Apps (Appx) entfernt.");
    }

    private static void BlockScreenshotProcesses()
    {
        string[] targets = new string[]
        {
                "SnippingTool.exe",
                "ScreenClip.exe",
                "psr.exe",
                "GameBar.exe",
                "GameBarFT.exe",
                "bcastdvr.exe"
        };

        string ifeoPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";

        foreach (var exe in targets)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(Path.Combine(ifeoPath, exe)))
                {
                    key.SetValue("Debugger", "systray.exe");
                }
            }
            catch { }
        }
        Console.WriteLine(" - [OK] Ausführung von Screenshot-Prozessen blockiert.");
    }


    private static void SetRegValue(string keyPath, string valueName, int value)
    {
        try
        {
            RegistryKey root = keyPath.StartsWith("HKLM") ? Registry.LocalMachine : Registry.CurrentUser;
            string subKey = keyPath.Substring(5);

            using (RegistryKey key = root.CreateSubKey(subKey))
            {
                if (key != null)
                {
                    key.SetValue(valueName, value, RegistryValueKind.DWord);
                }
            }
        }
        catch (Exception e)
        {
           
        }
    }

    private static void RunPowerShell(string command)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(psi)?.WaitForExit();
        }
        catch { }
    }
}