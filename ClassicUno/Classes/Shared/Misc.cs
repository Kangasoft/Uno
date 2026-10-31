/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ClassicUno.Classes.Shared
{
    public static class Misc
    {
        private static readonly string BaseFolder = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /* Mins/secs time formatting */
        public static string FormatTime(int seconds)
        {
            var ts = new TimeSpan(0, 0, 0, seconds);
            return ts.Hours > 0 ? $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s" :
                ts.Minutes > 0 ? $"{ts.Minutes}m {ts.Seconds}s" : $"{ts.Seconds}s";
        }

        /* Main application folder */
        public static string MainDir(string path)
        {
            /* Default is not to return application data roaming profile path */
            return MainDir(path, false);
        }

        public static string MainDir(string path, bool forceApplicationDataPath)
        {
            var folder = forceApplicationDataPath ? UserFolder : BaseFolder;
            if (string.IsNullOrEmpty(path))
            {
                /* Failed */
                return folder;
            }
            if (!forceApplicationDataPath && path.ToLower().Contains(folder.ToLower()))
            {
                /* Remove app path */
                return @"\" + path.Replace(folder, null);
            }
            var currentPath = path.Substring(0, 1) == @"\" ? (folder + path).Replace(@"\\", @"\") : path.Replace(@"\\", @"\");
            var pathOnly = Path.GetDirectoryName(currentPath);
            if (forceApplicationDataPath && !string.IsNullOrEmpty(pathOnly) && pathOnly.ToLower() != folder.ToLower() && !Directory.Exists(pathOnly))
            {
                /* If the directory doesn't exists, create it */
                Directory.CreateDirectory(pathOnly);
            }
            return currentPath;
        }

        /* Quick util for getting the current monitor the application is running on */
        public static Screen GetCurrentMonitor(Form wnd)
        {
            foreach (var s in Screen.AllScreens.Where(s => s.Bounds.Contains(wnd.Bounds)))
            {
                return s;
            }
            return Screen.PrimaryScreen;
        }

        /* Return [Description] meta data from enum */
        public static string GetDescriptionFromEnumValue(Enum value)
        {
            var attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        /* List extension */
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = RandomEx.Shared.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /* Graphics extension */
        public static void DrawText(this Graphics g, string text, Color color, RectangleF bounds, StringAlignment alignment, int fontSize = 14)
        {
            using (var font = new Font("Arial", fontSize, FontStyle.Bold))
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
