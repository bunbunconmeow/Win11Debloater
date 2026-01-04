using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecVerseLHE.UI
{
    internal class BetterUI : ProfessionalColorTable
    {
        private readonly bool _isDarkMode;

        private readonly Color _darkBack = Color.FromArgb(31, 31, 31); 
        private readonly Color _darkBorder = Color.FromArgb(60, 60, 60);
        private readonly Color _darkHover = Color.FromArgb(55, 55, 55);
        private readonly Color _darkText = Color.FromArgb(240, 240, 240);
        private readonly Color _lightBack = Color.FromArgb(245, 245, 245);
        private readonly Color _lightBorder = Color.FromArgb(220, 220, 220);
        private readonly Color _lightHover = Color.FromArgb(230, 230, 230);
        private readonly Color _lightText = Color.Black;

        public BetterUI(bool darkMode)
        {
            _isDarkMode = darkMode;
        }

        public override Color ToolStripDropDownBackground => _isDarkMode ? _darkBack : _lightBack;
        public override Color ImageMarginGradientBegin => _isDarkMode ? _darkBack : _lightBack;
        public override Color ImageMarginGradientMiddle => _isDarkMode ? _darkBack : _lightBack;
        public override Color ImageMarginGradientEnd => _isDarkMode ? _darkBack : _lightBack;
        public override Color MenuBorder => _isDarkMode ? _darkBorder : _lightBorder;
        public override Color MenuItemSelected => _isDarkMode ? _darkHover : _lightHover;
        public override Color MenuItemBorder => Color.Transparent; 

        public override Color SeparatorDark => _isDarkMode ? _darkBorder : _lightBorder;
        public override Color SeparatorLight => Color.Transparent; 

        public Color TextColor => _isDarkMode ? _darkText : _lightText;
    }

    public class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly BetterUI _colors;

        public ModernMenuRenderer(bool darkMode) : base(new BetterUI(darkMode))
        {
            _colors = (BetterUI)ColorTable;
            this.RoundedEdges = false; 
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = _colors.TextColor;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = _colors.TextColor;
            base.OnRenderArrow(e);
        }
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            base.OnRenderItemCheck(e);
        }
    }
}
