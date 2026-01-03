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

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var tray = new TrayManager();
                var monitor = new ProcessMonitor(tray);
                monitor.Start();
                tray.ExitRequested += (s, e) => {
                    monitor.Stop();
                    tray.CleanUp();
                    Application.Exit();
                };

                Application.Run(tray);
            }
        }
    }
}
