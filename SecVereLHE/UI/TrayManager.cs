using SecVerseLHE.Core;
using System;
using System.Drawing; // Wichtig für Fonts/Colors
using System.Windows.Forms;

namespace SecVerseLHE.UI
{
    public class TrayManager : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _menu; // Menü als Variable speichern

        public event EventHandler ExitRequested;
        public event EventHandler<bool> OfficeProtectionToggled;

        public TrayManager()
        {
            var appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            _trayIcon = new NotifyIcon
            {
                Icon = appIcon,
                Visible = true,
                Text = "SecVers LHE"
            };

            BuildMenu();
        }

        private void BuildMenu()
        {
            _menu = new ContextMenuStrip();
            bool usageDarkMode = ShouldUseDarkMode();
            _menu.Renderer = new ModernMenuRenderer(usageDarkMode);

            _menu.Font = new Font("Segoe UI", 10f, FontStyle.Regular);

            _menu.ShowImageMargin = false; 
            _menu.ShowImageMargin = true;
            _menu.BackColor = usageDarkMode ? Color.FromArgb(31, 31, 31) : Color.White;


            // disable office thingy
            var toggleItem = new ToolStripMenuItem("Disable Office Protection");
            toggleItem.Click += (s, e) => ToggleProtection(toggleItem);
            toggleItem.Padding = new Padding(0, 4, 0, 4);
            _menu.Items.Add(toggleItem);


            _menu.Items.Add(new ToolStripSeparator());
            #region InfoSection
            // about
            var aboutItem = new ToolStripMenuItem("About", null, (s, e) =>
                MessageBox.Show("SecVerse LHE\n\nA lightway System Hardening Tool.\nBlocks Software run from Appdata that dosnt include a code Sign Certificate.", "About", MessageBoxButtons.OK, MessageBoxIcon.Information));
            aboutItem.Padding = new Padding(0, 4, 0, 4);
            _menu.Items.Add(aboutItem);


            // githb
            var gitItem = new ToolStripMenuItem("Github Repository", null, (s, e) =>
                System.Diagnostics.Process.Start("https://github.com/bunbunconmeow/Win11Debloater"));
            gitItem.Padding = new Padding(0, 4, 0, 4);
            _menu.Items.Add(gitItem);


            // exit
            var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty));
            exitItem.Padding = new Padding(0, 4, 0, 4);
            exitItem.Font = new Font(_menu.Font, FontStyle.Bold);
            _menu.Items.Add(exitItem);
            #endregion InfoSection
            _trayIcon.ContextMenuStrip = _menu;
        }

        private void ToggleProtection(ToolStripMenuItem item)
        {
            if (item.Text.StartsWith("Disable"))
            {
                item.Text = "Enable Office Protection";
                ShowAlert("Protection Paused", "Monitoring disabled.");
                OfficeProtectionToggled?.Invoke(this, false);
            }
            else
            {
                item.Text = "Disable Office Protection";
                ShowAlert("Protection Active", "Monitoring enabled.");
                OfficeProtectionToggled?.Invoke(this, true);
            }
        }
        private bool ShouldUseDarkMode()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    object val = key?.GetValue("AppsUseLightTheme");
                    if (val is int i && i == 0) return true; // 0 = Dark, 1 = Light
                }
            }
            catch { }
            return true; 
        }

        public void ShowAlert(string title, string msg)
        {
            _trayIcon.ShowBalloonTip(3000, title, msg, ToolTipIcon.Warning);
        }

        public void CleanUp()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _menu.Dispose();
        }
    }
}
