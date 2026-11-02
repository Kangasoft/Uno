/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ClassicUno.Properties;

namespace ClassicUno.Forms
{
    /* This may seem overkill just to display an image center screen for a splash screen. However,
     * WinForms natively doesn't do transparent forms well. So, we have to use an update layered
     * window and Win32 interop. */
    public class FrmSplash : Form
    {
        #region Win32
        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Size
        {
            public int cx;
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BlendFunction
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BitmapInfoHeader
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public uint biCompression;
            private readonly uint biSizeImage;
            private readonly int biXPelsPerMeter;
            private readonly int biYPelsPerMeter;
            private readonly uint biClrUsed;
            private readonly uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BitmapInfo
        {
            public BitmapInfoHeader bmiHeader;
            private readonly uint bmiColors;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(
            IntPtr hwnd,
            IntPtr hdcDst,
            ref Win32Point pptDst,
            ref Win32Size psize,
            IntPtr hdcSrc,
            ref Win32Point pptSrc,
            int crKey,
            ref BlendFunction pblend,
            int dwFlags);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateDIBSection(
            IntPtr hdc,
            ref BitmapInfo pbmi,
            uint usage,
            out IntPtr ppvBits,
            IntPtr hSection,
            uint offset);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private const int WsExLayered = 0x00080000;
        private const int UlwAlpha = 0x00000002;
        private const byte AcSrcOver = 0x00;
        private const byte AcSrcAlpha = 0x01;
        #endregion

        #region Fields
        private Bitmap _splashImage;
        private bool _shown;
        private bool _updating;

        private Timer _fadeTimer;
        private Stopwatch _fadeWatch;

        private byte _opacity;

        private const int FadeDuration = 400;
        private const int DisplayDuration = 5000;
        private const int FadeOutStart = FadeDuration + DisplayDuration;
        private const int TotalDuration = FadeOutStart + FadeDuration;
        #endregion

        #region Constructor
        public FrmSplash()
        {
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;

            /* Load the embedded resource. */
            using (var original = Resources.splash)
            {
                _splashImage = new Bitmap(500, 333, PixelFormat.Format32bppPArgb);

                using (var g = Graphics.FromImage(_splashImage))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    g.DrawImage(original, 0, 0, 500, 333);
                }
            }

            ClientSize = new Size(500, 333);

            _opacity = 0;

            _fadeWatch = new Stopwatch();

            _fadeTimer = new Timer
            {
                Interval = 16
            };

            _fadeTimer.Tick += FadeTimer;
        }
        #endregion

        #region Overrides
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WsExLayered;
                return cp;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _shown = true;

            _fadeWatch.Restart();
            UpdateLayeredImage();
            _fadeTimer.Start();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            if (_shown && !_updating)
            {
                UpdateLayeredImage();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_fadeTimer != null)
                {
                    _fadeTimer.Stop();
                    _fadeTimer.Tick -= FadeTimer;
                    _fadeTimer.Dispose();
                    _fadeTimer = null;
                }

                if (_fadeWatch != null)
                {
                    _fadeWatch.Stop();
                    _fadeWatch = null;
                }

                if (_splashImage != null)
                {
                    _splashImage.Dispose();
                    _splashImage = null;
                }
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Update Layered Image
        private void UpdateLayeredImage()
        {
            if (_splashImage == null || !IsHandleCreated || _updating)
            {
                return;
            }

            _updating = true;

            var screenDc = IntPtr.Zero;
            var memoryDc = IntPtr.Zero;
            var dib = IntPtr.Zero;
            var oldBitmap = IntPtr.Zero;

            try
            {
                var width = _splashImage.Width;
                var height = _splashImage.Height;

                /* Create a top-down, 32-bit DIB section. */
                var bmi = new BitmapInfo
                {
                    bmiHeader =
                    {
                        biSize = (uint) Marshal.SizeOf(typeof(BitmapInfoHeader)),
                        biWidth = width,
                        biHeight = -height,
                        biPlanes = 1,
                        biBitCount = 32,
                        biCompression = 0
                    }
                };

                /* BI_RGB */
                screenDc = GetDC(IntPtr.Zero);
                memoryDc = CreateCompatibleDC(screenDc);

                dib = CreateDIBSection(screenDc, ref bmi, 0, out var bits, IntPtr.Zero, 0);

                if (dib == IntPtr.Zero || bits == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Could not create DIB section.");
                }

                /* Copy the premultiplied-alpha pixels into the DIB. */
                var rect = new Rectangle(0, 0, width, height);
                var data = _splashImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

                try
                {
                    var rowBytes = width * 4;
                    var row = new byte[rowBytes];

                    for (var y = 0; y < height; y++)
                    {
                        var src = IntPtr.Add(data.Scan0, y * data.Stride);
                        var dest = IntPtr.Add(bits, y * rowBytes);

                        Marshal.Copy(src, row, 0, rowBytes);
                        Marshal.Copy(row, 0, dest, rowBytes);
                    }
                }
                finally
                {
                    _splashImage.UnlockBits(data);
                }

                oldBitmap = SelectObject(memoryDc, dib);

                var destination = new Win32Point
                {
                    x = Left,
                    y = Top
                };

                var source = new Win32Point { x = 0, y = 0 };

                var size = new Win32Size
                {
                    cx = width,
                    cy = height
                };

                var blend = new BlendFunction
                {
                    BlendOp = AcSrcOver,
                    BlendFlags = 0,
                    SourceConstantAlpha = _opacity,
                    AlphaFormat = AcSrcAlpha
                };

                if (!UpdateLayeredWindow(
                    Handle,
                    screenDc,
                    ref destination,
                    ref size,
                    memoryDc,
                    ref source,
                    0,
                    ref blend,
                    UlwAlpha))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (oldBitmap != IntPtr.Zero)
                {
                    SelectObject(memoryDc, oldBitmap);
                }

                if (dib != IntPtr.Zero)
                {
                    DeleteObject(dib);
                }

                if (memoryDc != IntPtr.Zero)
                {
                    DeleteDC(memoryDc);
                }

                if (screenDc != IntPtr.Zero)
                {
                    ReleaseDC(IntPtr.Zero, screenDc);
                }

                _updating = false;
            }
        }
        #endregion

        #region Fading timer callback
        private void FadeTimer(object sender, EventArgs e)
        {
            var elapsed = _fadeWatch.ElapsedMilliseconds;

            if (elapsed < FadeDuration)
            {
                /* Fade in. */
                _opacity = (byte)(255.0 * elapsed / FadeDuration);
            }
            else if (elapsed < FadeOutStart)
            {
                /* Fully visible for five seconds. */
                _opacity = 255;
            }
            else if (elapsed < TotalDuration)
            {
                /* Fade out. */
                var fadeElapsed = elapsed - FadeOutStart;
                _opacity = (byte)(255.0 * (1.0 - (double)fadeElapsed / FadeDuration));
            }
            else
            {
                /* Finished. */
                _opacity = 0;

                _fadeTimer.Stop();
                _fadeWatch.Stop();

                Close();
                return;
            }

            UpdateLayeredImage();
        }
        #endregion
    }
}
