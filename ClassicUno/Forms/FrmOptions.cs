/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Windows.Forms;
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Controls;

namespace ClassicUno.Forms
{
    public partial class FrmOptions : FormEx
    {
        public FrmOptions()
        {
            InitializeComponent();

            chkVoice.Checked = SettingsManager.Settings.Options.Sounds.EnableVoice;
            chkSfx.Checked = SettingsManager.Settings.Options.Sounds.EnableEffects;
            chkMusic.Checked = SettingsManager.Settings.Options.Sounds.EnableMusic;

            tbVoice.Value = SettingsManager.Settings.Options.Sounds.VoiceVolume;
            tbSfx.Value = SettingsManager.Settings.Options.Sounds.EffectsVolume;
            tbMusic.Value = SettingsManager.Settings.Options.Sounds.MusicVolume;

            chkVoice.CheckedChanged += CheckStateChanged;
            chkSfx.CheckedChanged += CheckStateChanged;
            chkMusic.CheckedChanged += CheckStateChanged;

            tbVoice.ValueChanged += TrackBarValueChanged;
            tbSfx.ValueChanged += TrackBarValueChanged;
            tbMusic.ValueChanged += TrackBarValueChanged;
        }

        private static void CheckStateChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox) sender;
            if (cb == null)
            {
                return;
            }

            switch (cb.Tag.ToString())
            {
                case "VOICECHK":
                    SettingsManager.Settings.Options.Sounds.EnableVoice = cb.Checked;
                    break;

                case "SFXCHK":
                    SettingsManager.Settings.Options.Sounds.EnableEffects = cb.Checked;
                    break;

                case "MUSICCHK":
                    SettingsManager.Settings.Options.Sounds.EnableMusic = cb.Checked;
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

        private static void TrackBarValueChanged(object sender, EventArgs e)
        {
            var tb = (VuMeterTrackBar) sender;
            if (tb == null)
            {
                return;
            }

            switch (tb.Tag.ToString())
            {
                case "TBVOICE":
                    SettingsManager.Settings.Options.Sounds.VoiceVolume = tb.Value;
                    AudioManager.SetVoiceVolume(tb.Value);
                    break;

                case "TBSFX":
                    SettingsManager.Settings.Options.Sounds.EffectsVolume = tb.Value;
                    AudioManager.SetEffectsVolume(tb.Value);
                    break;

                case "TBMUSIC":
                    SettingsManager.Settings.Options.Sounds.MusicVolume = tb.Value;
                    AudioManager.SetMusicVolume(tb.Value);
                    break;
            }
        }
    }
}
