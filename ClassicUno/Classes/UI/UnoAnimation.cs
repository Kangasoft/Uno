/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Drawing;
using System.Windows.Forms;
using ClassicUno.Properties;

namespace ClassicUno.Classes.UI
{
    public class UnoAnimation : IDisposable
    {
        private readonly Control _owner;
        private readonly Image _image;

        private RectangleF _bounds;
        private bool _disposed;

        public UnoAnimation(Control owner, RectangleF bounds)
        {
            _owner = owner;
            _bounds = bounds;

            /* Load the animated GIF once. */
            _image = (Image)Resources.animatedUno.Clone();
        }

        public RectangleF Bounds
        {
            get => _bounds;
            set
            {
                if (_disposed || _bounds == value)
                {
                    return;
                }

                var oldBounds = _bounds;
                _bounds = value;

                /* Repaint both the old and new locations. */
                Invalidate(Rectangle.Round(oldBounds));
                Invalidate(Rectangle.Round(_bounds));
            }
        }

        public bool IsAnimating { get; private set; }

        public void Start()
        {
            if (_disposed || IsAnimating)
            {
                return;
            }

            IsAnimating = true;

            ImageAnimator.Animate(_image, OnFrameChanged);

            Invalidate(Rectangle.Round(_bounds));
        }

        public void Stop()
        {
            if (_disposed || !IsAnimating)
            {
                return;
            }

            IsAnimating = false;

            ImageAnimator.StopAnimate(_image, OnFrameChanged);

            /* Clear the animation area. */
            Invalidate(Rectangle.Round(_bounds));
        }

        public void Draw(Graphics graphics)
        {
            if (_disposed || !IsAnimating || graphics == null)
            {
                return;
            }

            /* Update this GIF's current animation frame. */
            ImageAnimator.UpdateFrames(_image);

            /* Draw into the specified rectangle. The GIF's transparent pixels remain transparent. */
            graphics.DrawImage(_image, _bounds);
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            if (_disposed || !IsAnimating)
            {
                return;
            }

            Invalidate(Rectangle.Round(_bounds));
        }

        private void Invalidate(Rectangle bounds)
        {
            if (_disposed || bounds.IsEmpty)
            {
                return;
            }

            if (_owner.IsDisposed || !_owner.IsHandleCreated)
            {
                return;
            }

            try
            {
                if (_owner.InvokeRequired)
                {
                    _owner.BeginInvoke((MethodInvoker)delegate
                    {
                        if (!_disposed && !_owner.IsDisposed && _owner.IsHandleCreated)
                        {
                            _owner.Invalidate(bounds);
                        }
                    });
                }
                else
                {
                    _owner.Invalidate(bounds);
                }
            }
            catch (InvalidOperationException)
            {
                /* The control may be closing or destroying its handle. */
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            /* Stop callbacks before disposing the image. */
            if (IsAnimating)
            {
                IsAnimating = false;
                ImageAnimator.StopAnimate(_image, OnFrameChanged);
            }

            _disposed = true;
            _image.Dispose();
        }
    }
}