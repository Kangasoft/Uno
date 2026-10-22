/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ClassicUno.Classes.UI.Controls
{
    public sealed class CardStripControl : UserControl
    {
        #region Fields
        private readonly HScrollBar _scrollBar;
        private List<Image> _images = new List<Image>();

        /* Reference card dimensions; scaled to fit the available height. */
        private Size _cardSize = new Size(72, 108);
        private int _cardSpacing = 6;
        private int _index = -1;
        private int _scrollOffset;

        private const int PaddingLeft = 12;
        private const int PaddingRight = 12;
        private const int PaddingTop = 8;
        private const int PaddingBottom = 8;

        private int _maxScrollOffset;

        public event EventHandler SelectedIndexChanged;
        #endregion

        #region Properties
        private int ViewportHeight => Math.Max(0, ClientSize.Height - _scrollBar.Height);

        /* Images to display. The control does not own or dispose them. */
        public List<Image> Images
        {
            get => _images;
            set
            {
                _images = value ?? new List<Image>();

                if (_index >= _images.Count)
                {
                    Index = -1;
                }

                UpdateScrollBar();
                Invalidate();
            }
        }

        /* Selected image index, or -1 when nothing is selected. */
        public int Index
        {
            get => _index;
            set
            {
                var newIndex = value >= 0 && value < _images.Count ? value : -1;

                if (_index == newIndex)
                {

                    return;
                }

                _index = newIndex;

                if (_index >= 0)
                {
                    EnsureIndexVisible(_index);
                }

                Invalidate();
                OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        /* Gap between adjacent cards. */
        public int CardSpacing
        {
            get => _cardSpacing;
            set
            {
                _cardSpacing = Math.Max(0, value);
                UpdateScrollBar();
                Invalidate();
            }
        }

        /* Reference dimensions used to calculate the card aspect ratio. */
        public Size CardSize
        {
            get => _cardSize;
            set
            {
                if (value.Width <= 0 || value.Height <= 0)
                {
                    return;
                }

                _cardSize = value;
                UpdateScrollBar();
                Invalidate();
            }
        }

        /* Recalculate after modifying the Images list directly. */
        public void RefreshItems()
        {
            if (_index >= _images.Count)
            {
                Index = -1;
            }

            UpdateScrollBar();
            Invalidate();
        }
        #endregion

        #region Constructor
        public CardStripControl()
        {
            SetStyle(
                ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            BackColor = Color.FromArgb(20, 80, 50);

            _scrollBar = new HScrollBar
            {
                Dock = DockStyle.Bottom,
                Height = SystemInformation.HorizontalScrollBarHeight,
                Minimum = 0,
                SmallChange = 1,
                LargeChange = 1
            };

            _scrollBar.Scroll += ScrollBarScroll;

            Controls.Add(_scrollBar);
        }
        #endregion

        #region Overrides
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left || e.Y >= ViewportHeight)
            {
                return;
            }

            for (var i = 0; i < _images.Count; i++)
            {
                var bounds = GetCardBounds(i);

                if (!bounds.Contains(e.Location))
                {
                    continue;
                }

                Index = i;
                return;
            }

            /* Clicking an empty area clears the selection. */
            Index = -1;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_images == null || _images.Count == 0)
            {
                return;
            }

            var graphics = e.Graphics;
            var cardSize = GetScaledCardSize();

            if (cardSize.IsEmpty)
            {
                return;
            }

            var viewport = new Rectangle(0, 0, ClientSize.Width, ViewportHeight);

            graphics.SetClip(viewport);

            for (var i = 0; i < _images.Count; i++)
            {
                var image = _images[i];

                if (image == null)
                {
                    continue;
                }

                var bounds = GetCardBounds(i);

                if (!viewport.IntersectsWith(bounds))
                {
                    continue;
                }

                graphics.DrawImage(image, bounds);

                if (i != _index)
                {
                    continue;
                }

                using (var brush = new SolidBrush(Color.FromArgb(65, Color.DeepSkyBlue)))
                {
                    using (var pen = new Pen(Color.DeepSkyBlue, 2f))
                    {
                        graphics.FillRectangle(brush, bounds);

                        graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                    }
                }
            }

            graphics.ResetClip();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollBar();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scrollBar.Scroll -= ScrollBarScroll;
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Callbacks
        private void OnSelectedIndexChanged(EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
        }

        private void ScrollBarScroll(object sender, ScrollEventArgs e)
        {
            _scrollOffset = e.NewValue;
            Invalidate();
        }
        #endregion

        #region Private helper methods
        /* Scale the reference dimensions to the available control height. */
        private Size GetScaledCardSize()
        {
            var availableHeight = ClientSize.Height - _scrollBar.Height - PaddingTop - PaddingBottom;

            if (availableHeight <= 0)
            {
                return Size.Empty;
            }

            var scale = availableHeight / (float)_cardSize.Height;

            return new Size(
                Math.Max(1, (int)Math.Round(_cardSize.Width * scale)),
                Math.Max(1, (int)Math.Round(_cardSize.Height * scale)));
        }

        private Rectangle GetCardBounds(int index)
        {
            var cardSize = GetScaledCardSize();

            if (cardSize.IsEmpty)
            {
                return Rectangle.Empty;
            }

            var x = PaddingLeft + index * (cardSize.Width + _cardSpacing) - _scrollOffset;
            var y = PaddingTop + Math.Max(0, (ViewportHeight - PaddingTop - PaddingBottom - cardSize.Height) / 2);

            return new Rectangle(x, y, cardSize.Width, cardSize.Height);
        }

        private void UpdateScrollBar()
        {
            if (_scrollBar == null || IsDisposed)
            {
                return;
            }

            var cardSize = GetScaledCardSize();
            var viewportWidth = Math.Max(1, ClientSize.Width);

            var contentWidth = _images.Count == 0 || cardSize.IsEmpty
                ? 0
                : PaddingLeft + PaddingRight
                              + _images.Count * cardSize.Width
                              + Math.Max(0, _images.Count - 1) * _cardSpacing;

            _maxScrollOffset = Math.Max(0, contentWidth - viewportWidth);

            /* Temporarily reduce LargeChange so changing Maximum cannot clamp the current Value prematurely. */
            _scrollBar.Minimum = 0;
            _scrollBar.LargeChange = 1;
            _scrollBar.Maximum = _maxScrollOffset + viewportWidth - 1;

            /* The page size controls the thumb size. */
            _scrollBar.LargeChange = viewportWidth;
            _scrollBar.SmallChange = Math.Max(1, cardSize.Width + _cardSpacing);
            _scrollBar.Enabled = _maxScrollOffset > 0;

            var effectiveMaximum = Math.Max(0, _scrollBar.Maximum - _scrollBar.LargeChange + 1);

            _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, effectiveMaximum));

            _scrollBar.Value = _scrollOffset;
            _scrollOffset = _scrollBar.Value;

            Invalidate();
        }

        private void EnsureIndexVisible(int index)
        {
            if (index < 0 || index >= _images.Count)
            {
                return;
            }

            var cardSize = GetScaledCardSize();

            if (cardSize.IsEmpty)
            {
                return;
            }

            var stride = cardSize.Width + _cardSpacing;
            var left = PaddingLeft + index * stride;
            var right = left + cardSize.Width;

            var viewportLeft = _scrollOffset + PaddingLeft;
            var viewportRight = _scrollOffset + ClientSize.Width - PaddingRight;

            if (left < viewportLeft)
            {
                _scrollOffset = Math.Max(0, left - PaddingLeft);
            }
            else if (right > viewportRight)
            {
                _scrollOffset = right - ClientSize.Width + PaddingRight;
            }

            var cardCount = _images.Count;
            var contentWidth = cardCount == 0
                ? 0
                : PaddingLeft + PaddingRight
                              + cardCount * cardSize.Width
                              + Math.Max(0, cardCount - 1) * _cardSpacing;

            var maxOffset = Math.Max(0, contentWidth - ClientSize.Width);
            _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, maxOffset));

            var effectiveMaximum = Math.Max(0, _scrollBar.Maximum - _scrollBar.LargeChange + 1);

            _scrollBar.Value = Math.Min(_scrollOffset, effectiveMaximum);
            _scrollOffset = _scrollBar.Value;

            Invalidate();
        }
        #endregion
    }
}