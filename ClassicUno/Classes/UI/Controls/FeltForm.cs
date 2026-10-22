/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassicUno.Classes.UI.Controls
{
    public class FeltForm : Form
    {
        private Bitmap _feltTexture;

        public FeltForm()
        {
            /* Prevent flickering during resizing */
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
            UpdateStyles();

            CreateFeltTexture();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;

            /* Paint the entire current client area.
             * This automatically handles any new size after resizing. */
            using (var brush = new TextureBrush(_feltTexture))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
            /* Subtle edge shading */
            using (var pen = new Pen(Color.FromArgb(40, 0, 30, 15), 3))
            {
                var r = ClientRectangle;
                r.Width -= 1;
                r.Height -= 1;
                g.DrawRectangle(pen, r);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            /* Force the background to repaint immediately */
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_feltTexture != null)
                {
                    _feltTexture.Dispose();
                    _feltTexture = null;
                }
            }

            base.Dispose(disposing);
        }

        private void CreateFeltTexture()
        {
            const int size = 64;

            _feltTexture = new Bitmap(size, size);

            var rnd = new Random(12345);

            using (var g = Graphics.FromImage(_feltTexture))
            {
                /* Base felt color */
                g.Clear(Color.FromArgb(20, 85, 48));

                /* Fine felt fibres */
                for (var i = 0; i < 180; i++)
                {
                    var x = rnd.Next(size);
                    var y = rnd.Next(size);

                    var color = rnd.Next(2) == 0 ? Color.FromArgb(18, 80, 125, 75) : Color.FromArgb(18, 0, 35, 15);

                    using (var pen = new Pen(color))
                    {
                        g.DrawLine(pen, x, y, x + rnd.Next(-2, 3), y + rnd.Next(-1, 2));
                    }
                }
            }
        }
    }
}