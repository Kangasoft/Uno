/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Drawing;
using System.Xml.Serialization;
using ClassicUno.Classes.Serialization;

namespace ClassicUno.Classes.Settings.SettingsData
{
    [Serializable]
    public class GeneralData
    {
        public class WindowData
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
        }

        public class ConfirmData
        {
            [XmlAttribute("exit")]
            public bool OnExit { get; set; }

            [XmlAttribute("new")]
            public bool OnNew { get; set; }
        }

        public class RecoveryData
        {
            [XmlAttribute("save")]
            public bool Save { get; set; }

            [XmlAttribute("load")]
            public bool Load { get; set; }
        }

        [XmlAttribute("tabIndex")]
        public int TabIndex { get; set; }

        [XmlAttribute("status")]
        public bool ShowStatus { get; set; }

        [XmlAttribute("logText")]
        public bool ShowLogText { get; set; }

        [XmlAttribute("shadow")]
        public bool ShowDropShadow { get; set; }

        [XmlAttribute("cardBack")]
        public int CardBack { get; set; }

        [XmlElement("window")]
        public WindowData Window = new WindowData();

        [XmlElement("confirm")]
        public ConfirmData Confirm = new ConfirmData();

        [XmlElement("recovery")]
        public RecoveryData Recovery = new RecoveryData();
    }
}
