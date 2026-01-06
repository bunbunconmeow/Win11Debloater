using SecVerseLHE.UI;
using System;
using System.Diagnostics;
using System.Management;
namespace SecVerseLHE.Core
{
    internal class ProcessMonitor
    {
        private ManagementEventWatcher _watcher;
        private readonly Heuristics _heuristics;
        private readonly TrayManager _ui;
        private readonly int _myPid; 

        public bool riskAssessmentEnabled { get; set; } = true;

        public ProcessMonitor(TrayManager ui)
        {
            _ui = ui;
            _heuristics = new Heuristics();
            _myPid = Process.GetCurrentProcess().Id;
        }

        public void Start()
        {
            try
            {
                var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");

                _watcher = new ManagementEventWatcher(query);
                _watcher.EventArrived += OnProcessStart; 
                _watcher.Start();
            }
            catch (Exception ex)
            {
                _ui.ShowAlert("Error", "Monitor Start failed: " + ex.Message);
            }
        }

        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.Stop();
                _watcher.Dispose();
            }
        }

        private void OnProcessStart(object sender, EventArrivedEventArgs e)
        {
            int pid = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);

            if (pid == _myPid) return;
            string path = PathResolver.GetPathFromPid(pid);
            if (string.IsNullOrEmpty(path)) return;
            if (_heuristics.IsTrusted(path)) return;
            if(riskAssessmentEnabled) SuspiciousFileScanner.StartTry(path, _ui);
            if (_heuristics.IsThreat(path)) NeutralizeThreat(pid, path);
        }

        private void NeutralizeThreat(int pid, string path)
        {
            Process p = null;
            try
            {
               
                p = Process.GetProcessById(pid);
                Enforcer.Neutralize(p, path, _ui);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Kill failed: {ex.Message}");
            }
            finally
            {
                if (p != null) p.Dispose();
            }
        }
    }
}
