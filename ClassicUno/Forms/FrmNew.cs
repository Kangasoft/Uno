/* Classic Uno 2026
 * Version: 1.0.1
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
        private readonly object[] _maleVoice = {
            "Juan",
            "Robert",
            "Dan",
            "Adam",
            "Richard",
            "Tom"
        };

        private readonly object[] _femaleVoice = {
            "Debbie",
            "Samantha",
            "Jenny",
            "Vicky",
            "Michelle",
            "Anna"
        };

        public PlayerNameData NameData { get; set; }

        public int NumberOfPlayers { get; set; }

        public bool TournamentMode { get; set; }


        public FrmNew()
        {
            InitializeComponent();

            for (var i = 0; i <= 1; i++)
            {
                cmbGender.Items.Add(Utils.GetDescriptionFromEnumValue((PlayerGender) i));
            }

            cmbGender.SelectedIndexChanged += GenderSelectedIndexChanged;

            chkSelect.CheckedChanged += CheckStateChanged;
            chkTournament.CheckStateChanged += CheckStateChanged;

            btnStart.Click += ButtonClick;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (NameData != null)
            {
                txtName.Text = NameData.Name;
                txtName.SelectionStart = txtName.TextLength;
                cmbGender.SelectedIndex = (int) NameData.Gender;
                chkSelect.Checked = NameData.SelectVoice;
                cmbVoice.SelectedIndex = NameData.VoiceIndex;
            }
            else
            {
                NameData = new PlayerNameData
                {
                    SelectVoice = true
                };
                cmbGender.SelectedIndex = 0;

                chkSelect.Checked = true;
                cmbVoice.SelectedIndex = 0;
            }
            cmbPlayers.SelectedIndex = NumberOfPlayers > 0 ? NumberOfPlayers - 2 : 0;

            var state = TournamentMode;
            chkTournament.Checked = state;
            //lblPlayers.Enabled = !state;
            //cmbPlayers.Enabled = !state;

            base.OnLoad(e);
        }

        private void CheckStateChanged(object sender, EventArgs e)
        {
            var check = (CheckBox) sender;
            if (check == null)
            {
                return;
            }

            switch (check.Tag.ToString())
            {
                case "VOICE":
                    cmbVoice.Enabled = check.Checked;
                    break;

                case "TOURNAMENT":
                    var state = check.Checked;
                    lblPlayers.Enabled = !state;
                    cmbPlayers.Enabled = !state;
                    TournamentMode = state;
                    break;
            }
        }

        private void GenderSelectedIndexChanged(object sender, EventArgs e)
        {
            cmbVoice.Items.Clear();
            switch (cmbGender.SelectedIndex)
            {
                case 0:
                    /* Male */
                    cmbVoice.Items.AddRange(_maleVoice);
                    break;

                default:
                    /* Female */
                    cmbVoice.Items.AddRange(_femaleVoice);
                    break;
            }

            cmbVoice.SelectedIndex = NameData.VoiceIndex;
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

            NameData.SelectVoice = chkSelect.Checked;
            NameData.VoiceIndex = cmbVoice.SelectedIndex;

            NumberOfPlayers = cmbPlayers.SelectedIndex + 2;
            DialogResult = DialogResult.OK;
        }
    }
}
