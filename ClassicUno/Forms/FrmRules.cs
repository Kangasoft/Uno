/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ClassicUno.Classes.Shared;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public sealed class FrmRules : FormEx
    {
        #region Paddded RichTextBox class
        private class PaddedRichTextBox : RichTextBox
        {
            private const int EmSetMargins = 0x00D3;
            private const int EcLeftMargin = 0x0001;
            private const int EcRightMargin = 0x0002;

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            public int LeftPadding { get; set; } = 8;

            public int RightPadding { get; set; } = 8;

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                UpdateMargins();
            }

            private void UpdateMargins()
            {
                var margins = (LeftPadding & 0xFFFF) | ((RightPadding & 0xFFFF) << 16);

                SendMessage(Handle, EmSetMargins, (IntPtr)(EcLeftMargin | EcRightMargin), (IntPtr)margins);
            }
        }
        #endregion

        #region Constructor
        public FrmRules()
        {
            ClientSize = new Size(583, 405);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Rules";

            var rtfText = new PaddedRichTextBox
            {
                BackColor = SystemColors.Window,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Top,
                LeftPadding = 12,
                RightPadding = 12,
                Location = new Point(0, 0),
                Name = "rtfText",
                ReadOnly = true,
                DetectUrls = true,
                ScrollBars = RichTextBoxScrollBars.ForcedVertical,
                Size = new Size(583, 364),
                TabIndex = 1
            };

            var btnClose = new Button
            {
                DialogResult = DialogResult.OK,
                Location = new Point(496, 370),
                Size = new Size(75, 23),
                TabIndex = 0,
                Text = @"Close",
                UseVisualStyleBackColor = true
            };

            rtfText.LinkClicked += OnLinkClicked;

            AcceptButton = btnClose;

            Controls.AddRange(new Control[] {rtfText, btnClose});

            var file = Misc.MainDir(@"\data\rules.rtf", false);

            if (!File.Exists(file))
            {
                return;
            }
            try
            {
                rtfText.LoadFile(file);
            }
            catch
            {
                /* If it's in use by another process, LoadFile will fail */
                Debug.Assert(true);
            }
        }
        #endregion

        #region Link helper
        private void OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.LinkText,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this,
                    $"Unable to open the link.\r\n\r\n{ex.Message}",
                    @"Rules",
                    CustomMessageBoxButtons.Ok,
                    CustomMessageBoxIcon.Exclamation);
            }
        }
        #endregion
    }
}
