using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        public readonly Color AccentOn = Color.FromArgb(76, 194, 108);
        public readonly Color AccentOff = Color.FromArgb(120, 120, 120);
        public readonly Color AccentOnHover = Color.FromArgb(96, 214, 128);
        public readonly Color AccentOffHover = Color.FromArgb(140, 140, 140);
        public readonly Color ToggleKnob = Color.White;
        public readonly Color ToggleKnobShadow = Color.FromArgb(40, 0, 0, 0);

        public bool IsDarkMode => _isDarkMode;

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
        public Color SubTextColor => _isDarkMode ? Color.FromArgb(160, 160, 160) : Color.FromArgb(100, 100, 100);
        public Color BackgroundColor => _isDarkMode ? _darkBack : _lightBack;
        public Color HoverColor => _isDarkMode ? _darkHover : _lightHover;
    }

    public class ModernMenuRenderer : ToolStripProfessionalRenderer, IDisposable
    {
        private readonly BetterUI _colors;
        private readonly Timer _scrollTimer;
        private readonly Dictionary<ToolStripItem, ScrollState> _scrollStates;

        private ToolStripItem _currentHoveredItem;
        private ContextMenuStrip _parentMenu;

        private const int ToggleWidth = 36;
        private const int ToggleHeight = 18;
        private const int ToggleMarginRight = 12;
        private const int KnobPadding = 2;

        private const int ScrollSpeed = 2;
        private const int ScrollDelayMs = 30;
        private const int ScrollPauseAtEndMs = 1000;
        private const int ScrollStartDelayMs = 500;

        private class ScrollState
        {
            public int Offset { get; set; }
            public int MaxOffset { get; set; }
            public int TextWidth { get; set; }
            public int AvailableWidth { get; set; }
            public bool NeedsScrolling { get; set; }
            public ScrollPhase Phase { get; set; }
            public int PauseCounter { get; set; }
            public int StartDelayCounter { get; set; }
        }

        private enum ScrollPhase
        {
            WaitingToStart,
            ScrollingLeft,
            PausedAtEnd,
            ScrollingRight,
            PausedAtStart
        }

        public ModernMenuRenderer(bool darkMode) : base(new BetterUI(darkMode))
        {
            _colors = (BetterUI)ColorTable;
            _scrollStates = new Dictionary<ToolStripItem, ScrollState>();

            _scrollTimer = new Timer
            {
                Interval = ScrollDelayMs
            };
            _scrollTimer.Tick += OnScrollTick;
        }

        public void AttachToMenu(ContextMenuStrip menu)
        {
            _parentMenu = menu;
            menu.Opening += OnMenuOpening;
            menu.Closing += OnMenuClosing;

            foreach (ToolStripItem item in menu.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.MouseEnter += OnItemMouseEnter;
                    menuItem.MouseLeave += OnItemMouseLeave;
                }
            }
        }

        private void OnMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _scrollStates.Clear();
            _currentHoveredItem = null;
        }

        private void OnMenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _scrollTimer.Stop();
            _scrollStates.Clear();
            _currentHoveredItem = null;
        }

        private void OnItemMouseEnter(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                _currentHoveredItem = item;

                if (!_scrollStates.ContainsKey(item))
                {
                    var state = CalculateScrollState(item);
                    _scrollStates[item] = state;
                }
                else
                {
                    var state = _scrollStates[item];
                    state.Offset = 0;
                    state.Phase = ScrollPhase.WaitingToStart;
                    state.StartDelayCounter = 0;
                    state.PauseCounter = 0;
                }

                if (_scrollStates[item].NeedsScrolling)
                {
                    _scrollTimer.Start();
                }
            }
        }

        private void OnItemMouseLeave(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                if (_currentHoveredItem == item)
                {
                    _currentHoveredItem = null;
                    _scrollTimer.Stop();

                    if (_scrollStates.ContainsKey(item))
                    {
                        _scrollStates[item].Offset = 0;
                        _scrollStates[item].Phase = ScrollPhase.WaitingToStart;
                    }

                    item.Invalidate();
                    _parentMenu?.Invalidate();
                }
            }
        }

        private ScrollState CalculateScrollState(ToolStripMenuItem item)
        {
            var state = new ScrollState();

            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                var textSize = g.MeasureString(item.Text, item.Font ?? _parentMenu?.Font ?? SystemFonts.MenuFont);
                state.TextWidth = (int)Math.Ceiling(textSize.Width);
            }

            bool hasToggle = item.CheckOnClick || item.Tag is bool;
            int toggleSpace = hasToggle ? ToggleWidth + ToggleMarginRight + 15 : 0;
            state.AvailableWidth = item.ContentRectangle.Width - toggleSpace - 10;

            state.NeedsScrolling = state.TextWidth > state.AvailableWidth;
            state.MaxOffset = Math.Max(0, state.TextWidth - state.AvailableWidth + 5);
            state.Offset = 0;
            state.Phase = ScrollPhase.WaitingToStart;

            return state;
        }

        private void OnScrollTick(object sender, EventArgs e)
        {
            if (_currentHoveredItem == null || !_scrollStates.ContainsKey(_currentHoveredItem))
            {
                _scrollTimer.Stop();
                return;
            }

            var state = _scrollStates[_currentHoveredItem];

            if (!state.NeedsScrolling)
            {
                _scrollTimer.Stop();
                return;
            }

            int pauseTicks = ScrollPauseAtEndMs / ScrollDelayMs;
            int startDelayTicks = ScrollStartDelayMs / ScrollDelayMs;

            switch (state.Phase)
            {
                case ScrollPhase.WaitingToStart:
                    state.StartDelayCounter++;
                    if (state.StartDelayCounter >= startDelayTicks)
                    {
                        state.Phase = ScrollPhase.ScrollingLeft;
                        state.StartDelayCounter = 0;
                    }
                    break;

                case ScrollPhase.ScrollingLeft:
                    state.Offset += ScrollSpeed;
                    if (state.Offset >= state.MaxOffset)
                    {
                        state.Offset = state.MaxOffset;
                        state.Phase = ScrollPhase.PausedAtEnd;
                        state.PauseCounter = 0;
                    }
                    break;

                case ScrollPhase.PausedAtEnd:
                    state.PauseCounter++;
                    if (state.PauseCounter >= pauseTicks)
                    {
                        state.Phase = ScrollPhase.ScrollingRight;
                    }
                    break;

                case ScrollPhase.ScrollingRight:
                    state.Offset -= ScrollSpeed;
                    if (state.Offset <= 0)
                    {
                        state.Offset = 0;
                        state.Phase = ScrollPhase.PausedAtStart;
                        state.PauseCounter = 0;
                    }
                    break;

                case ScrollPhase.PausedAtStart:
                    state.PauseCounter++;
                    if (state.PauseCounter >= pauseTicks)
                    {
                        state.Phase = ScrollPhase.ScrollingLeft;
                    }
                    break;
            }

            _currentHoveredItem.Invalidate();
            _parentMenu?.Invalidate();
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (e.Item is ToolStripMenuItem menuItem)
            {
                bool hasToggle = menuItem.CheckOnClick || menuItem.Tag is bool;
                bool isHovered = e.Item.Selected;


                int toggleSpace = hasToggle ? ToggleWidth + ToggleMarginRight + 15 : 0;
                int availableWidth = e.Item.ContentRectangle.Width - toggleSpace - 10;

                ScrollState scrollState = null;
                if (_scrollStates.ContainsKey(menuItem))
                {
                    scrollState = _scrollStates[menuItem];
                }


                Color textColor;
                if (menuItem.Text.StartsWith("    "))
                {
                    textColor = _colors.SubTextColor;
                }
                else
                {
                    textColor = menuItem.ForeColor != Color.Empty && menuItem.ForeColor != SystemColors.ControlText
                        ? menuItem.ForeColor
                        : _colors.TextColor;
                }

               
                var clipRect = new Rectangle(
                    e.TextRectangle.X,
                    e.TextRectangle.Y,
                    availableWidth,
                    e.TextRectangle.Height);

                var originalClip = e.Graphics.ClipBounds;
                e.Graphics.SetClip(clipRect);


                int textX = e.TextRectangle.X;
                if (isHovered && scrollState != null && scrollState.NeedsScrolling)
                {
                    textX -= scrollState.Offset;
                }

                var textRect = new Rectangle(
                    textX,
                    e.TextRectangle.Y,
                    scrollState?.TextWidth ?? e.TextRectangle.Width + 100,
                    e.TextRectangle.Height);

                TextRenderer.DrawText(
                    e.Graphics,
                    e.Text,
                    e.TextFont,
                    textRect,
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

                // Clip zurücksetzen
                e.Graphics.SetClip(originalClip);
            }
            else
            {
                e.TextColor = _colors.TextColor;
                base.OnRenderItemText(e);
            }
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = _colors.TextColor;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderMenuItemBackground(e);

            if (e.Item is ToolStripMenuItem menuItem && (menuItem.CheckOnClick || menuItem.Tag is bool))
            {
                bool isChecked = menuItem.Checked;
                if (menuItem.Tag is bool tagValue)
                    isChecked = tagValue;

                DrawToggleSwitch(e.Graphics, e.Item.ContentRectangle, isChecked, e.Item.Selected);
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            if (e.Item is ToolStripMenuItem menuItem && (menuItem.CheckOnClick || menuItem.Tag is bool))
            {
                return;
            }
            base.OnRenderItemCheck(e);
        }

        private void DrawToggleSwitch(Graphics g, Rectangle bounds, bool isOn, bool isHovered)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int toggleX = bounds.Right - ToggleWidth - ToggleMarginRight;
            int toggleY = bounds.Y + (bounds.Height - ToggleHeight) / 2;

            var toggleRect = new Rectangle(toggleX, toggleY, ToggleWidth, ToggleHeight);

            Color backColor = isOn
                ? (isHovered ? _colors.AccentOnHover : _colors.AccentOn)
                : (isHovered ? _colors.AccentOffHover : _colors.AccentOff);

            using (var path = CreateRoundedRectangle(toggleRect, ToggleHeight / 2))
            using (var brush = new SolidBrush(backColor))
            {
                g.FillPath(brush, path);
            }

            int knobSize = ToggleHeight - (KnobPadding * 2);
            int knobX = isOn
                ? toggleRect.Right - knobSize - KnobPadding
                : toggleRect.X + KnobPadding;
            int knobY = toggleRect.Y + KnobPadding;

            var knobRect = new Rectangle(knobX, knobY, knobSize, knobSize);

            var shadowRect = new Rectangle(knobX + 1, knobY + 1, knobSize, knobSize);
            using (var shadowBrush = new SolidBrush(_colors.ToggleKnobShadow))
            {
                g.FillEllipse(shadowBrush, shadowRect);
            }

            using (var knobBrush = new SolidBrush(_colors.ToggleKnob))
            {
                g.FillEllipse(knobBrush, knobRect);
            }

            g.SmoothingMode = SmoothingMode.Default;
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        public void Dispose()
        {
            _scrollTimer?.Stop();
            _scrollTimer?.Dispose();
            _scrollStates?.Clear();

            if (_parentMenu != null)
            {
                _parentMenu.Opening -= OnMenuOpening;
                _parentMenu.Closing -= OnMenuClosing;

                foreach (ToolStripItem item in _parentMenu.Items)
                {
                    if (item is ToolStripMenuItem menuItem)
                    {
                        menuItem.MouseEnter -= OnItemMouseEnter;
                        menuItem.MouseLeave -= OnItemMouseLeave;
                    }
                }
            }
        }
    }
}
