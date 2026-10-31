/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Drawing;
using System.Windows.Forms;
using ClassicUno.Classes.Shared.Management;
using ClassicUno.Controls;
using ClassicUno.Controls.Options;

namespace ClassicUno.Forms
{
    public partial class FrmOptions : FormEx
    {
        public FrmOptions()
        {
            InitializeComponent();

            /* General tab */
            var general = new OptionsGeneral(SettingsManager.Settings.General) {Dock = DockStyle.Fill, Size = new Size(306, 292) };
            tbGeneral.Controls.Add(general);

            /* Sounds tab */
            var audio = new OptionsAudio(SettingsManager.Settings.Sound) {Dock = DockStyle.Fill, Size = new Size(306, 292) };
            tbSound.Controls.Add(audio);

            tabs.SelectedIndexChanged += SelectedTabChanged;

            tabs.SelectedIndex = SettingsManager.Settings.General.TabIndex;
        }

        private void SelectedTabChanged(object sender, EventArgs e)
        {
            SettingsManager.Settings.General.TabIndex = tabs.SelectedIndex;
        }
    }
}
