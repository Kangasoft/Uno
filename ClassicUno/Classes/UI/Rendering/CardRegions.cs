/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Drawing;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.UI.Controls;
using ClassicUno.Classes.UI.Imaging;
using ClassicUno.Properties;

namespace ClassicUno.Classes.UI.Rendering
{
    public class CardRegions : FeltForm
    {
        #region Card and game state
        private Game _currentGame;

        /* Card image resources */
        public CardImages Cards = new CardImages();

        public int CardBackIndex { get; set; }

        /* Cached layout geometry */
        protected float LayoutScale;
        protected float CardWidth;
        protected float CardHeight;

        protected PointF StatusCenter;
        protected RectangleF DirectionBounds;
        protected RectangleF DirectionTextBounds;
        protected RectangleF OrbBounds;
        protected PointF PickupPilePosition;
        protected PointF DiscardPilePosition;
        protected RectangleF[] PlayerHandBounds = new RectangleF[4];
        protected RectangleF[] PlayerNameBounds = new RectangleF[4];
        protected RectangleF[] UnoIndicatorBounds = new RectangleF[4];

        public Game CurrentGame
        {
            get => _currentGame;
            set
            {
                if (_currentGame != null && _currentGame == value)
                {
                    return;
                }

                _currentGame = value;
            }
        }
        #endregion

        #region Scaling
        protected void ReScale()
        {
            const float referenceWidth = 1600f;
            const float referenceHeight = 850f;

            LayoutScale = Math.Min(ClientRectangle.Width / referenceWidth, ClientRectangle.Height / referenceHeight);

            CardWidth = 110f * LayoutScale;
            CardHeight = 160f * LayoutScale;

            /* Central status area shared by the direction indicator, orb and piles. */
            StatusCenter = new PointF(ClientRectangle.Width / 2f, (ClientRectangle.Height - CardHeight * 2f) / 2f);

            var directionHeight = CardHeight * (64f / 216f);
            var directionWidth = directionHeight * (Resources.clockwise.Width / (float)Resources.clockwise.Height);
            var directionY = StatusCenter.Y;

            DirectionBounds = new RectangleF(StatusCenter.X - directionWidth / 2f, directionY, directionWidth, directionHeight);
            DirectionTextBounds = new RectangleF(StatusCenter.X - 100f, directionY + directionHeight, 200f, 28f);

            var orbSize = CardHeight * (64f / 216f);
            OrbBounds = new RectangleF(StatusCenter.X - orbSize / 2f, StatusCenter.Y + CardHeight - orbSize, orbSize, orbSize);

            PickupPilePosition = new PointF(StatusCenter.X - (CardWidth + 20f) * 2f, StatusCenter.Y);
            DiscardPilePosition = new PointF(StatusCenter.X + (CardWidth - 20f) * 2f, StatusCenter.Y);

            PlayerHandBounds = GetPlayerHandBounds(CardWidth, CardHeight);
            PlayerNameBounds = GetPlayerNameBounds(PlayerHandBounds);

            /* Calculate UNO animation bounds for each player's hand.
             * Player order: bottom, left, top, right. */
            UnoIndicatorBounds = new RectangleF[4];

            /* Scale the animation relative to the card dimensions. */
            var unoWidth = CardWidth * 0.75f;
            var unoHeight = CardHeight * (64f / 144f) * 0.75f;

            for (var i = 0; i <= 3; i++)
            {
                var hand = PlayerHandBounds[i];

                float x;
                float y;

                switch (i)
                {
                    case 0: /* Bottom player */
                        x = hand.Left + (hand.Width - unoWidth) / 2f;
                        y = hand.Bottom - unoHeight * 0.65f - 12f;
                        break;

                    case 1: /* Left player - centre UNO over left-aligned name */
                        x = PlayerNameBounds[1].Left;
                        y = PlayerNameBounds[1].Top - unoHeight * LayoutScale;
                        break;

                    case 2: /* Top player */
                        x = hand.Left + (hand.Width - unoWidth) / 2f;
                        y = hand.Bottom - unoHeight * 0.65f - 12f;
                        break;

                    default: /* Right player - centre UNO over right-aligned name */
                        x = PlayerNameBounds[3].Right - unoWidth;
                        y = PlayerNameBounds[3].Top - unoHeight * LayoutScale;
                        break;
                }

                UnoIndicatorBounds[i] = new RectangleF(
                    (int)Math.Round(x),
                    (int)Math.Round(y),
                    (int)Math.Round(unoWidth),
                    (int)Math.Round(unoHeight));
            }
        }
        #endregion

