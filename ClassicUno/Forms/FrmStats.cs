/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */

/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Windows.Forms;
using ClassicUno.Classes.Helpers;
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public partial class FrmStats : FormEx
    {
        public FrmStats()
        {
            InitializeComponent();

            SetData();

            btnReset.Click += ButtonReset;
        }

        private void SetData()
        {
            var played = SettingsManager.Settings.Statistics.GamesPlayed;

            lblPlayed.Text = $@"{played}";

            lblTotalScore.Text = SettingsManager.Settings.Statistics.HighScore.ToString();

            var gamesWon = SettingsManager.Settings.Statistics.GamesWon;
            var gamesLost = SettingsManager.Settings.Statistics.GamesLost;
            var winPercentage = played > 0 ? Math.Round((double)gamesWon / (gamesWon + gamesLost) * 100, 1) : 0;

            pctWheel.Value = winPercentage;

            lblWins.Text = gamesWon.ToString();
            lblLosses.Text = gamesLost.ToString();

            lblShortest.Text = Utils.FormatTime(SettingsManager.Settings.Statistics.ShortestGame);
            lblLongest.Text = Utils.FormatTime(SettingsManager.Settings.Statistics.LongestGame);

            var averageGameTime = played > 0
                ? SettingsManager.Settings.Statistics.TotalGameTime / (double)played
                : 0;

            lblAvgTime.Text = Utils.FormatTime((int)averageGameTime);
        }

        private void ButtonReset(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, @"Are you sure you want to reset all game statistics? This action cannot be undone.", @"Classic UNO! 2026",
                MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }
            
            /* Reset all values */
            SettingsManager.Settings.Statistics.GamesPlayed = 0;
            SettingsManager.Settings.Statistics.TotalGameTime = 0;
            SettingsManager.Settings.Statistics.GamesWon = 0;
            SettingsManager.Settings.Statistics.GamesLost = 0;
            SettingsManager.Settings.Statistics.HighScore = 0;
            SettingsManager.Settings.Statistics.ShortestGame = 0;
            SettingsManager.Settings.Statistics.LongestGame = 0;

            SetData();
        }
    }
}
