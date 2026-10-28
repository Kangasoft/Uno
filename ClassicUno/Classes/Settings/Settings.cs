/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using ClassicUno.Classes.Serialization;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Settings
{
    [Serializable, XmlRoot("settings")]
    public sealed class Settings
    {
        [XmlElement("game")]
        public GameOptionData Game = new GameOptionData();

        [XmlElement("general")]
        public GeneralData General = new GeneralData();

        [XmlElement("sound")]
        public SoundData Sound = new SoundData();

        [XmlElement("statistics")]
        public StatisticsData Statistics = new StatisticsData();

        [XmlElement("savedGames")]
        public BindingList<SavedGameData> SavedGames = new BindingList<SavedGameData>();

        /* Constructor */
        public Settings()
        {
            /* Set default settings */
            Game.NumberOfPlayers = 2;

            /* Window defaults */
            General.Window.Size = new Size(720, 470);

            General.Confirm.OnExit = true;
            General.Confirm.OnNew = true;
            General.ShowStatus = true;
            General.ShowLogText = true;
            General.ShowDropShadow = true;

            /* Sound options */
            Sound.EnableVoice = true;
            Sound.VoiceVolume = 90;
            Sound.EnableEffects = true;
            Sound.EffectsVolume = 90;
            Sound.EnableMusic = true;
            Sound.MusicVolume = 70;
        }
    }
}
