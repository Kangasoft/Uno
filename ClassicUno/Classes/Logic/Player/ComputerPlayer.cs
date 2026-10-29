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

        public ComputerDifficulty Difficulty { get; set; } = ComputerDifficulty.Easy;

        public PlayerNameData NameData { get; set; }

        public int VoiceIndex { get; set; }

        public int AudioPan { get; set; }

        public List<Card> Cards { get; set; }

        public bool IsSkipped { get; set; }

        public bool HasPassed { get; set; }

        public bool HasDrawn { get; set; }

        public bool HasRequestedDraw { get; set; }
        
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
            //Pass(); //test code
            //EndTurn();
            //return;

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
            HasRequestedDraw = false;
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
                    HasRequestedDraw = true;
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
            if (HasRequestedDraw)
            {
                HasRequestedDraw = false;
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

            var playableCards = hand
                .Where(card => Validation.IsCardPlayable(
                    _discardCard, card, _currentColor, hand))
                .ToList();

            if (playableCards.Count == 0)
            {
                return null;
            }

            switch (Difficulty)
            {
                case ComputerDifficulty.Easy:
                    return SelectEasyCard(playableCards);

                case ComputerDifficulty.Hard:
                    return SelectHardCard(playableCards, hand);

                default:
                    return SelectNormalCard(playableCards, hand);
            }
        }

        private Card SelectEasyCard(List<Card> playableCards)
        {
            /* Easy AI: occasionally misses a chance to play its final draw card. */
            if (Cards.Count == 1 && _rnd.Next(100) < 30)
            {
                var lastCard = Cards[0];

                if ((lastCard.Type == CardType.DrawTwo ||
                     lastCard.Type == CardType.DrawFour) &&
                    lastCard.Color == _currentColor &&
                    playableCards.Contains(lastCard))
                {
                    return null;
                }
            }

            /* Easy AI: occasionally makes a bad decision and draws unnecessarily. */
            if (_rnd.Next(100) < 22)
            {
                return null;
            }

            /* Usually prefer a card matching the current colour. */
            var matchingCards = playableCards
                .Where(c => c.Type != CardType.Wild &&
                            c.Type != CardType.DrawFour &&
                            c.Color == _currentColor)
                .ToList();

            var choices = matchingCards.Count > 0
                ? matchingCards
                : playableCards;

            /* Prefer ordinary cards over action and wild cards. */
            var ordinaryCards = choices
                .Where(c => c.Type == CardType.Numeric)
                .ToList();

            if (ordinaryCards.Count > 0 && _rnd.Next(100) < 75)
            {
                choices = ordinaryCards;
            }

            /* Pick randomly rather than calculating the perfect move. */
            return choices[_rnd.Next(choices.Count)];
        }

        private Card SelectNormalCard(IEnumerable<Card> playableCards, IReadOnlyCollection<Card> hand)
        {
            /* More balanced - still allow a slight mistake by drawing for no reason. */
            if (_rnd.Next(100) < 17)
            {
                return null;
            }

            Card bestCard = null;
            var bestScore = int.MinValue;

            foreach (var card in playableCards)
            {
                var score = 0;

                switch (card.Type)
                {
                    case CardType.Numeric:
                        score += 5 + card.Value;
                        break;

                    case CardType.Skip:
                    case CardType.Reverse:
                        score += 7;
                        break;

                    case CardType.DrawTwo:
                        score += 5;
                        break;

                    case CardType.Wild:
                        score += 2;
                        break;

                    case CardType.DrawFour:
                        score += 0;
                        break;
                }

                if (card.Type != CardType.Wild && card.Type != CardType.DrawFour)
                {
                    var matchingColorCount = hand.Count(other => other != card && other.Color == card.Color);

                    score += matchingColorCount * 2;
                }

                if (card.Color == _currentColor)
                {
                    score += 2;
                }

                if (hand.Count <= 2)
                {
                    switch (card.Type)
                    {
                        case CardType.DrawFour:
                            score += 12;
                            break;

                        case CardType.Wild:
                            score += 10;
                            break;

                        case CardType.DrawTwo:
                            score += 6;
                            break;
                    }
                }

                score += _rnd.Next(0, 4);

                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestCard = card;
            }

            return bestCard;
        }

        private Card SelectHardCard(IEnumerable<Card> playableCards, IReadOnlyCollection<Card> hand)
        {
            /* More aggressive - still introduce mistake drawing, but less likely. */
            if (_rnd.Next(100) < 10)
            {
                return null;
            }

            Card bestCard = null;
            var bestScore = int.MinValue;

            foreach (var card in playableCards)
            {
                var score = 0;

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

                if (card.Type != CardType.Wild && card.Type != CardType.DrawFour)
                {
                    var matchingColorCount = hand.Count(other => other != card && other.Color == card.Color);

                    score += matchingColorCount * 5;
                }

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
            var colors = Cards
                .Where(c => c.Color != CardColor.None)
                .GroupBy(c => c.Color)
                .Select(g => new
                {
                    Color = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            if (colors.Count == 0)
            {
                return CardColor.Red;
            }

            /* Usually choose the colour we have most of. */
            if (colors.Count == 1 || _rnd.Next(100) < 80)
            {
                return colors[0].Color;
            }

            /* Occasionally choose another colour held in the hand. */
            return colors[_rnd.Next(1, colors.Count)].Color;
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
                Difficulty = Difficulty,
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
