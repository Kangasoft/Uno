/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ClassicUno.Classes.Settings.SettingsData;
using ClassicUno.Classes.Shared;

namespace ClassicUno.Controls
{
    /* Custom-drawn statistics display for the UNO game.
     *  Add this control to the Game Statistics form and call SetStatistics()
     * whenever the statistics need updating. */
    public sealed class GameStatistics : UserControl
    {
        #region Fields
        private StatisticsData _stats;

        private readonly Color _textColor = Color.FromArgb(30, 30, 30);
        private readonly Color _mutedTextColor = Color.FromArgb(85, 85, 85);
        private readonly Color _accentColor = Color.FromArgb(0, 166, 81);
        private readonly Color _ringBackColor = Color.FromArgb(70, 70, 70);
        private readonly Color _tileBackColor = Color.White;
        private readonly Color _lineColor = Color.FromArgb(210, 210, 210);

        public StatisticsData Statistics
        {
            get => _stats;
            set
            {
                _stats = value;
                Invalidate();
            }
        }
        #endregion

        #region Constructor
        public GameStatistics()
        {
            SetStyle(
                ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            ForeColor = Color.Black;
            MinimumSize = new Size(380, 300);
            Size = new Size(430, 320);
            Font = new Font("Arial", 10F, FontStyle.Regular);
            Margin = Padding.Empty;
        }
        #endregion

        #region Paint methods

        #region OnPaint
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            const int padding = 14;
            const int topSectionHeight = 132;
            const int streakHeight = 54;
            const int sectionGap = 10;

            var ringDiameter = Math.Min(112, topSectionHeight - 12);
            var ringBounds = new Rectangle(padding, padding + (topSectionHeight - ringDiameter) / 2 - 8, ringDiameter, ringDiameter);

            DrawWinRateRing(g, ringBounds);

            var contentLeft = ringBounds.Right + 22;
            var contentWidth = Math.Max(100, Width - contentLeft - padding);
            var statsY = padding + 10;
            const int statRowHeight = 25;

            var played = _stats.GamesPlayed;
            using (var valueFont = new Font(Font.FontFamily, Font.Size, FontStyle.Bold))
            {
                DrawStatRow(g, "Games played:", played.ToString("N0"), contentLeft, statsY, contentWidth, valueFont);
                statsY += statRowHeight;

                DrawStatRow(g, "Score:", _stats.HighScore.ToString("N0"), contentLeft, statsY, contentWidth, valueFont);
                statsY += statRowHeight;

                DrawStatRow(g, "Wins:", _stats.GamesWon.ToString("N0"), contentLeft, statsY, contentWidth, valueFont);
                statsY += statRowHeight;

                DrawStatRow(g, "Losses:", _stats.GamesLost.ToString("N0"), contentLeft, statsY, contentWidth, valueFont);
            }

            const int streakY = padding + topSectionHeight;
            DrawStreakTiles(g, padding, streakY, Width - padding * 2, streakHeight);

            const int dividerY = streakY + streakHeight + sectionGap;
            using (var dividerPen = new Pen(_lineColor))
            {
                g.DrawLine(dividerPen, padding, dividerY, Width - padding, dividerY);
            }

            var durationY = dividerY + 10;
            var durationWidth = Width - padding * 2;
            const int durationRowHeight = 25;

            DrawDurationRow(g, "Shortest game:", _stats.ShortestGame, padding, durationY, durationWidth);
            durationY += durationRowHeight;

            DrawDurationRow(g, "Longest game:", _stats.LongestGame, padding, durationY, durationWidth);
            durationY += durationRowHeight;

            var averageGameTime = played > 0
                ? _stats.TotalGameTime / (double)played
                : 0;
            DrawDurationRow(g, "Average time per game:", (int)Math.Max(0, averageGameTime), padding, durationY, durationWidth);
        }
        #endregion

