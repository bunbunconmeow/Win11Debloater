using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecVerseLHE.UI
{
    public class ContextMenuManager : IDisposable
    {
        private ContextMenuStrip _contextMenu;
        private NotifyIcon _notifyIcon;

        // Menu Items
        private ToolStripMenuItem _officeProtectionItem;
        private ToolStripMenuItem _interceptorGuardItem;
        private ToolStripMenuItem _igWhitelistItem;
        private ToolStripMenuItem _runtimeGuardItem;
        private ToolStripMenuItem _riskAssessmentItem;
        private ToolStripMenuItem _settingsItem;
        private ToolStripMenuItem _aboutItem;
        private ToolStripMenuItem _exitItem;

        // Events
        public event EventHandler<bool> OfficeProtectionToggled;
        public event EventHandler<bool> InterceptorGuardToggled;
        public event EventHandler<bool> IGWhitelistToggled;
        public event EventHandler<bool> RuntimeGuardToggled;
        public event EventHandler<bool> RiskAssessmentToggled;
        public event EventHandler SettingsRequested;
        public event EventHandler AboutRequested;
        public event EventHandler ExitRequested;

        public ContextMenuManager(NotifyIcon notifyIcon)
        {
            _notifyIcon = notifyIcon ?? throw new ArgumentNullException(nameof(notifyIcon));
            InitializeContextMenu();
        }

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Renderer = new ModernContextMenuRenderer();

            var headerItem = new ToolStripLabel("SecVerse LHE")
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                Padding = new Padding(5, 5, 5, 5)
            };
            _contextMenu.Items.Add(headerItem);
            _contextMenu.Items.Add(new ToolStripSeparator());

          
        }


        public void Dispose()
        {
            _contextMenu?.Dispose();
        }
    }

    internal class ModernContextMenuRenderer : ToolStripProfessionalRenderer
    {
        public ModernContextMenuRenderer() : base(new ModernColorTable()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(230, 240, 250)),
                    e.Item.ContentRectangle);
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }
    }

    internal class ModernColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Color.FromArgb(230, 240, 250);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(230, 240, 250);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(230, 240, 250);
        public override Color MenuItemBorder => Color.FromArgb(0, 120, 215);
    }
}
