/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.ComponentModel;
using System.Drawing;

namespace ClassicUno.Classes.Logic
{
    [Serializable]
    public enum CardColor
    {
        [Description("None")]
        None = 0,

        [Description("Red")]
        Red = 1,

        [Description("Yellow")]
        Yellow = 2,

        [Description("Green")]
        Green = 3,

        [Description("Blue")]
        Blue = 4
    }

    [Serializable]
    public enum CardType
    {
        None = 0,
        Numeric = 1,
        Skip = 2,
        Reverse = 3,
        DrawTwo = 4,
        DrawFour = 5,
        Wild = 6
    }

    [Serializable]
    public class Card
    {
        public CardType Type { get; set; }

        public CardColor Color { get; set; }

        public int Value { get; set; }

        public RectangleF Region { get; set; }

        public float DrawAngle { get; set; }
    }
}
