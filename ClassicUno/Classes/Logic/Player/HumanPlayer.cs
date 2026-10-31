/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Logic.Player
{
    public class HumanPlayer : IPlayer
    {
        /*
         * Q: Why do programmers prefer dark mode?
         * A: Because light attracts bugs!
         */
        #region Fields
        public PlayerNameData NameData { get; set; }

        public int VoiceIndex { get; set; }

        public int AudioPan { get; set; }

        public List<Card> Cards { get; set; } 

        public bool IsSkipped { get; set; }

        public bool HasPassed { get; set; }

        public bool HasDrawn { get; set; }


        public event Action<IPlayer> PlayerBeginTurn;
        public event Action<IPlayer> PlayerDraw;
        public event Action<IPlayer, Card> PlayerPlaysCard;
        public event Action<IPlayer, CardColor> PlayerChoosesColor;
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
            /* Discard card isn't used */
            if (IsSkipped)
            {
                IsSkipped = false;
                EndTurn();
                return;
            }

            HasPassed = false;
            PlayerBeginTurn?.Invoke(this);
        }

        public async void PlayCard(Card card)
        {
            Cards.Remove(card);
            PlayerPlaysCard?.Invoke(this, card);
        }

        public void ChooseColor(CardColor color)
        {
            PlayerChoosesColor?.Invoke(this, color);
        }

        public void DrawCard()
        {
            HasDrawn = true;
            PlayerDraw?.Invoke(this);
        }

        public void CardDrawn(Card card)
        {
            Cards.Add(card);
        }

        public async void Pass()
        {
            HasPassed = true;
            PlayerPass?.Invoke(this);

            await Task.Delay(300);

            EndTurn();
        }

        public void EndTurn()
        {
            /* Make sure these variables are RESET. */
            IsSkipped = false;
            HasDrawn = false;
            PlayerEndTurn?.Invoke(this);
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            return NameData.Name;
        }
        #endregion

        #region Clone
        public IPlayer Clone()
        {
            return new HumanPlayer
            {
                NameData = NameData,
                VoiceIndex = VoiceIndex,
                AudioPan = AudioPan,
                Cards = Cards.Select(c => (Card)c.Clone()).ToList(),
                IsSkipped = IsSkipped,
                HasPassed = HasPassed,
                HasDrawn = HasDrawn
            };
        }
        #endregion
    }
}
