using SecVerseLHE.Core;
using SecVerseLHE.Helper;
using SecVerseLHE.UI;
using System;
using System.Threading;
using System.Windows.Forms;

namespace SecVerseLHE
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            InitHelper.Initialize();
            using (Mutex mutex = new Mutex(true, "SecVersLHE", out bool createdNew))
            {
                if (!createdNew) return;


#if !DEBUG
                IntegrityManager.EnsureIntegrityAndStartup();
                BsodProtection.SetCritical(true);
#endif
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var tray = new TrayManager();
                var processMonitor = new ProcessGenealogy();
                var monitor = new ProcessMonitor(tray);
                var interceptor = new IntercepterGuard();
                var threadManager = new ThreadManager();
                var ransomwareWorker = new RansomwareDetectionWorker(tray);

                threadManager.StartWorker(ransomwareWorker);

                monitor.Start();
                processMonitor.StartMonitoring(tray);
                RegisterShutdownHandler();

                tray.OfficeProtectionToggled += (sender, isEnabled) =>
                {
                    if (isEnabled) processMonitor.StartMonitoring(tray);
                    else processMonitor.Stop();
                };

                tray.IG_Toggled += (sender, isEnabled) =>
                {
                    if(isEnabled) interceptor.StartMonitoring(tray);
                    else interceptor.Stop();
                };

                tray.IG_WhitelistToggled += (sender, isEnabled) =>
                {
                    interceptor.SetWhitelistEnabled(isEnabled);
                };

                tray.RuntimeGuardToggled += (sender, isEnabled) =>
                {
                    if (isEnabled) monitor.Start();
                    else monitor.Stop();
                };

                tray.RiskAssessmentEngine += (sender, isEnabled) =>
                {
                    monitor.riskAssessmentEnabled = isEnabled;
                };

                tray.ExitRequested += (s, e) => {

                    if(MessageBox.Show(
                        "Are you sure you want to exit SecVerse LHE? Exiting will disable all protection features.",
                        "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) return;

                    monitor.Stop();
                    processMonitor.Stop();

#if !DEBUG
                    BsodProtection.SetCritical(false);
#endif
                    tray.CleanUp();
                    Application.Exit();
                };

                Application.Run(tray);
            }
        }


        private static void RegisterShutdownHandler()
        {
            Microsoft.Win32.SystemEvents.SessionEnding += (sender, e) =>
            {
#if !DEBUG
                try
                {
                    string v = "Unknown reason";
                    switch(e.Reason)
                    {
                          case Microsoft.Win32.SessionEndReasons.Logoff:
                            BsodProtection.SetCritical(false);
                            v = "User logoff";
                            break;
                        case Microsoft.Win32.SessionEndReasons.SystemShutdown:
                            BsodProtection.SetCritical(false);
                            v = "System shutdown";
                            break;
                        default:
                            v = "Unknown reason";
                            break;
                    }
              
                    string reason = v;

                    System.Diagnostics.Debug.WriteLine($"LHE: BSOD protection disabled for system event: {reason}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"LHE: Failed to disable BSOD protection on shutdown: {ex.Message}");
                }
#endif
            };
        }
    }
}
