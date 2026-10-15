/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.IO;
using ClassicUno.Classes.Serialization;
using ClassicUno.Properties;

namespace ClassicUno.Classes.Helpers.Management
{
    public static class SettingsManager
    {
        /* It's worse than that, he's DEAD Jim! */
        private static readonly string
            FilePath = Utils.MainDir(string.Format(Resources.userPath, "settings.xml"), true);

        public static Settings.Settings Settings { get; set; }

        static SettingsManager()
        {
            Settings = new Settings.Settings();
        }

        public static void Load()
        {
            if (!File.Exists(FilePath))
            {
                XmlSerialize<Settings.Settings>.Save(FilePath, Settings);
                return;
            }

            var s = new Settings.Settings();
            if (XmlSerialize<Settings.Settings>.Load(FilePath, ref s))
            {
                Settings = s;
            }
        }

        public static void Save()
        {
            XmlSerialize<Settings.Settings>.Save(FilePath, Settings);
        }
    }
}
