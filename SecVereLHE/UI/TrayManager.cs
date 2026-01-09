using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecVerseLHE.UI
{
    public class TrayManager : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _menu;
        private bool _isDarkMode;
        private ModernMenuRenderer _menuRenderer;



        private ToolStripMenuItem _igToggle;
        private ToolStripMenuItem _whitelistToggle;
        private ToolStripMenuItem _officeToggle;
        private ToolStripMenuItem _runtimeToggle;
        private ToolStripMenuItem _riskToggle;
        private ToolStripMenuItem _ransomwareToggle;


        public event EventHandler ExitRequested;
        public event EventHandler<bool> OfficeProtectionToggled;
        public event EventHandler<bool> RuntimeGuardToggled;
        public event EventHandler<bool> IG_Toggled;
        public event EventHandler<bool> IG_WhitelistToggled;
        public event EventHandler<bool> RansomwareDetectionToggled;
        public event EventHandler<bool> RiskAssessmentEngine;

        public TrayManager()
        {
            var appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            _trayIcon = new NotifyIcon
            {
                Icon = appIcon,
                Visible = true,
                Text = "SecVerse LHE"
            };

            _isDarkMode = ShouldUseDarkMode();
            BuildMenu();
        }

        private void BuildMenu()
        {
            _menu = new ContextMenuStrip();
            _menuRenderer = new ModernMenuRenderer(_isDarkMode);
            _menu.Renderer = _menuRenderer;
            _menu.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            _menu.ShowImageMargin = true;
            _menu.BackColor = _isDarkMode ? Color.FromArgb(31, 31, 31) : Color.White;
            _menu.Padding = new Padding(4, 8, 4, 8);
            var header = new ToolStripMenuItem("SecVerse LHE")
            {
                Enabled = false,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            };

            _menu.Items.Add(header);
            _menu.Items.Add(CreateSeparator());



            _igToggle = CreateToggleMenuItem(
                "Interpreter Guard",
                isEnabled: true,
                onToggle: (enabled) =>
                {
                    IG_Toggled?.Invoke(this, enabled);
                    ShowAlert(
                        enabled ? "Interpreter Guard Active" : "Interpreter Guard Paused",
                        enabled ? "Interpreter Guard enabled." : "Interpreter Guard disabled.");
                    _whitelistToggle.Enabled = enabled;
                    if (!enabled)
                    {
                        _whitelistToggle.Checked = false;
                        _whitelistToggle.Tag = false;
                    }
                });
            _menu.Items.Add(_igToggle);
            _menu.Items.Add(_igToggle);
            _whitelistToggle = CreateToggleMenuItem(
                "    Whitelist (Games)",
                isEnabled: false,
                onToggle: (enabled) =>
                {
                    IG_WhitelistToggled?.Invoke(this, enabled);
                    ShowAlert(
                        enabled ? "Whitelist Active" : "Strict Mode",
                        enabled ? "Game compatibility enabled (Steam/Minecraft allowed)." : "Whitelist disabled. ALL AppData scripts blocked.");
                },
                isSubItem: true);
            _menu.Items.Add(_whitelistToggle);

            _officeToggle = CreateToggleMenuItem(
                "Office Protection",
                isEnabled: true,
                onToggle: (enabled) =>
                {
                    OfficeProtectionToggled?.Invoke(this, enabled);
                    ShowAlert(
                        enabled ? "Office Protection Active" : "Office Protection Paused",
                        enabled ? "Office monitoring enabled." : "Office monitoring disabled.");
                });
            _menu.Items.Add(_officeToggle);

            _runtimeToggle = CreateToggleMenuItem(
                "Runtime Guard",
                isEnabled: true,
                onToggle: (enabled) =>
                {
                    RuntimeGuardToggled?.Invoke(this, enabled);
                    ShowAlert(
                        enabled ? "Runtime Guard Active" : "Runtime Guard Paused",
                        enabled ? "Runtime Guard enabled." : "Runtime Guard disabled.");
                });
            _menu.Items.Add(_runtimeToggle);

            _riskToggle = CreateToggleMenuItem(
                "Risk Assessment Engine",
                isEnabled: true,
                onToggle: (enabled) =>
                {
                    RiskAssessmentEngine?.Invoke(this, enabled);
                    ShowAlert(
                        enabled ? "Risk Assessment Active" : "Risk Assessment Paused",
                        enabled ? "Risk Assessment Engine enabled." : "Risk Assessment Engine disabled.");
                });
            _menu.Items.Add(_riskToggle);

           
            _ransomwareToggle = CreateToggleMenuItem(
                "Ransomware Detection",
                isEnabled: true,
                onToggle: (enabled) =>
                {
                    RansomwareDetectionToggled?.Invoke(this, enabled);
                    ShowAlert(
                        enabled ? "Ransomware Detection Active" : "Ransomware Detection Paused",
                        enabled ? "Ransomware monitoring enabled." : "Ransomware monitoring disabled.");
                });
            _menu.Items.Add(_ransomwareToggle);

            _menu.Items.Add(CreateSeparator());

            var aboutItem = new ToolStripMenuItem("About")
            {
                Padding = new Padding(0, 4, 0, 4)
            };
            aboutItem.Click += (s, e) =>
                MessageBox.Show(
                    "SecVerse LHE\n\n" +
                    "A lightweight System Hardening Tool.\n\n" +
                    "• Blocks unsigned software from AppData\n" +
                    "• Monitors Office macro execution\n" +
                    "• Detects ransomware behavior\n" +
                    "• Runtime process protection",
                    "About SecVerse LHE",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            _menu.Items.Add(aboutItem);


            var gitItem = new ToolStripMenuItem("GitHub Repository")
            {
                Padding = new Padding(0, 4, 0, 4)
            };
            gitItem.Click += (s, e) =>
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/bunbunconmeow/Win11Debloater",
                    UseShellExecute = true
                });
            _menu.Items.Add(gitItem);

            _menu.Items.Add(CreateSeparator());

            var exitItem = new ToolStripMenuItem("Exit")
            {
                Padding = new Padding(0, 4, 0, 4),
                Font = new Font(_menu.Font, FontStyle.Bold)
            };
            exitItem.ForeColor = Color.FromArgb(220, 80, 80);
            exitItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
            _menu.Items.Add(exitItem);
            _menuRenderer.AttachToMenu(_menu);
            _trayIcon.ContextMenuStrip = _menu;
        }

        private ToolStripMenuItem CreateToggleMenuItem(string text, bool isEnabled, Action<bool> onToggle, bool isSubItem = false)
        {
            var item = new ToolStripMenuItem(text)
            {
                CheckOnClick = true,
                Checked = isEnabled,
                Tag = isEnabled,
                Padding = new Padding(0, 6, 0, 6)
            };

            if (isSubItem)
            {
                item.ForeColor = _isDarkMode ? Color.FromArgb(140, 140, 140) : Color.Gray;
            }

            item.Click += (s, e) =>
            {
                var menuItem = (ToolStripMenuItem)s;
                menuItem.Tag = menuItem.Checked;
                onToggle?.Invoke(menuItem.Checked);
            };

            return item;
        }

        private ToolStripSeparator CreateSeparator()
        {
            return new ToolStripSeparator
            {
                BackColor = _isDarkMode ? Color.FromArgb(64, 64, 64) : Color.LightGray
            };
        }

        private bool ShouldUseDarkMode()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    object val = key?.GetValue("AppsUseLightTheme");
                    if (val is int i && i == 0) return true;
                }
            }
            catch { }
            return true;
        }

        public void ShowAlert(string title, string msg)
        {
            _trayIcon.ShowBalloonTip(1500, title, msg, ToolTipIcon.None);
        }


        public void SetToggleState(ProtectionFeature feature, bool enabled)
        {
            ToolStripMenuItem target = null;

            switch (feature)
            {
                case ProtectionFeature.InterpreterGuard:
                    target = _igToggle;
                    break;
                case ProtectionFeature.Whitelist:
                    target = _whitelistToggle;
                    break;
                case ProtectionFeature.OfficeProtection:
                    target = _officeToggle;
                    break;
                case ProtectionFeature.RuntimeGuard:
                    target = _runtimeToggle;
                    break;
                case ProtectionFeature.RiskAssessment:
                    target = _riskToggle;
                    break;
                case ProtectionFeature.RansomwareDetection:
                    target = _ransomwareToggle;
                    break;
                default:
                    target = null;
                    break;
            }

            if (target != null)
            {
                target.Checked = enabled;
                target.Tag = enabled;
            }
        }

        public void CleanUp()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _menu.Dispose();
        }
    }

    public enum ProtectionFeature
    {
        InterpreterGuard,
        Whitelist,
        OfficeProtection,
        RuntimeGuard,
        RiskAssessment,
        RansomwareDetection
    }
}