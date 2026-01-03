using System;
using System.Diagnostics;
using System.IO;


namespace SecVerseLHE.Core
{
    internal class Enforcer
    {
        public static void Neutralize(Process p, string path, UI.TrayManager ui)
        {
            try
            {
                p.Kill();
                ui.ShowAlert("Enforcer Info", $"Process Killed: {p.ProcessName}");

                if (File.Exists(path))
                {
                    string quarantinePath = path + ".LOCKED";
                    if (File.Exists(quarantinePath)) File.Delete(quarantinePath);
                    File.Move(path, quarantinePath);
                    File.SetAttributes(quarantinePath, FileAttributes.Hidden);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Enforce Error: {ex.Message}");
            }
        }
    }
}
