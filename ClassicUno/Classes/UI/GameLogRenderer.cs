/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;

namespace ClassicUno.Classes.UI
{
    public sealed class GameLogRenderer : IDisposable
    {
        #region Fields
        private const float Padding = 8f;
        private const float LineGap = 4f;
        private const int FeatherHeight = 55;

        private readonly Font _font;

        private readonly Color _textColor;
        private readonly Color _shadowColor;

        private Bitmap _bitmap;
        private bool _disposed;
        #endregion

        #region Properties
        public int MaxLines { get; set; } = 6;

        public int Width { get; set; } = 400;

        public int Height { get; set; } = 155;

        public Rectangle Bounds { get; private set; }

        public int BottomOffset { get; set; } = 0;

        public bool ShowDropShadow { get; set; }

        public bool Visible { get; set; }
        #endregion

        #region Constructor
        public GameLogRenderer(Color textColor, float fontSize = 11f)
        {
            _textColor = textColor;
            _shadowColor = Color.FromArgb(180, 0, 0, 0);

            _font = new Font("Arial", fontSize, FontStyle.Regular);

            _bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        }
        #endregion

        #region Bound rectangle
        public void UpdateBounds(Rectangle clientRectangle)
        {
            Bounds = new Rectangle(clientRectangle.Left, clientRectangle.Bottom - Height - BottomOffset, Width, Height);
        }
        #endregion

        #region Update log text
        public void Update(IEnumerable<string> messages)
        {
            /* Rebuilds the cached log bitmap. Called when the log messages change. */
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GameLogRenderer));
            }

            using (var g = Graphics.FromImage(_bitmap))
            {
                using (Brush textBrush = new SolidBrush(_textColor))
                {
                    using (Brush shadowBrush = new SolidBrush(_shadowColor))
                    {
                        g.Clear(Color.Transparent);

                        if (ShowDropShadow)
                        {
                            using (Brush backgroundBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                            {
                                /* Subtle translucent backdrop, which also makes the top feather visible. */
                                g.FillRectangle(backgroundBrush, 0, 0, Width, Height);
                            }
                        }

                        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                        var textWidth = Width - Padding * 2;
                        var lineHeight = _font.GetHeight(g);

                        /* Wrap longer lines */
                        var wrappedMessages = new List<List<string>>();

                        if (messages != null)
                        {
                            wrappedMessages.AddRange(from message in messages
                                where !string.IsNullOrWhiteSpace(message)
                                select WrapText(g, message, textWidth));
                        }

                        /* Keep the newest six visual lines, preserving message groups. */
                        var visibleLines = new List<(string Text, int MessageIndex)>();

                        for (var i = wrappedMessages.Count - 1; i >= 0 && visibleLines.Count < MaxLines; i--)
                        {
                            var lines = wrappedMessages[i];

                            for (var j = lines.Count - 1; j >= 0 && visibleLines.Count < MaxLines; j--)
                            {
                                visibleLines.Add((lines[j], i));
                            }
                        }

                        /* Draw from the bottom upwards. */
                        var y = Height - Padding - lineHeight;

                        for (var i = 0; i < visibleLines.Count; i++)
                        {
                            var item = visibleLines[i];

                            var rect = new RectangleF(Padding, y, textWidth, lineHeight + 2f);

                            var shadow = rect;
                            shadow.Offset(1f, 1f);

                            /* Draw shadow */
                            g.DrawString(item.Text, _font, shadowBrush, shadow);

                            /* Draw text */
                            g.DrawString(item.Text, _font, textBrush, rect);

                            y -= lineHeight;

                            /* Add a gap only when moving to a different message. */
                            if (i + 1 < visibleLines.Count && visibleLines[i + 1].MessageIndex != item.MessageIndex)
                            {
                                y -= LineGap;
                            }
                        }
                    }
                }
            }

            /* Slightly feather the top of the bitmap so the top line fades in to the background */
            ApplyFeather(_bitmap, FeatherHeight);
        }
        #endregion

        #region Wrapping
        private List<string> WrapText(Graphics g, string text, float maxWidth)
        {
            /* Wrap longer lines of text to fit the physical drawing area */
            var result = new List<string>();

            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var line = string.Empty;

            foreach (var word in words)
            {
                var test = string.IsNullOrEmpty(line) ? word : line + " " + word;

                if (g.MeasureString(test, _font).Width <= maxWidth)
                {
                    line = test;
                }
                else
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        result.Add(line);
                    }

                    var remainingWord = word;

                    /* Split exceptionally long words. */
                    while (g.MeasureString(remainingWord, _font).Width > maxWidth && remainingWord.Length > 1)
                    {
                        var length = remainingWord.Length - 1;

                        while (length > 1 && g.MeasureString(remainingWord.Substring(0, length), _font).Width > maxWidth)
                        {
                            length--;
                        }

                        result.Add(remainingWord.Substring(0, length));
                        remainingWord = remainingWord.Substring(length);
                    }

                    line = remainingWord;
                }
            }

            if (!string.IsNullOrEmpty(line))
            {
                result.Add(line);
            }

            return result;
        }
        #endregion

        #region Feathering
        private static void ApplyFeather(Bitmap bitmap, int featherHeight)
        {
            /*  Applies a smooth alpha fade across the top edge */
            featherHeight = Math.Max(1, Math.Min(featherHeight, bitmap.Height));

            var bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var data = bitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            try
            {
                var stride = data.Stride;
                var byteCount = Math.Abs(stride) * bitmap.Height;
                var pixels = new byte[byteCount];

                Marshal.Copy(data.Scan0, pixels, 0, byteCount);

                for (var y = 0; y < featherHeight; y++)
                {
                    var t = (float)y / (featherHeight - 1);
                    t = t * t * (3f - 2f * t);

                    var row = y * stride;

                    for (var x = 0; x < bitmap.Width; x++)
                    {
                        var index = row + x * 4;

                        pixels[index + 3] = (byte)(pixels[index + 3] * t);
                    }
                }

                Marshal.Copy(pixels, 0, data.Scan0, byteCount);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
        #endregion

        #region Draw bitmap
        public void Draw(Graphics g)
        {
            if (_disposed || _bitmap == null || !Visible)
            {
                return;
            }

            g.DrawImageUnscaled(_bitmap, Bounds.Location);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            /* Dispose GDI objects */
            _bitmap?.Dispose();
            _bitmap = null;

            _font.Dispose();

            _disposed = true;
        }
        #endregion
    }
}