        #region Bounds methods
        protected RectangleF[] GetPlayerHandBounds(float cardWidth, float cardHeight)
        {
            /* Returns the bounds of each player's hand on screen. */
            const float edgeMargin = 20f;
            const float nameHeight = 28f;
            const float nameGap = 6f;

            return new[]
            {
                /* Bottom hand */
                new RectangleF(
                    ClientRectangle.Left + (ClientRectangle.Width - cardWidth * 2f) / 2f,
                    ClientRectangle.Bottom - cardHeight - 120f,
                    cardWidth * 2f,
                    cardHeight),

                /* Left hand */
                new RectangleF(
                    ClientRectangle.Left + edgeMargin,
                    ClientRectangle.Top + (ClientRectangle.Height - cardWidth) / 2f,
                    cardHeight,
                    cardWidth),

                /* Top hand */
                new RectangleF(
                    ClientRectangle.Left + (ClientRectangle.Width - cardWidth * 2f) / 2f,
                    ClientRectangle.Top + edgeMargin + nameHeight + nameGap,
                    cardWidth * 2f,
                    cardHeight),

                /* Right hand */
                new RectangleF(
                    ClientRectangle.Right - cardHeight - edgeMargin,
                    ClientRectangle.Top + (ClientRectangle.Height - cardWidth) / 2f,
                    cardHeight,
                    cardWidth)
            };
        }

        protected RectangleF[] GetPlayerNameBounds(IReadOnlyList<RectangleF> handBounds)
        {
            /* Returns the bounds of where each player's name is to be drawn */
            const float gap = 6f;
            const float nameHeight = 28f;
            const float sideNameWidth = 245f;

            return new[]
            {
                /* Bottom name */
                new RectangleF(
                    ClientRectangle.Left + (ClientRectangle.Width - sideNameWidth) / 2f,
                    handBounds[0].Bottom + gap,
                    sideNameWidth,
                    nameHeight),

                /* Left name */
                new RectangleF(
                    handBounds[1].Right + gap + 10f,
                    handBounds[1].Top,
                    sideNameWidth,
                    nameHeight),

                /* Top name */
                new RectangleF(
                    ClientRectangle.Left + (ClientRectangle.Width - sideNameWidth) / 2f,
                    handBounds[2].Bottom + gap,
                    sideNameWidth,
                    nameHeight),

                /* Right name */
                new RectangleF(
                    handBounds[3].Left - sideNameWidth - gap - 10f,
                    handBounds[3].Top,
                    sideNameWidth,
                    nameHeight)
            };
        }

        /* Card animation */
        protected PointF GetPickupPileCenter()
        {
            return new PointF(
                ClientRectangle.Left + PickupPilePosition.X + CardWidth / 2f,
                ClientRectangle.Top + PickupPilePosition.Y + CardHeight / 2f);
        }

        protected PointF GetPlayerCardDestination(IPlayer player, out float angle)
        {
            angle = 0f;

            if (CurrentGame == null || player == null)
            {
                return PointF.Empty;
            }

            var players = CurrentGame.GameData.Players;
            var playerIndex = players.IndexOf(player);

            if (playerIndex < 0 || playerIndex >= players.Count || playerIndex >= 4)
            {
                return PointF.Empty;
            }

            /* Work out the hand position and rotation using the same
             * player ordering as DrawPlayerCards(). */
            var handIndex = players.Count == 2 && playerIndex == 1 ? 2 : playerIndex;

            angle = players.Count > 2 ? playerIndex * 90f : playerIndex * 180f;

            var sideways = Math.Abs(angle - 90f) < 0.1f || Math.Abs(angle - 270f) < 0.1f;
            var handBounds = PlayerHandBounds[handIndex];

            /* Calculate the layout as though the new card has already
             * been added to the player's hand. */
            var count = player.Cards.Count + 1;

            var spacing = sideways ? CardHeight * 0.17f : CardWidth * 0.30f;
            var available = sideways ? ClientRectangle.Height - 40f : ClientRectangle.Width - 40f;

            if (count > 1)
            {
                spacing = Math.Min(spacing, (available - CardWidth) / (count - 1));
            }

            spacing = Math.Max(0f, spacing);

            var totalWidth = CardWidth + (count - 1) * spacing;
            var totalHeight = CardHeight + (count - 1) * spacing;

            /* The new card will be the last card in the hand. */
            var cardIndex = count - 1;

            if (!sideways)
            {
                var x = handBounds.Left + (handBounds.Width - totalWidth) / 2f + cardIndex * spacing;
                var y = handBounds.Top;

                return new PointF(x + CardWidth / 2f, y + CardHeight / 2f);
            }
            else
            {
                var x = handBounds.Left + handBounds.Width / 2f;
                var y = handBounds.Top + (handBounds.Height - totalHeight) / 2f +
                    cardIndex * spacing + CardHeight / 2f - CardWidth / 2f;

                return new PointF(x, y);
            }
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Cards?.Dispose();
                Cards = null;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
