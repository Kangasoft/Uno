/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Windows.Forms;
using ClassicUno.Classes.Settings.SettingsData;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public partial class FrmStats : FormEx
    {
        private Statistics _stats;

        public Statistics Statistics
        {
            get => _stats;
            set
            {
                _stats = value;
                ctlStats.SetStatistics(_stats);
            }
        }

        public FrmStats()
        {
            InitializeComponent();

            btnReset.Click += ResetClick;
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
            ctlStats.SetStatistics(_stats);
        }
    }
}
