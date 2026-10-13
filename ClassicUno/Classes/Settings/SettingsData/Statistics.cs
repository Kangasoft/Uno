/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Xml.Serialization;

namespace ClassicUno.Classes.Settings.SettingsData
{
    [Serializable]
    public class Statistics
    {
        [XmlAttribute("gamesPlayed")]
        public int GamesPlayed { get; set; }

        [XmlAttribute("gamesWon")]
        public int GamesWon { get; set; }

        [XmlAttribute("gamesLost")]
        public int GamesLost { get; set; }

        [XmlAttribute("currentStreak")]
        public int CurrentStreak { get; set; }

        [XmlAttribute("longestStreak")]
        public int LongestStreak { get; set; }

        [XmlAttribute("shortestGame")]
        public int ShortestGame { get; set; }

        [XmlAttribute("longestGame")]
        public int LongestGame { get; set; }

        [XmlAttribute("totalGameTime")]
        public long TotalGameTime { get; set; }

        [XmlAttribute("highScore")]
        public int HighScore { get; set; }
    }
}
