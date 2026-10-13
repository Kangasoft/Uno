/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Logic.Player
{
    [Serializable]
    public class HumanPlayer : IPlayer
    {
        #region Fields
        public PlayerNameData NameData { get; set; }

        public int VoiceIndex { get; set; }
        public int AudioPan { get; set; }

        public List<Card> Cards { get; set; }
        public bool IsSkipped { get; set; }
        public bool HasPassed { get; set; }

        public event Action<IPlayer> PlayerBeginTurn;
        public event Action<IPlayer> PlayerDraw; /* Not used on human players */
        public event Action<IPlayer, CardColor> PlayerChoosesColor; /* Not used on human players */
        public event Action<IPlayer, Card> PlayerPlaysCard;
        public event Action<IPlayer> PlayerPass;
        public event Action<IPlayer> PlayerEndTurn;
        #endregion

        #region Constructor
        public HumanPlayer()
        {
            NameData = new PlayerNameData();
            Cards = new List<Card>();
        }
        #endregion

        #region IPlayer
        public void BeginTurn(Card card, CardColor currentColor)
        {
            /* Discard card "card" isn't necessary for human player */
            if (IsSkipped)
            {
                IsSkipped = false;
                EndTurn();
                return;
            }

            HasPassed = false;
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
            HasPassed = true;
            PlayerPass?.Invoke(this);
        }

        public void EndTurn()
        {
            IsSkipped = false; /* Make sure this variable is RESET */
            PlayerEndTurn?.Invoke(this);
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            return NameData.Name;
        }
        #endregion
    }
}
