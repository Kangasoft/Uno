/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Windows.Forms;
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Controls.Options
{
    public partial class OptionsAudio : UserControl
    {
        private readonly SoundData _settingsData;

        public OptionsAudio(SoundData data)
        {
            _settingsData = data;

            InitializeComponent();

            chkVoice.Checked = _settingsData.EnableVoice;
            chkSfx.Checked = _settingsData.EnableEffects;
            chkMusic.Checked = _settingsData.EnableMusic;

            tbVoice.Value = _settingsData.VoiceVolume;
            tbSfx.Value = _settingsData.EffectsVolume;
            tbMusic.Value = _settingsData.MusicVolume;

            chkVoice.CheckedChanged += CheckStateChanged;
            chkSfx.CheckedChanged += CheckStateChanged;
            chkMusic.CheckedChanged += CheckStateChanged;

            tbVoice.ValueChanged += TrackBarValueChanged;
            tbSfx.ValueChanged += TrackBarValueChanged;
            tbMusic.ValueChanged += TrackBarValueChanged;
        }

        private void CheckStateChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;
            if (cb == null)
            {
                return;
            }

            switch (cb.Tag.ToString())
            {
                case "VOICECHK":
                    _settingsData.EnableVoice = cb.Checked;
                    break;

                case "SFXCHK":
                    _settingsData.EnableEffects = cb.Checked;
                    break;

                case "MUSICCHK":
                    _settingsData.EnableMusic = cb.Checked;
                    switch (cb.Checked)
                    {
                        case true:
                            AudioManager.PlayMusic();
                            break;

                        default:
                            AudioManager.StopMusic();
                            break;
                    }

                    break;
            }
        }

        private void TrackBarValueChanged(object sender, EventArgs e)
        {
            var tb = (VuMeterTrackBar)sender;
            if (tb == null)
            {
                return;
            }

            switch (tb.Tag.ToString())
            {
                case "TBVOICE":
                    _settingsData.VoiceVolume = tb.Value;
                    AudioManager.SetVoiceVolume(tb.Value);
                    break;

                case "TBSFX":
                    _settingsData.EffectsVolume = tb.Value;
                    AudioManager.SetEffectsVolume(tb.Value);
                    break;

                case "TBMUSIC":
                    _settingsData.MusicVolume = tb.Value;
                    AudioManager.SetMusicVolume(tb.Value);
                    break;
            }
        }
    }
}
