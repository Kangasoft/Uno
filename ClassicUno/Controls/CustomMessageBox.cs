/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace ClassicUno.Controls
{
    #region Enums
    public enum CustomMessageBoxIcon
    {
        None = 0,
        Exclamation = 1,
        Question = 2
    }

    public enum CustomMessageBoxButtons
    {
        Ok = 0,
        OkCancel = 1,
        YesNo = 2
    }
    #endregion

    public sealed class CustomMessageBox : FormEx
    {
        #region Fields
        private const int PaddingSize = 20;
        private const int IconSize = 32;
        private const int TextGap = 16;

        private const int ButtonWidth = 75;
        private const int ButtonHeight = 23;
        private const int ButtonGap = 6;
        private const int ButtonPanelHeight = 43;

        private readonly string _message;
        private readonly CustomMessageBoxIcon _icon;
        private readonly CustomMessageBoxButtons _buttons;

        private readonly Font _messageFont;
        private readonly Panel _buttonPanel;

        private readonly Rectangle _iconBounds;
        private readonly Rectangle _textBounds;
        #endregion

        #region Constructor
        private CustomMessageBox(string text, string caption, CustomMessageBoxButtons buttons, CustomMessageBoxIcon icon)
        {
            /*
             * Q: Why did the computer show up late to work?
             * A: It had a hard drive! 🚗
             */
            _message = text ?? string.Empty;
            _icon = icon;
            _buttons = buttons;

            _messageFont = new Font("Segoe UI", 9f, FontStyle.Regular);

            Text = caption ?? string.Empty;
            Font = _messageFont;

            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;
            AutoScaleMode = AutoScaleMode.None;
            DoubleBuffered = true;

            const int minClientWidth = 200;
            const int maxClientWidth = 440;

            var textLeft = _icon == CustomMessageBoxIcon.None ? PaddingSize : PaddingSize + IconSize + TextGap;

            /* Measure the longest explicit line without wrapping. */
            var longestLineWidth = 0;
            var lines = _message.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            using (var g = CreateGraphics())
            {
                longestLineWidth = lines
                    .Select(line => TextRenderer.MeasureText(g, string.IsNullOrEmpty(line) ? " " : line, _messageFont,
                        Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width)
                    .Concat(new[] {longestLineWidth}).Max();
            }

            /* Size to the message, up to the maximum width. */
            var desiredWidth = textLeft + longestLineWidth + PaddingSize * 2;
            var clientWidth = Math.Max(minClientWidth, Math.Min(maxClientWidth, desiredWidth));

            /* Available width for the text after accounting for the icon and padding. */
            var textWidth = clientWidth - textLeft - PaddingSize;
            
            /* Measure using the final width, allowing wrapping only when necessary. */
            var measuredText = TextRenderer.MeasureText(
                _message.Length == 0 ? " " : _message,
                _messageFont,
                new Size(textWidth, int.MaxValue),
                TextFormatFlags.WordBreak |
                TextFormatFlags.TextBoxControl |
                TextFormatFlags.ExpandTabs);

            var textHeight = measuredText.Height;
            var contentHeight = Math.Max(textHeight, _icon == CustomMessageBoxIcon.None ? 0 : IconSize);

            _textBounds = new Rectangle(textLeft, PaddingSize, textWidth, contentHeight);
            _iconBounds = new Rectangle(PaddingSize, PaddingSize, IconSize, IconSize);

            var contentAreaHeight = PaddingSize + contentHeight + PaddingSize;

            ClientSize = new Size(clientWidth, contentAreaHeight + ButtonPanelHeight);

            _buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = ButtonPanelHeight,
                BackColor = Color.Transparent
            };

            Controls.Add(_buttonPanel);

            CreateButtons();

            /* Play windows sound event for the current message box icon */
            switch (_icon)
            {
                case CustomMessageBoxIcon.Exclamation:
                    SystemSounds.Exclamation.Play();
                    break;

                case CustomMessageBoxIcon.Question:
                    SystemSounds.Question.Play();
                    break;
            }
        }
        #endregion

        #region Show method
        public static DialogResult Show(IWin32Window owner, string text, string caption, CustomMessageBoxButtons buttons, CustomMessageBoxIcon icon)
        {
            /* Create a new instance of the form and show it as modal */
            using (var dialog = new CustomMessageBox(text, caption, buttons, icon))
            {
                return owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
            }
        }
        #endregion

        #region Overrides
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            /* Make sure if the "X" close button is clicked to set the appropiate "negative" DialogResult */
            if (DialogResult == DialogResult.None)
            {
                switch (_buttons)
                {
                    case CustomMessageBoxButtons.OkCancel:
                        DialogResult = DialogResult.Cancel;
                        break;

                    case CustomMessageBoxButtons.YesNo:
                        DialogResult = DialogResult.No;
                        break;

                    default:
                        DialogResult = DialogResult.OK;
                        break;
                }
            }

            base.OnFormClosing(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            /* Draw the icon, if set */
            if (_icon != CustomMessageBoxIcon.None)
            {
                var systemIcon = _icon == CustomMessageBoxIcon.Exclamation ? SystemIcons.Warning : SystemIcons.Question;
                e.Graphics.DrawIcon(systemIcon, _iconBounds);
            }

            /* Draw the message */
            TextRenderer.DrawText(
                e.Graphics,
                _message,
                _messageFont,
                _textBounds,
                SystemColors.ControlText,
                TextFormatFlags.WordBreak |
                TextFormatFlags.TextBoxControl |
                TextFormatFlags.ExpandTabs);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _messageFont?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Private helper methods
        private void CreateButtons()
        {
            var count = _buttons == CustomMessageBoxButtons.Ok ? 1 : 2;

            var totalWidth = count * ButtonWidth + (count - 1) * ButtonGap;

            /* Right-align the buttons, remove 10px from client padding so it matches the rest of the dialogs in the application. */
            var x = (ClientSize.Width - PaddingSize - totalWidth) + 10;
            var y = (ButtonPanelHeight - ButtonHeight) / 2;

            switch (_buttons)
            {
                case CustomMessageBoxButtons.Ok:
                    AddButton("OK", DialogResult.OK, x, y);
                    break;

                case CustomMessageBoxButtons.OkCancel:
                    AddButton("OK", DialogResult.OK, x, y);
                    AddButton("Cancel", DialogResult.Cancel, x + ButtonWidth + ButtonGap, y);
                    break;

                case CustomMessageBoxButtons.YesNo:
                    AddButton("Yes", DialogResult.Yes, x, y);
                    AddButton("No", DialogResult.No, x + ButtonWidth + ButtonGap, y);
                    break;
            }
        }

        private void AddButton(string text, DialogResult result, int x, int y)
        {
            var button = new Button
            {
                Text = text,
                Bounds = new Rectangle(x, y, ButtonWidth, ButtonHeight),
                Font = _messageFont,
                DialogResult = result
            };

            _buttonPanel.Controls.Add(button);

            switch (result)
            {
                case DialogResult.OK:
                case DialogResult.Yes:
                    AcceptButton = button;
                    break;

                case DialogResult.Cancel:
                case DialogResult.No:
                    CancelButton = button;
                    break;
            }
        }
        #endregion
    }
}