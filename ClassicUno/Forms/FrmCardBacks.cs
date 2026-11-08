/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public sealed class FrmCardBacks : FormEx
    {
        #region Fields
        private readonly CardStripControl _ctlCards;

        public List<Image> CardImages
        {
            set => _ctlCards.Images = value;
        }

        public int SelectedIndex
        {
            get => _ctlCards.Index;
            set => _ctlCards.Index = value;
        }
        #endregion

        #region Constructor
        public FrmCardBacks()
        {
            ClientSize = new Size(432, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Select card back image";

            _ctlCards = new CardStripControl
            {
                BackColor = Color.FromArgb(20, 80, 50),
                CardSize = new Size(72, 108),
                CardSpacing = 6,
                Dock = DockStyle.Top,
                Index = -1,
                Location = new Point(0, 0),
                Size = new Size(432, 220)
            };

            _ctlCards.DoubleClick += OnDoubleClick;

            var btnClose = new Button
            {
                DialogResult = DialogResult.OK,
                Location = new Point(345, 225),
                Size = new Size(75, 23),
                TabIndex = 0,
                Text = @"Ok",
                UseVisualStyleBackColor = true
            };

            Controls.AddRange(new Control[] {_ctlCards, btnClose});

            AcceptButton = btnClose;
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ctlCards.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region List double-click handler
        private void OnDoubleClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        #endregion
    }
}
