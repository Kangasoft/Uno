/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Logic.Player
{
    public interface IPlayer
    {
        /* This is the basic "contract" for each player - note: that this is a work in progress
         * and will change over time. */
        PlayerNameData NameData { get; set; }

        int VoiceIndex { get; set; }

        int AudioPan { get; set; }

        List<Card> Cards { get; set; }

        bool IsSkipped { get; set; }

        bool HasPassed { get; set; }

        bool HasDrawn { get; set; }

        event Action<IPlayer> PlayerBeginTurn;
        event Action<IPlayer> PlayerDraw;
        event Action<IPlayer, CardColor> PlayerChoosesColor;
        event Action<IPlayer, Card> PlayerPlaysCard;
        event Action<IPlayer> PlayerPass;
        event Action<IPlayer> PlayerEndTurn;

        void BeginTurn(Card card, CardColor currentColor);

        void PlayCard(Card card);

        void DrawCard(Card card);

        void Pass();

        void EndTurn();

        IPlayer Clone();
    }
}
