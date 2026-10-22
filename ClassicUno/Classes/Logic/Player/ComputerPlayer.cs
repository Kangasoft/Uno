/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Linq;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Logic.Player
{
    public class ComputerPlayer : IPlayer
    {
        #region Fields
        private Card _discardCard;
        private CardColor _currentColor;

        private BackgroundWorker _worker;

        private readonly Random _rnd = new Random();

        private bool _hasRequestedDraw;

        public PlayerNameData NameData { get; set; }

        public int VoiceIndex { get; set; }

        public int AudioPan { get; set; }

        public List<Card> Cards { get; set; }

        public bool IsSkipped { get; set; }

        public bool HasPassed { get; set; }

        public bool HasDrawn { get; set; }


        public event Action<IPlayer> PlayerBeginTurn;
        public event Action<IPlayer> PlayerDraw;
        public event Action<IPlayer, CardColor> PlayerChoosesColor;
        public event Action<IPlayer, Card> PlayerPlaysCard;
        public event Action<IPlayer> PlayerPass;
        public event Action<IPlayer> PlayerEndTurn;
        #endregion

        #region Constructor
        public ComputerPlayer()
        {
            NameData = new PlayerNameData();
            Cards = new List<Card>();
        }
        #endregion

        #region IPlayer
        public void BeginTurn(Card card, CardColor currentColor)
        {
            HasPassed = false;
            if (IsSkipped)
            {
                IsSkipped = false;
                EndTurn();
                return;
            }

            _discardCard = card;
            _currentColor = currentColor;

            _worker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _worker.DoWork += BackgroundWorkerCallback;
            _worker.RunWorkerCompleted += BackgroundWorkerEndWork;
            _worker.RunWorkerAsync();

            /* This isn't really used for computer players - except to force a UI repaint */
            PlayerBeginTurn?.Invoke(this);
        }

        public void PlayCard(Card card)
        {
            /* This isn't necessary in computer players as PlayerPlaysCard event is raised in the background worker */
        }

        public void DrawCard(Card card)
        {
            Cards.Add(card);
            if (!HasDrawn)
            {
                return;
            }
            /* Loop back to original background process */
            _worker = new BackgroundWorker { WorkerSupportsCancellation = true };
            _worker.DoWork += BackgroundWorkerCallback;
            _worker.RunWorkerCompleted += BackgroundWorkerEndWork;
            _worker.RunWorkerAsync();
        }

        public void Pass()
        {
            HasPassed = true;
            PlayerPass?.Invoke(this);
        }

        public void EndTurn()
        {
            IsSkipped = false; /* Make sure this variable is RESET */
            HasDrawn = false;
            _hasRequestedDraw = false;
            PlayerEndTurn?.Invoke(this);
        }
        #endregion

        #region Background worker
        private void BackgroundWorkerCallback(object sender, DoWorkEventArgs e)
        {
            /* Analyze upper card on discard pile (_discardCard) */
            Thread.Sleep(_rnd.Next(600, 1500));
            var card = SelectComputerCard();
            if (card == null)
            {
                Thread.Sleep(_rnd.Next(300, 1500));
                if (!HasDrawn)
                {
                    /* Need to draw */
                    HasDrawn = true;
                    _hasRequestedDraw = true;
                    PlayerDraw?.Invoke(this);
                    return;
                }
                /* Have to pass */
                Pass();
                return;
            }

            Cards.Remove(card);
            PlayerPlaysCard?.Invoke(this, card);
            if (card.Type == CardType.Wild || card.Type == CardType.DrawFour)
            {
                /* Choose color */
                var c = ChooseWildColor();
                PlayerChoosesColor?.Invoke(this, c);
            }
            Thread.Sleep(_rnd.Next(400, 800));
        }

        private void BackgroundWorkerEndWork(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_hasRequestedDraw)
            {
                _hasRequestedDraw = false;
                return;
            }

            EndTurn();
        }
        #endregion

        #region Private helpers
        private Card SelectComputerCard()
        {
            if (Cards.Count == 0)
            {
                return null;
            }
            var hand = Cards.Where(card => card != null).ToList();

            /* Only consider cards allowed by your validation rules. */
            var playableCards = hand.Where(card => Validation.IsCardPlayable(_discardCard, card, _currentColor, hand)).ToList();

            if (playableCards.Count == 0)
            {
                return null;
            }

            Card bestCard = null;
            var bestScore = int.MinValue;

            foreach (var card in playableCards)
            {
                var score = 0;
                /* Prioritise getting rid of high-penalty cards. (Note: the values are for strategy only, not actually what the cards are worth). */
                switch (card.Type)
                {
                    case CardType.DrawFour:
                        score += 100;
                        break;

                    case CardType.Wild:
                        score += 90;
                        break;

                    case CardType.DrawTwo:
                        score += 70;
                        break;

                    case CardType.Skip:
                    case CardType.Reverse:
                        score += 50;
                        break;

                    case CardType.Numeric:
                        score += card.Value;
                        break;
                }

                /* Prefer colours that leave the computer with more cards it can potentially play next. */
                if (card.Type != CardType.Wild && card.Type != CardType.DrawFour)
                {
                    var matchingColorCount = hand.Count(other => other != card && other.Color == card.Color);
                    score += matchingColorCount * 5;
                }

                /* Prefer playing a card that matches the current color when scores are otherwise similar. */
                if (card.Color == _currentColor)
                {
                    score += 2;
                }
                if (score <= bestScore)
                {
                    continue;
                }
                bestScore = score;
                bestCard = card;
            }
            return bestCard;
        }

        private CardColor ChooseWildColor()
        {
            return Cards.Where(c => c.Color != CardColor.None)
                .GroupBy(c => c.Color)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
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
            return new ComputerPlayer
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
