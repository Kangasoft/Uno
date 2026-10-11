/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Logic.Player
{
    [Serializable]
    public class HumanPlayer : IPlayer
    {
        public PlayerNameData NameData { get; set; }

        public int VoiceIndex { get; set; }

        public List<Card> Cards { get; set; }
        public bool IsSkipped { get; set; }

        public event Action<IPlayer> PlayerBeginTurn;
        public event Action<IPlayer> PlayerDraw; /* Not used on human players */
        public event Action<IPlayer, CardColor> PlayerChoosesColor; /* Not used on human players */
        public event Action<IPlayer, Card> PlayerPlaysCard;
        public event Action<IPlayer> PlayerPass;
        public event Action<IPlayer> PlayerEndTurn;

        public HumanPlayer()
        {
            NameData = new PlayerNameData();
            Cards = new List<Card>();
        }

        public void BeginTurn(Card card, CardColor currentColor)
        {
            /* Discard card "card" isn't necessary for human player */
            if (IsSkipped)
            {
                Debug.Print(NameData.Name + " SKIPPED!");
                IsSkipped = false;
                EndTurn();
                return;
            }
            PlayerBeginTurn?.Invoke(this);
        }

        public void PlayCard(Card card)
        {
            Cards.Remove(card);
            PlayerPlaysCard?.Invoke(this, card);
        }

        public void DrawCard(Card card)
        {
            Cards.Add(card);
        }

        public void Pass()
        {
            PlayerPass?.Invoke(this);
        }

        public void EndTurn()
        {
            PlayerEndTurn?.Invoke(this);
        }
    }
}
