/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClassicUno.Properties;

namespace ClassicUno.Controls.UserButtons
{
    /* I decided to write my own custom button and button strip control, as I don't like
     * the TableLayout control and a standard Button leaves a lot to be desired when
     * you try to customize it... */
    public sealed class ButtonStrip : UserControl
    {
        #region Fields
        private readonly List<Image> _imageList = new List<Image>();

        private const int ButtonPadding = 3;

        private readonly int _shortWidth; /* Width of control when color buttons are invisible */
        private readonly int _totalWidth; /* Total width when all 7 buttons are visible */

        public Action<CustomButtonType, CustomButtonIdentifier> ButtonClick;
        #endregion

        #region Constructor
        public ButtonStrip()
        {
            BackColor = Color.Transparent;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);

            /* A stand ImageList cannot be used here as you must set the ImageSize
             * property. If the width/height of each image differs, it's going to
             * scale the image appropriately. Hence, we use a List<Image> here. */
            _imageList.AddRange(new Image[]
            {
                Resources.btnDraw,
                Resources.btnPass,
                Resources.btnPlay,
                Resources.btnRed,
                Resources.btnBlue,
                Resources.btnGreen,
                Resources.btnYellow
            });

            /* Create the buttons */
            var x = 0;
            for (var i = 0; i <= 6; i++)
            {
                var image = _imageList[i];
                var type = i <= 2 ? CustomButtonType.Control : CustomButtonType.Color;
                var button = new CustomButton
                {
                    ButtonType = type,
                    ButtonIdentifier = (CustomButtonIdentifier) i,
                    ButtonImage = image,
                    Location = new Point(x,0),
                    Enabled = type != CustomButtonType.Control, /* Control buttons enabled all the time */
                    Visible = type == CustomButtonType.Control /* Color buttons are not visible until needed */
                };

                button.Click += ButtonClickHandler;

                Controls.Add(button);

                x += image.Size.Width + (i < 6 ? i == 2 ? ButtonPadding * 4 : ButtonPadding : 0);
                if (i == 2)
                {
                    _shortWidth = button.Right;
                }
            }

            _totalWidth = x;

            /* Initially set client size to the short width as color buttons are invisible by default */
            ClientSize = new Size(_shortWidth, 48);
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var image in _imageList)
                {
                    image.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Button control methods
        public void EnableButton(CustomButtonIdentifier identifier, bool enable)
        {
            foreach (var button in Controls.Cast<CustomButton>().Where(button => button.ButtonIdentifier == identifier))
            {
                button.Enabled = enable;
            }
        }

        public void ShowColorButtons(bool show)
        {
            foreach (var button in Controls.Cast<CustomButton>().Where(button => button.ButtonType == CustomButtonType.Color))
            {
                button.Visible = show;
            }

            /* Reset width of control */
            Width = show ? _totalWidth : _shortWidth;
        }
        #endregion

        #region Button click
        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (CustomButton) sender;
            if (btn == null)
            {
                return;
            }

            ButtonClick?.Invoke(btn.ButtonType, btn.ButtonIdentifier);
        }
        #endregion
    }
}
