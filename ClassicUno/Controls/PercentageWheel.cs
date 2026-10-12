/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClassicUno.Controls
{
    public sealed class PercentageWheel : UserControl
    {
        /* This control draws a percentage arc */
        private Color _progressArcColor;
        private Color _progressColor;
        private double _value;

        public Color ProgressArcColor
        {
            get => _progressArcColor;
            set
            {
                _progressArcColor = value;
                Invalidate();
            }
        }

        public Color ProgressColor
        {
            get => _progressColor;
            set
            {
                _progressColor = value;
                Invalidate();
            }
        }

        public double Value
        {
            get => _value;
            set
            {
                _value = value;
                Invalidate();
            }
        }

        public PercentageWheel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            ProgressArcColor = Color.FromArgb(0, 166, 81);
            ProgressColor = Color.Black;

            MinimumSize = new Size(122, 122);
            MaximumSize = MinimumSize;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            /* Draw background circle */
            using (var bgPen = new Pen(Color.FromArgb(70, 70, 70), 10))
            {
                e.Graphics.DrawEllipse(bgPen, 5, 5, 110, 110);
            }

            /* Draw Win Rate progress arc */
            using (var progressPen = new Pen(_progressArcColor, 10))
            {
                var sweepAngle = (float)(_value / 100.0 * 360.0);
                e.Graphics.DrawArc(progressPen, 5, 5, 110, 110, -90, sweepAngle);
            }

            /* Text inside ring */
            var pctText = $"{_value:F0}%";
            using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
            {
                using (Brush textBrush = new SolidBrush(_progressColor))
                {
                    var textSize = e.Graphics.MeasureString(pctText, font);
                    e.Graphics.DrawString(pctText, font, textBrush, 60 - (textSize.Width / 2),
                        45 - (textSize.Height / 2));
                }
            }
            base.OnPaint(e);
        }
    }
}
