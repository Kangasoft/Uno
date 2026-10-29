/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Xml.Serialization;

namespace ClassicUno.Classes.Settings.SettingsData
{
    public class SavedGameData
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("fileName")]
        public string FileName { get; set; }

        [XmlAttribute("date")]
        public DateTime DateSaved { get; set; }

        [XmlAttribute("tournament")]
        public bool Tournament { get; set; }
    }
}