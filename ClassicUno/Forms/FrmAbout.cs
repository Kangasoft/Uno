/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ClassicUno.Controls;
using ClassicUno.Properties;

namespace ClassicUno.Forms
{
    public sealed class FrmAbout : FormEx
    {
        private readonly Button _btnOk;

        public FrmAbout()
        {
            ClientSize = new Size(310, 272);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = $@"About {Resources.appTitle}";
            AcceptButton = _btnOk;

            /* Controls */
            var pnlIcon = new Panel
            {
                BackColor = Color.Transparent,
                BackgroundImage = Resources.unoIcon,
                BackgroundImageLayout = ImageLayout.Center,
                Location = new Point(14, 14),
                Size = new Size(64, 64),
                TabIndex = 0
            };

            var lblTitle = new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point, 0),
                Location = new Point(84, 14),
                Size = new Size(153, 32),
                TabIndex = 2,
                Text = Resources.appTitle
            };

            var lblInfo = new Label
            {
                BackColor = Color.Transparent,
                Location = new Point(87, 58),
                Size = new Size(222, 100),
                TabIndex = 5
            };

            var lblTradeMark = new Label
            {
                BackColor = Color.Transparent,
                Location = new Point(14, 171),
                Size = new Size(295, 53),
                TabIndex = 6,
                Text =
                    @"All card images and the UNO! logo are © Copyright and a registered trademark of Mattel. This game was reproduced for entertainment purposes only."
            };

            _btnOk = new Button
            {
                DialogResult = DialogResult.OK,
                Location = new Point(223, 237),
                Size = new Size(75, 23),
                TabIndex = 1,
                Text = @"Ok",
                UseVisualStyleBackColor = true
            };

            Controls.AddRange(new Control[] {pnlIcon, lblTitle, lblInfo, lblTradeMark, _btnOk});

            /* Get current assembly */
            var assembly = Assembly.GetExecutingAssembly();

            /* Get version information */
            var version = assembly.GetName().Version;
            lblInfo.Text = string.Format(Resources.aboutInfo, version.Major, version.Minor);
        }
    }
}
