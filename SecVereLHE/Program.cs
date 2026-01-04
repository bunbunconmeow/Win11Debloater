using SecVerseLHE.Core;
using SecVerseLHE.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecVerseLHE
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            using (Mutex mutex = new Mutex(true, "SecVersLHE", out bool createdNew))
            {
                if (!createdNew) return;
#if !DEBUG
                BsodProtection.SetCritical(true);
#endif
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var tray = new TrayManager();
                var processMonitor = new ProcessGenealogy();
                var monitor = new ProcessMonitor(tray);
                var interceptor = new IntercepterGuard();

                monitor.Start();
                processMonitor.StartMonitoring(tray);

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
    }
}
