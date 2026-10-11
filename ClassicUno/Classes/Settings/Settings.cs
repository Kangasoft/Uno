/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Drawing;
using System.Xml.Serialization;
using ClassicUno.Classes.Serialization;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Settings
{
    [Serializable, XmlRoot("settings")]
    public sealed class Settings
    {
        [XmlAttribute("location")]
        public string LocationString
        {
            get => XmlFormatting.WritePointFormat(Location);
            set => Location = XmlFormatting.ParsePointFormat(value);
        }

        [XmlAttribute("size")]
        public string SizeString
        {
            get => XmlFormatting.WriteSizeFormat(Size);
            set => Size = XmlFormatting.ParseSizeFormat(value);
        }

        [XmlAttribute("max")]
        public bool Maximized { get; set; }

        [XmlIgnore]
        public Point Location { get; set; }

        [XmlIgnore]
        public Size Size { get; set; }

        [XmlElement("options")]
        public GameOptionData Options = new GameOptionData();

        [XmlElement("statistics")]
        public Statistics Statistics = new Statistics();

        /* Constructor */
        public Settings()
        {
            /* Set default settings */
            Size = new Size(720, 470);

            Options.NumberOfPlayers = 2;
            Options.Sounds.EnableVoice = true;
            Options.Sounds.VoiceVolume = 95;
            Options.Sounds.EnableEffects = true;
            Options.Sounds.EffectsVolume = 95;
            Options.Sounds.EnableMusic = true;
            Options.Sounds.MusicVolume = 70;
        }
    }
}
