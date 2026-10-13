/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Windows.Forms;
using System.Drawing;
using ClassicUno.Classes.Settings.SettingsData;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public sealed class FrmStats : FormEx
    {
        private Statistics _stats;

        private readonly GameStatistics _ctlStats;
        private readonly Button _btnClose;

        public Statistics Statistics
        {
            get => _stats;
            set
            {
                _stats = value;
                _ctlStats.Statistics = _stats;
            }
        }

        public FrmStats()
        {
            ClientSize = new Size(568, 347);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Game Statistics";
            AcceptButton = _btnClose;

            _ctlStats = new GameStatistics
            {
                BackColor = Color.Transparent,
                Dock = DockStyle.Top,
                Font = new Font("Arial", 10F),
                ForeColor = Color.Black,
                Location = new Point(0, 0),
                Margin = new Padding(0),
                MinimumSize = new Size(380, 300),
                Name = "ctlStats",
                Size = new Size(568, 302),
                TabStop = false
            };

            var btnReset = new Button
            {
                Location = new Point(400, 312),
                Name = "btnReset",
                Size = new Size(75, 23),
                TabIndex = 1,
                Text = @"Reset",
                UseVisualStyleBackColor = true
            };
            btnReset.Click += ResetClick;

            _btnClose = new Button
            {
                DialogResult = DialogResult.OK,
                Location = new Point(481, 312),
                Name = "btnClose",
                Size = new Size(75, 23),
                TabIndex = 0,
                Text = @"Close",
                UseVisualStyleBackColor = true
            };

            Controls.AddRange(new Control[]
            {
                _ctlStats, btnReset, _btnClose
            });
        }

        private void ResetClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(this,
                @"Are you sure you want to reset all game statistics? This action cannot be undone.",
                @"Classic UNO! 2026",
                MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            _stats = new Statistics();
            _ctlStats.Statistics = _stats;
        }
    }
}
