/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClassicUno.Controls.UserButtons
{
    #region Enums
    public enum CustomButtonType
    {
        Control = 0,
        Color = 1
    }

    public enum CustomButtonIdentifier
    {
        Draw = 0,
        Pass = 1,
        Play = 2,
        Red = 3,
        Blue = 4,
        Green = 5,
        Yellow = 6
    }
    #endregion

    public sealed class CustomButton : UserControl
    {
        #region Fields
        private Image _buttonImage;
        private bool _hovering;
        private bool _clicked;

        public new event Action<object, EventArgs> Click;
        #endregion

        #region Properties
        public CustomButtonType ButtonType { get; set; }

        public CustomButtonIdentifier ButtonIdentifier { get; set; }

        public Image ButtonImage
        {
            get => _buttonImage;
            set
            {
                _buttonImage = value;
                Size = _buttonImage.Size;
            }
        }

        public Color HoverColor { get; set; } = Color.DeepSkyBlue;

        public Color ClickedColor { get; set; } = Color.White;
        #endregion

        #region Constructor
        public CustomButton()
        {
            BackColor = Color.Transparent;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
        }
        #endregion

        #region Overrides
        protected override void OnMouseEnter(EventArgs e)
        {
            _hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovering = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _hovering = false;
                _clicked = true;
            }
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _hovering = true;
                _clicked = false;
                Click?.Invoke(this, EventArgs.Empty);
            }
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!Visible)
            {
                return;
            }

            if (ButtonImage != null)
            {
                e.Graphics.DrawImage(ButtonImage, new Rectangle(0, 0, _buttonImage.Width, _buttonImage.Height));
            }

            if (!Enabled)
            {
                return;
            }

            if (_hovering || _clicked)
            {
                /* Draw a "focus/hover" rectangle */
                var color = _hovering ? Color.FromArgb(80, HoverColor) : Color.FromArgb(80, ClickedColor);
                const float radius = 7f;

                var rect = new RectangleF(
                    1,
                    1,
                    ClientSize.Width - 2,
                    ClientSize.Height - 2);

                using (var path = new GraphicsPath())
                using (var brush = new SolidBrush(color))
                {
                    const float diameter = radius * 2;

                    path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                    path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                    path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                    path.CloseFigure();

                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                }
            }
            base.OnPaint(e);
        }
        #endregion
    }
}
