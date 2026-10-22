/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ClassicUno.Classes.UI
{
    public class UnoAnimation : IDisposable
    {
        #region Fields
        private readonly Control _owner;
        private readonly Image _image;

        private readonly ImageAttributes _imageAttributes;

        private float _opacity = 1f;
        private RectangleF _bounds;
        private bool _disposed;
        #endregion

        #region Constructor
        public UnoAnimation(Control owner, RectangleF bounds, Image image)
        {
            _owner = owner;
            _bounds = bounds;

            /* Load the animated GIF once. */
            _image = image;

            /* Apply reduced opacity. 1.0 = fully opaque, 0.0 = fully transparent. */
            _imageAttributes = new ImageAttributes();

            SetOpacity();
        }
        #endregion

        #region Properties
        public float Opacity
        {
            get => _opacity;
            set
            {
                var opacity = value;
                if (opacity < 0f)
                {
                    opacity = 0f;
                }

                if (opacity > 1f)
                {
                    opacity = 1f;
                }

                _opacity = opacity;

                SetOpacity();
            }
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
        #endregion

        #region Animation control
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
        #endregion

        #region Draw method
        public void Draw(Graphics graphics)
        {
            if (_disposed || !IsAnimating || graphics == null)
            {
                return;
            }

            /* Update this GIF's current animation frame. */
            ImageAnimator.UpdateFrames(_image);

            /* Apply opacity attributes. */
            graphics.DrawImage(_image, Rectangle.Round(_bounds), 0, 0, _image.Width, _image.Height, GraphicsUnit.Pixel, _imageAttributes);
        }
        #endregion

        #region Private methods

        private void SetOpacity()
        {
            var colorMatrix = new ColorMatrix
            {
                Matrix33 = _opacity
            };

            _imageAttributes.SetColorMatrix(colorMatrix);
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
        #endregion

        #region IDisposable
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
            _imageAttributes.Dispose();
        }
        #endregion
    }
}