        #region Percentage ring
        private void DrawWinRateRing(Graphics g, Rectangle bounds)
        {
            const float thickness = 9F;
            var arcBounds = new RectangleF(bounds.X + thickness / 2F, bounds.Y + thickness / 2F,
                bounds.Width - thickness, bounds.Height - thickness);
            var played = _stats.GamesPlayed;

            using (var backgroundPen = new Pen(_ringBackColor, thickness))
            {
                using (var winPen = new Pen(_accentColor, thickness))
                {
                    using (var percentFont = new Font("Arial", 17F, FontStyle.Bold))
                    {
                        using (Brush textBrush = new SolidBrush(_textColor))
                        {
                            using (var sf = new StringFormat())
                            {
                                backgroundPen.StartCap = LineCap.Flat;
                                backgroundPen.EndCap = LineCap.Flat;
                                winPen.StartCap = LineCap.Flat;
                                winPen.EndCap = LineCap.Flat;

                                g.DrawArc(backgroundPen, arcBounds, 0F, 359.9F);

                                var rate = played > 0 ? (double) _stats.GamesWon / played : 0.0;

                                var sweep = (float) (Math.Max(0.0, Math.Min(1.0, rate)) * 360.0);
                                if (sweep > 0F)
                                {
                                    g.DrawArc(winPen, arcBounds, -90F, sweep);
                                }

                                sf.Alignment = StringAlignment.Center;
                                sf.LineAlignment = StringAlignment.Center;

                                var percent = played > 0 ? $"{Math.Round(rate * 100.0)}%" : "0%";

                                g.DrawString(percent, percentFont, textBrush, bounds, sf);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Win/Loss rows
        private void DrawStatRow(Graphics g, string label, string value, int x, int y, int width, Font valueFont)
        {
            using (Brush labelBrush = new SolidBrush(_textColor))
            {
                using (Brush valueBrush = new SolidBrush(_textColor))
                {
                    using (var left = new StringFormat())
                    {
                        using (var right = new StringFormat())
                        {
                            left.Alignment = StringAlignment.Near;
                            left.LineAlignment = StringAlignment.Center;
                            left.FormatFlags = StringFormatFlags.NoWrap;
                            right.Alignment = StringAlignment.Far;
                            right.LineAlignment = StringAlignment.Center;
                            right.FormatFlags = StringFormatFlags.NoWrap;

                            var row = new RectangleF(x, y, width, 22F);
                            g.DrawString(label, Font, labelBrush, row, left);
                            g.DrawString(value, valueFont, valueBrush, row, right);
                        }
                    }
                }
            }
        }
        #endregion

        #region Streak tiles
        private void DrawStreakTiles(Graphics g, int x, int y, int width, int height)
        {
            const int gap = 10;
            var tileWidth = (width - gap) / 2;

            var currentTile = new Rectangle(x, y, tileWidth, height);
            var bestTile = new Rectangle(x + tileWidth + gap, y, tileWidth, height);

            using (Brush tileBrush = new SolidBrush(_tileBackColor))
            {
                using (var borderPen = new Pen(_lineColor))
                {
                    using (Brush labelBrush = new SolidBrush(_mutedTextColor))
                    {
                        using (Brush valueBrush = new SolidBrush(_accentColor))
                        {
                            using (var labelFont = new Font(Font.FontFamily, 8F, FontStyle.Bold))
                            {
                                using (var valueFont = new Font(Font.FontFamily, 12F, FontStyle.Bold))
                                {
                                    using (var centered = new StringFormat())
                                    {
                                        centered.Alignment = StringAlignment.Center;
                                        centered.LineAlignment = StringAlignment.Center;
                                        centered.FormatFlags = StringFormatFlags.NoWrap;

                                        g.FillRectangle(tileBrush, currentTile);
                                        g.DrawRectangle(borderPen, currentTile);
                                        g.FillRectangle(tileBrush, bestTile);
                                        g.DrawRectangle(borderPen, bestTile);

                                        var currentLabel = new RectangleF(currentTile.X + 3, currentTile.Y + 3, currentTile.Width - 6, 16);
                                        var currentValue = new RectangleF(currentTile.X + 3, currentTile.Y + 19, currentTile.Width - 6, 29);
                                        var bestLabel = new RectangleF(bestTile.X + 3, bestTile.Y + 3, bestTile.Width - 6, 16);
                                        var bestValue = new RectangleF(bestTile.X + 3, bestTile.Y + 19, bestTile.Width - 6, 29);

                                        g.DrawString("CURRENT STREAK", labelFont, labelBrush, currentLabel, centered);
                                        g.DrawString(FormatWins(_stats.CurrentStreak), valueFont, valueBrush, currentValue, centered);
                                        g.DrawString("BEST STREAK", labelFont, labelBrush, bestLabel, centered);
                                        g.DrawString(FormatWins(_stats.LongestStreak), valueFont, valueBrush, bestValue, centered);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Game durations
        private void DrawDurationRow(Graphics g, string label, int value, int x, int y, int width)
        {
            using (Brush labelBrush = new SolidBrush(_textColor))
            {
                using (Brush valueBrush = new SolidBrush(_textColor))
                {
                    using (var left = new StringFormat())
                    {
                        using (var right = new StringFormat())
                        {
                            left.Alignment = StringAlignment.Near;
                            left.LineAlignment = StringAlignment.Center;
                            left.FormatFlags = StringFormatFlags.NoWrap;
                            right.Alignment = StringAlignment.Far;
                            right.LineAlignment = StringAlignment.Center;
                            right.FormatFlags = StringFormatFlags.NoWrap;

                            var row = new RectangleF(x, y, width, 23F);
                            g.DrawString(label, Font, labelBrush, row, left);
                            g.DrawString(Misc.FormatTime(value), Font, valueBrush, row, right);
                        }
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Private helpers
        private static string FormatWins(int wins)
        {
            return $"{wins:N0} {(wins == 1 ? "win" : "wins")}";
        }
        #endregion
    }
}
