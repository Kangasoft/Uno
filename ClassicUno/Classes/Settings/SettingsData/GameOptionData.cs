/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ClassicUno.Classes.Settings.SettingsData
{
    [Serializable]
    public enum PlayerGender
    {
        [Description("Male")]
        Male = 0,

        [Description("Female")]
        Female = 1
    }

    [Serializable]
    public class PlayerNameData
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("gender")]
        public PlayerGender Gender { get; set; }

        [XmlAttribute("selectVoice")]
        public bool SelectVoice { get; set; }

        [XmlAttribute("voiceIndex")]
        public int VoiceIndex { get; set; }
    }

    [Serializable]
    public class GameOptionData
    {
        [XmlElement("human")]
        public PlayerNameData NameData = new PlayerNameData();

        [XmlAttribute("players")]
        public int NumberOfPlayers { get; set; }
    }
}
