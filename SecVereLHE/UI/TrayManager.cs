using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecVerseLHE.UI
{
    public class TrayManager : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        public event EventHandler ExitRequested;

        public TrayManager()
        {
            var appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            _trayIcon = new NotifyIcon
            {
                Icon = appIcon,
                Visible = true,
                Text = "SecVers LHE"
            };

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Exit", null, (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty));
            _trayIcon.ContextMenuStrip = menu;
        }

        public void ShowAlert(string title, string msg)
        {
            _trayIcon.ShowBalloonTip(3000, title, msg, ToolTipIcon.Warning);
        }

        public void CleanUp()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
    }
}
