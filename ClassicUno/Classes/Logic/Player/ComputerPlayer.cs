/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassicUno.Classes.Settings.SettingsData;
using ClassicUno.Classes.Shared;

namespace ClassicUno.Classes.Logic.Player
{
    public class ComputerPlayer : IPlayer
    {
        #region Fields
        private Card _discardCard;
        private CardColor _currentColor;

        public ComputerDifficulty Difficulty { get; set; } = ComputerDifficulty.Easy;

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

            /* This isn't really used for computer players - except to force a UI repaint */
            PlayerBeginTurn?.Invoke(this);

            ComputerBeginTurn();
        }

        public void PlayCard(Card card)
        {
            PlayerPlaysCard?.Invoke(this, card);
        }

        public void ChooseColor(CardColor color)
        {
            if (color != CardColor.None)
            {
                return;
            }
            color = ChooseWildColor();
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
            if (!HasDrawn)
            {
                return;
            }

            /* Loop back to original background process */
            ComputerBeginTurn();
        }

        public void Pass()
        {
            HasPassed = true;
            PlayerPass?.Invoke(this);
        }

        public void EndTurn()
        {
            /* Make sure these variables are RESET. */
            IsSkipped = false;
            HasDrawn = false;
            PlayerEndTurn?.Invoke(this);
        }
        #endregion
        
        #region Private helpers
        private async void ComputerBeginTurn()
        {
            /* Analyze upper card on discard pile (_discardCard) */
            await Task.Delay(RandomEx.Shared.Next(400, 800));
            var card = SelectComputerCard();
            if (card == null)
            {
                await Task.Delay(RandomEx.Shared.Next(400, 800));
                if (!HasDrawn)
                {
                    /* Need to draw */
                    DrawCard();
                    return;
                }
                /* Have to pass */
                Pass();
                await Task.Delay(300);
                EndTurn();
                return;
            }

            Cards.Remove(card);
            PlayCard(card);

            await Task.Delay(RandomEx.Shared.Next(400, 800));

            EndTurn();
        }

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
            if (Cards.Count == 1 && RandomEx.Shared.Next(100) < 15)
            {
                var lastCard = Cards[0];

                if ((lastCard.Type == CardType.DrawTwo ||
                     lastCard.Type == CardType.DrawFour) &&
                    playableCards.Contains(lastCard))
                {
                    return null;
                }
            }

            /* Easy AI: occasionally makes a bad decision and draws unnecessarily. */
            if (RandomEx.Shared.Next(100) < 15)
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

            if (ordinaryCards.Count > 0 && RandomEx.Shared.Next(100) < 75)
            {
                choices = ordinaryCards;
            }

            return choices[RandomEx.Shared.Next(choices.Count)];
        }

        private Card SelectNormalCard(IEnumerable<Card> playableCards, IReadOnlyCollection<Card> hand)
        {
            /* More balanced - still allow a slight mistake by drawing for no reason. */
            if (RandomEx.Shared.Next(100) < 20)
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

                score += RandomEx.Shared.Next(0, 4);

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
            if (RandomEx.Shared.Next(100) < 10)
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

        public CardColor ChooseWildColor()
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
            if (colors.Count == 1 || RandomEx.Shared.Next(100) < 80)
            {
                return colors[0].Color;
            }

            /* Occasionally choose another colour held in the hand. */
            return colors[RandomEx.Shared.Next(1, colors.Count)].Color;
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
