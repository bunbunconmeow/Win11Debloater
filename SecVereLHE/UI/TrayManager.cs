using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecVerseLHE.UI
{
    public class TrayManager : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _menu;
        private ToolStripMenuItem _whitelistItem;

        public event EventHandler ExitRequested;
        public event EventHandler<bool> OfficeProtectionToggled;
        public event EventHandler<bool> RuntimeGuardToggled;

        public event EventHandler<bool> IG_Toggled;
        public event EventHandler<bool> IG_WhitelistToggled;

        public event EventHandler<bool> RiskAssessmentEngine;

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

            var separator = new ToolStripSeparator
            {
                BackColor = usageDarkMode ? Color.FromArgb(64, 64, 64) : Color.LightGray,
                Height = 1
            };
            var separatorTitle = new ToolStripSeparator
            {
                BackColor = usageDarkMode ? Color.FromArgb(64, 64, 64) : Color.LightGray,
                Height = 1
            };
            
            _menu.Items.Add("SecVerse LHE");
            _menu.Items.Add(separatorTitle);

            #region ProtectionSection

            var tgl_InterpreterGuard = new ToolStripMenuItem("Disable Interpreter Guard");
            tgl_InterpreterGuard.Click += (s, e) => ToggleIG(tgl_InterpreterGuard);
            tgl_InterpreterGuard.Padding = new Padding(0, 4, 0, 4);
            _menu.Items.Add(tgl_InterpreterGuard);

            _whitelistItem = new ToolStripMenuItem("    Enable Whitelist (Games)");
            _whitelistItem.Click += (s, e) => ToggleWhitelist(_whitelistItem);
            _whitelistItem.Padding = new Padding(0, 4, 0, 4);
            _whitelistItem.ForeColor = Color.Gray; 
            _menu.Items.Add(_whitelistItem);

            // disable office thingy
            var tgl_officeProtection = new ToolStripMenuItem("Disable Office Protection");
            tgl_officeProtection.Click += (s, e) => ToggleProtection(tgl_officeProtection);
            tgl_officeProtection.Padding = new Padding(0, 4, 0, 4);
            _menu.Items.Add(tgl_officeProtection);

            // disable runtime Guard
            var tgl_RuntimeGuard = new ToolStripMenuItem("Disable Runtime Guard");
            tgl_RuntimeGuard.Click += (s, e) => ToggleProtectionRuntimeGuard(tgl_RuntimeGuard);
            tgl_RuntimeGuard.Padding = new Padding(0, 4, 0, 4);
            _menu.Items.Add(tgl_RuntimeGuard);

            var tgl_RiskAssessmentEngine = new ToolStripMenuItem("Disable Risk Assessment Engine");
            tgl_RiskAssessmentEngine.Click += (s, e) => ToggleRiskAssessmentEngine(tgl_RiskAssessmentEngine);
            tgl_RiskAssessmentEngine.Padding = new Padding(0, 4, 0, 4);
            _menu.Items.Add(tgl_RiskAssessmentEngine);

            #endregion ProtectionSection
            _menu.Items.Add(separator);
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


        private void ToggleRiskAssessmentEngine(ToolStripMenuItem item)
        {
            if (item.Text.StartsWith("Disable"))
            {
                item.Text = "Enable Risk Assessment Engine";
                ShowAlert("Risk Assessment Engine Paused", "Risk Assessment Engine disabled.");
                RiskAssessmentEngine?.Invoke(this, false);
            }
            else
            {
                item.Text = "Disable Risk Assessment Engine";
                ShowAlert("Risk Assessment Engine Active", "Risk Assessment Engine enabled.");
                RiskAssessmentEngine?.Invoke(this, true);
            }
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


        private void ToggleWhitelist(ToolStripMenuItem item)
        {
            if (item.Text.Contains("Enable"))
            {
                item.Text = "    Disable Whitelist (Strict Mode)";
                item.ForeColor = Color.Orange;
                ShowAlert("Whitelist Active", "Game compatibility enabled (Steam/Minecraft allowed).");
                IG_WhitelistToggled?.Invoke(this, true);
            }
            else
            {
                item.Text = "    Enable Whitelist (Games)";
                item.ForeColor = Color.Gray;
                ShowAlert("Strict Mode", "Whitelist disabled. ALL AppData scripts will be blocked.");
                IG_WhitelistToggled?.Invoke(this, false);
            }
        }

        private void ToggleIG(ToolStripMenuItem item)
        {
            if (item.Text.StartsWith("Disable"))
            {
                item.Text = "Enable Interpreter Guard";
                ShowAlert("Interpreter Guard Paused", "Interpreter Guard disabled.");
                IG_Toggled?.Invoke(this, false);
            }
            else
            {
                item.Text = "Disable Interpreter Guard";
                ShowAlert("Interpreter Guard Active", "Interpreter Guard enabled.");
                IG_Toggled?.Invoke(this, true);
            }
        }

        private void ToggleProtectionRuntimeGuard(ToolStripMenuItem item)
        {
            if (item.Text.StartsWith("Disable"))
            {
                item.Text = "Enable Runtime Guard";
                ShowAlert("Runtime Guard Paused", "Runtime Guard disabled.");
                RuntimeGuardToggled?.Invoke(this, false);
            }
            else
            {
                item.Text = "Disable Runtime Guard";
                ShowAlert("Runtime Guard Active", "Runtime Guard enabled.");
                RuntimeGuardToggled?.Invoke(this, true);
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
