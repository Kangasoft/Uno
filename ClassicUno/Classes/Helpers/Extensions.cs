/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClassicUno.Classes.Helpers
{
    public static class ListExtensions
    {
        private static readonly Random Rng = new Random();

        /* List extension */
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /* Graphics extension */
        public static void DrawText(this Graphics g, string text, Color color, RectangleF bounds, StringAlignment alignment)
        {
            using (var font = new Font("Arial", 14, FontStyle.Bold))
            {
                using (Brush shadowBrush = new SolidBrush(Color.Black))
                {
                    using (Brush textBrush = new SolidBrush(color))
                    {
                        using (var sf = new StringFormat())
                        {
                            sf.Alignment = alignment;
                            sf.LineAlignment = StringAlignment.Center;
                            sf.FormatFlags = StringFormatFlags.NoWrap;
                            sf.Trimming = StringTrimming.None;

                            var shadowBounds = bounds;
                            shadowBounds.Offset(2f, 2f);

                            /* Draw shadow. */
                            g.DrawString(text, font, shadowBrush, shadowBounds, sf);
                            /* Draw text */
                            g.DrawString(text, font, textBrush, bounds, sf);
                        }
                    }
                }
            }
        }
    }
}
