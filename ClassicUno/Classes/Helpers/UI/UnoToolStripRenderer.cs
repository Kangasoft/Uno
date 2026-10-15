/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClassicUno.Classes.Helpers.UI
{
    /* Menu and statusbar renderer */
    public class UnoToolStripRenderer : ToolStripProfessionalRenderer
    {
        /* When this baby hits runtime; you're going to see some serious shit! */

        private static readonly Color Background = Color.FromArgb(38, 40, 46);

        private static readonly Color DropdownBackground = Color.FromArgb(48, 50, 57);

        private static readonly Color UnoRed = Color.FromArgb(220, 45, 55);

        private static readonly Color HoverBackground = Color.FromArgb(65, 68, 77);

        private static readonly Color PressedBackground = Color.FromArgb(150, 35, 43);

        private static readonly Color Foreground = Color.FromArgb(245, 245, 245);

        private static readonly Color MutedForeground = Color.FromArgb(175, 180, 190);

        public UnoToolStripRenderer() : base(new UnoColorTable())
        {
            RoundedEdges = false;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            var bounds = e.AffectedBounds;

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var isStatusStrip = e.ToolStrip is StatusStrip;
            var isDropDown = e.ToolStrip is ToolStripDropDown;

            if (isDropDown)
            {
                using (var brush = new SolidBrush(DropdownBackground))
                {
                    e.Graphics.FillRectangle(brush, bounds);
                }

                return;
            }

            var topColor = isStatusStrip ? Color.FromArgb(28, 29, 34) : Color.FromArgb(65, 68, 76);
            var bottomColor = isStatusStrip ? Color.FromArgb(48, 50, 57) : Color.FromArgb(32, 34, 39);

            using (var brush = new LinearGradientBrush(bounds, topColor, bottomColor, LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, bounds);
            }

            using (var pen = new Pen(isStatusStrip ? Color.FromArgb(90, 92, 100) : Color.FromArgb(100, 102, 110)))
            {
                var y = bounds.Top;
                e.Graphics.DrawLine(pen, bounds.Left, y, bounds.Right - 1, y);
            }

            using (var pen = new Pen(isStatusStrip ? Color.FromArgb(15, 15, 18) : Color.FromArgb(20, 21, 25)))
            {
                e.Graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var bounds = new Rectangle(Point.Empty, e.Item.Size);

            if (!e.Item.Pressed && !e.Item.Selected)
            {
                return;
            }

            var color = e.Item.Pressed ? PressedBackground : HoverBackground;

            using (var brush = new SolidBrush(color))
            {
                e.Graphics.FillRectangle(brush, bounds);
            }

            using (var brush = new SolidBrush(UnoRed))
            {
                e.Graphics.FillRectangle(brush, bounds.X, bounds.Y, 3, bounds.Height);
            }

        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            var isStatusStrip = e.ToolStrip is StatusStrip;

            using (var pen = new Pen(
                isStatusStrip
                    ? UnoRed
                    : Color.FromArgb(75, 78, 87)))
            {
                if (isStatusStrip)
                {
                    e.Graphics.DrawLine(pen, 0, 0, e.ToolStrip.Width - 1, 0);
                }
                else if (!(e.ToolStrip is ToolStripDropDown))
                {
                    e.Graphics.DrawLine(pen, 0, e.ToolStrip.Height - 1,
                        e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
                }
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            if (e.Vertical)
            {
                var x = e.Item.Width / 2;

                using (var pen = new Pen(Color.FromArgb(85, 88, 98)))
                {
                    e.Graphics.DrawLine(pen, x, 3, x, e.Item.Height - 4);
                }
            }
            else
            {
                var y = e.Item.Height / 2;

                using (var pen = new Pen(Color.FromArgb(85, 88, 98)))
                {
                    e.Graphics.DrawLine(pen, 6, y, e.Item.Width - 6, y);
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (e.Item.Enabled)
            {
                e.TextColor = e.Item.Selected ? Color.White : Foreground;
            }
            else
            {
                e.TextColor = MutedForeground;
            }

            base.OnRenderItemText(e);
        }

        private sealed class UnoColorTable : ProfessionalColorTable
        {
            public override Color MenuStripGradientBegin => Background;

            public override Color MenuStripGradientEnd => Background;

            public override Color ToolStripDropDownBackground => DropdownBackground;

            public override Color ImageMarginGradientBegin => DropdownBackground;

            public override Color ImageMarginGradientMiddle => DropdownBackground;

            public override Color ImageMarginGradientEnd => DropdownBackground;

            public override Color MenuItemSelected => HoverBackground;

            public override Color MenuItemBorder => UnoRed;

            public override Color MenuItemSelectedGradientBegin => HoverBackground;

            public override Color MenuItemSelectedGradientEnd => HoverBackground;

            public override Color MenuItemPressedGradientBegin => PressedBackground;

            public override Color MenuItemPressedGradientEnd => PressedBackground;

            public override Color ToolStripGradientBegin => Background;

            public override Color ToolStripGradientMiddle => Background;

            public override Color ToolStripGradientEnd => Background;

            public override Color StatusStripGradientBegin => Background;

            public override Color StatusStripGradientEnd => Background;

            public override Color ToolStripBorder => Color.FromArgb(75, 78, 87);

            public override Color SeparatorDark => Color.FromArgb(85, 88, 98);

            public override Color SeparatorLight => Background;
        }
    }
}