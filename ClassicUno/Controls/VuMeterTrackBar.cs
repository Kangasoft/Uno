/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClassicUno.Controls
{
    /* This is a custom TrackBar control that looks like a VU (Volume Unit) meter */
    public sealed class VuMeterTrackBar : Control
    {
        private int _minimum;
        private int _maximum = 100;
        private int _value = 50;
        private int _segments = 15; /* Number of LED/meter bars */
        private bool _isDragging;

        public event EventHandler ValueChanged;

        public int Minimum
        {
            get => _minimum;
            set { _minimum = value; Invalidate(); }
        }

        public int Maximum
        {
            get => _maximum;
            set { _maximum = value; Invalidate(); }
        }

        public int Value
        {
            get => _value;
            set
            {
                var newVal = Math.Max(_minimum, Math.Min(_maximum, value));
                if (_value == newVal)
                {
                    return;
                }
                _value = newVal;
                Invalidate();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int Segments
        {
            get => _segments;
            set { _segments = value; Invalidate(); }
        }

        public VuMeterTrackBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(200, 35);
            BackColor = Color.FromArgb(30, 30, 30);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            /* Draw background */
            using (Brush bgBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            float range = _maximum - _minimum;
            var percent = range > 0 ? (_value - _minimum) / range : 0f;
            var activeSegments = (int)(percent * _segments);

            const int padding = 4;
            var totalWidth = ClientRectangle.Width - (padding * 2);
            var totalHeight = ClientRectangle.Height - (padding * 2);
            var segWidth = (float)(totalWidth - (_segments - 1) * 2) / _segments;

            for (var i = 0; i < _segments; i++)
            {
                var segRect = new RectangleF(padding + i * (segWidth + 2), padding, segWidth, totalHeight);

                Color segColor;
                var ratio = (float)i / _segments;

                if (i < activeSegments)
                {
                    if (ratio < 0.65f)
                    {
                        segColor = Color.LimeGreen; /* Normal level */
                    }
                    else if (ratio < 0.85f)
                    {
                        segColor = Color.Yellow; /* Warning level */
                    }
                    else
                    {
                        segColor = Color.Red; /* Peak / clipping level */
                    }
                }
                else
                {
                    segColor = Color.FromArgb(60, 60, 60); /* Inactive segment */
                }

                using (Brush b = new SolidBrush(segColor))
                {
                    g.FillRectangle(b, segRect);
                }
            }
        }

        private void UpdateValueByMouse(MouseEventArgs e)
        {
            const int padding = 4;
            var usableWidth = ClientRectangle.Width - (padding * 2);
            float relativeX = Math.Max(0, Math.Min(usableWidth, e.X - padding));
            var percent = relativeX / usableWidth;

            var newVal = (int)(_minimum + percent * (_maximum - _minimum));
            Value = newVal;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            _isDragging = true;
            UpdateValueByMouse(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging)
            {
                UpdateValueByMouse(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isDragging = false;
        }
    }
}