using SecVerseLHE.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace SecVerseLHE.Core
{
    internal class ProcessGenealogy
    {
        private ManagementEventWatcher _startWatch;
        private TrayManager _ui;
        #region HashSets
        private readonly HashSet<string> _suspiciousChildren = new HashSet<string>
        {
            "powershell.exe",
            "pwsh.exe",        // PowerShell Core
            "cmd.exe",
            "wscript.exe",
            "cscript.exe",
            "mshta.exe",       // Microsoft HTML Application Host
            "regsvr32.exe",    // Register Server
            "rundll32.exe",
            "certutil.exe",    // Download files, decode base64 etc.
            "bitsadmin.exe",
            "schtasks.exe",    // Create scheduled tasks -> Mostly unused nowadays 
            "scrcons.exe",     // WMI Standard Consumer
            "bash.exe",        // WSL, falls installiert
            "hh.exe"           // HTML Help (Script Execution and stuff like that)
        };
        private readonly HashSet<string> _vulnerableParents = new HashSet<string>
        {
            "winword.exe",
            "excel.exe",
            "outlook.exe",
            "powerpnt.exe",    // PowerPoint
            "msaccess.exe",    // Access
            "mspub.exe",       // Publisher
            "visio.exe",       // Visio
            "acrord32.exe",    // Adobe Reader 32-bit
            "acrobat.exe",     // Adobe Acrobat
            "foxitreader.exe", // Foxit PDF Reader
            "wmplayer.exe"     // Windows Media Player
        };
        #endregion HashSets

        public void StartMonitoring(TrayManager ui)
        {
            _ui = ui;
            var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
            _startWatch = new ManagementEventWatcher(query);
            _startWatch.EventArrived += ProcessStarted;
            _startWatch.Start();
        }

        private void ProcessStarted(object sender, EventArrivedEventArgs e)
        {
            try
            {
                string childName = e.NewEvent.Properties["ProcessName"].Value?.ToString().ToLower();
                if (childName != null && _suspiciousChildren.Contains(childName))
                {
                    int parentId = Convert.ToInt32(e.NewEvent.Properties["ParentProcessID"].Value);
                    int childId = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                    Process parent = Process.GetProcessById(parentId);
                    string parentName = parent.ProcessName.ToLower();

                    if (!parentName.EndsWith(".exe")) parentName += ".exe";

                    if (_vulnerableParents.Contains(parentName))
                    {
                        // Kills Child before the parent since they can start another process. 
                        try { Process.GetProcessById(childId).Kill(); }
                        catch { }

                        try { parent.Kill(); }
                        catch { }

                        _ui.ShowAlert("Security Alert",
                            $"Blocked suspicious activity: {parentName} tried to start {childName}.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Debugger stuffy
                Debug.WriteLine("Monitoring Error: " + ex.Message);
            }
        }

        public void Stop()
        {
            _startWatch?.Stop();
            _startWatch?.Dispose();
        }
    }
}
