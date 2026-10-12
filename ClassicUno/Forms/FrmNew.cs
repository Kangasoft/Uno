/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Windows.Forms;
using ClassicUno.Classes.Helpers;
using ClassicUno.Classes.Settings.SettingsData;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public partial class FrmNew : FormEx
    {
        public PlayerNameData NameData { get; set; }

        public int NumberOfPlayers { get; set; }

        public FrmNew()
        {
            InitializeComponent();

            for (var i = 0; i <= 1; i++)
            {
                cmbGender.Items.Add(Utils.GetDescriptionFromEnumValue((PlayerGender) i));
            }

            btnStart.Click += ButtonClick;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (NameData != null)
            {
                txtName.Text = NameData.Name;
                txtName.SelectionStart = txtName.TextLength;
                cmbGender.SelectedIndex = (int) NameData.Gender;
            }
            else
            {
                NameData = new PlayerNameData();
                cmbGender.SelectedIndex = 0;
            }
            cmbPlayers.SelectedIndex = NumberOfPlayers > 0 ? NumberOfPlayers - 2 : 0;
            base.OnLoad(e);
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            if (txtName.Text.Length == 0)
            {
                MessageBox.Show(this, @"Please enter a name.", @"Classic UNO! 2026", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            NameData.Name = txtName.Text;
            NameData.Gender = (PlayerGender) cmbGender.SelectedIndex;

            NumberOfPlayers = cmbPlayers.SelectedIndex + 2;
            DialogResult = DialogResult.OK;
        }
    }
}
