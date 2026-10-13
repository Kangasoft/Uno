/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Properties;

namespace ClassicUno.Classes.Helpers.Management
{
    public static class GraphicsManager
    {
        #region CardKey
        /* KeyPair class for Dictionary object */
        private class CardKey
        {
            private readonly CardColor _color;
            private readonly CardType _type;
            private readonly int _value;

            public CardKey(CardColor color, CardType type, int value)
            {
                _color = color;
                _type = type;
                _value = value;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is CardKey other))
                {
                    return false;
                }
                return _color == other._color && _type == other._type && _value == other._value;
            }

            public override int GetHashCode()
            {
                return (int)_color * 10000 + (int)_type * 100 + _value;
            }
        }
        #endregion

        #region Fields
        /* UNO! card images - this separates current game play card data from the actual bitmaps. This makes it easier
         * to create a smaller "game-save" file without including the images in the binary data file. */
        private static readonly Dictionary<CardKey, Bitmap> CardImages = new Dictionary<CardKey, Bitmap>();

        private static float _scale;
        private static float _cardWidth;
        private static float _cardHeight;

        /* Back of the UNO! cards - this is displayed on the UI when cards are "face-down" */
        public static Bitmap CardBack { get; private set; }
        #endregion

        #region Initialization
        public static void Initialize()
        {
            BuildCardImages();
        }
        #endregion

        #region Card image building
        private static void BuildCardImages()
        {
            var image = Resources.cards;
            var startY = 0;

            /* Loop through all four colors (this is the "Y" direction */
            for (var color = 0; color <= 3; color++)
            {
                /* Loop though all 14 cards of the resource image in the "X" direction "cutting-out" the images,
                 * or cropping, into a single bitmap of each card - paying particular attention to "special" cards;
                 * such as Wild, Wild Draw Four, Skip, UNO! Reverse, etc. */

                var startX = 0; /* This must be reset each new "Y" iteration */

                for (var i = 0; i <= 13; i++)
                {
                    var rect = new Rectangle(startX, startY, 144, 216);
                    var cardImg = image.Clone(rect, image.PixelFormat);

                    /* Set the transparency of the current card image to the predifined ARGB format of RGB(1, 1, 1) */
                    cardImg.MakeTransparent(Color.FromArgb(1, 1, 1));

                    /* Check which "X" position we are currently at for "special" cards, everything else is numeric */
                    switch (i)
                    {
                        case 10:
                            /* Skip */
                            AddCardImage((CardColor)color + 1, CardType.Skip, 0, cardImg);
                            break;

                        case 11:
                            /* Reverse */
                            AddCardImage((CardColor)color + 1, CardType.Reverse, 0, cardImg);
                            break;

                        case 12:
                            /* Draw two */
                            AddCardImage((CardColor)color + 1, CardType.DrawTwo, 0, cardImg);
                            break;

                        case 13:
                            switch (color)
                            {
                                /* Wild/wild draw four */
                                case 0:
                                    /* Wild */
                                    AddCardImage(CardColor.None, CardType.Wild, 0, cardImg);
                                    break;

                                case 1:
                                    /* Wild draw four */
                                    AddCardImage(CardColor.None, CardType.DrawFour, 0, cardImg);
                                    break;

                                case 2:
                                    /* Card back */
                                    CardBack = cardImg;
                                    break;
                            }
                            break;

                        default:
                            /* Numeric */
                            AddCardImage((CardColor)color + 1, CardType.Numeric, i, cardImg);
                            break;
                    }
                    startX += 144; /* Card image width */
                }
                startY += 216; /* Card image height */
            }
        }
        #endregion

        #region Scaling
        public static void Rescale(Size clientSize)
        {
            const float referenceWidth = 1600f;
            const float referenceHeight = 850f;

            _scale = Math.Min(clientSize.Width / referenceWidth, clientSize.Height / referenceHeight);

            _cardWidth = 110f * _scale;
            _cardHeight = 160f * _scale;
        }
        #endregion

        #region Drawing methods
        public static void DrawGame(Game game, Graphics g, Rectangle clientRectangle)
        {
            /* Draw the current game in progress */
            if (game == null)
            {
                return;
            }

            var position = new PointF(clientRectangle.Width / 2f,
                (clientRectangle.Height - _cardHeight * 2) / 2f);

            DrawStatus(game, g, clientRectangle, position);
            DrawPlayerCards(game, g, clientRectangle);
            DrawPlayerNames(game, g, GetPlayerHandBounds(clientRectangle, _cardWidth, _cardHeight), clientRectangle);
        }

        #region Game status
        private static void DrawStatus(Game game, Graphics g, Rectangle clientRectangle, PointF position)
        {
            /* Draw direction indicator. Vertical bounds of the cards. */
            var cardsTop = position.Y;
            var cardsBottom = position.Y + _cardHeight;

            /* Direction image: preserve its original aspect ratio. Resource image is 64 pixels high. */
            Image directionImage = game.CurrentDirection == PlayDirection.Clockwise
                ? Resources.clockwise
                : Resources.anticlockwise;

            var directionHeight = _cardHeight * (64f / 216f);
            var directionWidth = directionHeight * (directionImage.Width / (float)directionImage.Height);

            /* Orb size: scale it relative to the card height. */
            var orbSize = _cardHeight * (64f / 216f);

            /* Direction at the top of the pick-up/discard cards. */
            var directionX = position.X - directionWidth / 2f;
            var directionY = cardsTop;

            /* Draw direction indicator */
            g.DrawImage(directionImage, new RectangleF(directionX, directionY, directionWidth, directionHeight));

            /* Draw what the direction text is, either clockwise, or anti-clockwise */
            g.DrawText(Utils.GetDescriptionFromEnumValue(game.CurrentDirection), Color.FromArgb(255, 225, 140),
                new RectangleF(position.X - 100f, directionY + directionHeight, 200f, 28f), StringAlignment.Center);

            /* Orb at the bottom of the pick-up/discard cards. */
            var orbX = position.X - orbSize / 2f;
            var orbY = cardsBottom - orbSize;
            DrawOrbColor(game, g, new PointF(orbX, orbY), orbSize);

            /* Draw pick up pile */
            var centerX = position.X;
            position.X = centerX - (_cardWidth + 20f) * 2f;
            DrawPickupPile(game, g, clientRectangle, position);

            /* Draw the discard pile last. */
            position.X = centerX + (_cardWidth - 20f) * 2f;
            DrawDiscardPile(game, g, clientRectangle, position);
        }
        #endregion

        #region Pick-up pile
        private static void DrawPickupPile(Game game, Graphics g, Rectangle clientRectangle, PointF position)
        {
            /* Draw the pick-up pile - Get the center of the clientRectangle */
            var totalCards = game.Deck.Count;

            if (totalCards == 0)
            {
                return;
            }

            /* Map total list count (up to 108) to a visual layer count between 1 and 6. We can't use the "deck" list directly, as
             * it's being modified in a background thread during dealing. */
            const int maxPossibleCards = 108;
            var ratio = (double)Math.Min(totalCards, maxPossibleCards) / maxPossibleCards;
            var visualLayers = (int)Math.Ceiling(ratio * 6);

            /* Ensures it's always between 1 and 6 */
            visualLayers = Math.Max(1, Math.Min(6, visualLayers));

            /* Loop through and draw only the calculated visual layers */
            for (var i = 0; i < visualLayers; i++)
            {
                DrawCard(g, CardBack, clientRectangle, position);
                position.X += 1;
                position.Y += 1;
            }

            /* Draw card count text under deck - not sure I like or want to keep this code... */
            g.DrawText($"{game.Deck.Count} (108)", Color.FromArgb(255, 225, 140),
                new RectangleF(position.X - 50f, position.Y + _cardHeight + 6f, 200f, 28f), StringAlignment.Center);
        }
        #endregion

        #region Color orb
        private static void DrawOrbColor(Game game, Graphics g, PointF position, float orbSize)
        {
            /* When a wild card/wild draw four is the top card, after the color has been assigned - give the user
             * a visual cue/marker. */
            if (game.CurrentColor == CardColor.None)
            {
                return;
            }

            Bitmap orb = null;

            switch (game.CurrentColor)
            {
                case CardColor.Red:
                    orb = Resources.redOrb;
                    break;

                case CardColor.Blue:
                    orb = Resources.blueOrb;
                    break;

                case CardColor.Green:
                    orb = Resources.greenOrb;
                    break;

                case CardColor.Yellow:
                    orb = Resources.yellowOrb;
                    break;
            }

            if (orb == null)
            {
                return;
            }

            g.DrawImage(orb, position.X, position.Y, orbSize, orbSize);
        }
        #endregion

        #region Discard pile
        private static void DrawDiscardPile(Game game, Graphics g, Rectangle clientRectangle, PointF position)
        {
            var count = game.DiscardPile.Count;

            if (count == 0)
            {
                return;
            }

            /* Draw a few offset cards to simulate a stack.
             * Draw up to three slightly rotated card backs underneath top card. */
            Card card;
            
            if (count > 1)
            {
                var indices = count <= 3
                    ? Enumerable.Range(0, count - 1).ToArray()
                    : new[] { 0, count - 3, count - 2 };

                for (var i = 0; i < indices.Length; i++)
                {
                    var cardIndex = indices[i];
                    var offset = indices.Length - i;

                    var x = clientRectangle.Left + position.X - offset * 2
                            + _cardWidth / 2f;
                    var y = clientRectangle.Top + position.Y - offset * 2
                            + _cardHeight / 2f;

                    card = game.DiscardPile[cardIndex];

                    DrawRotatedCard(g, CardBack, x, y, card.DrawAngle);
                }
            }

            /* Draw the face-up top card, slightly crooked. */
            var topCentreX = clientRectangle.Left + position.X + _cardWidth / 2f;
            var topCentreY = clientRectangle.Top + position.Y + _cardHeight / 2f;

            card = game.DiscardPile[game.DiscardPile.Count - 1];
            DrawRotatedCard(g, GetCardImage(card), topCentreX, topCentreY, card.DrawAngle);
        }
        #endregion

        #region Player card drawing
        private static void DrawPlayerCards(Game game, Graphics g, Rectangle clientRectangle)
        {
            /* Clockwise: bottom, left, top, right. */
            var angle = 0;
            foreach (var player in game.Players)
            {
                DrawPlayerHand(game, player, g, clientRectangle, angle);
                /* If player count is greater than 2, we add 90 degrees; otherwise we just display the players
                 * top and bottom of the client rectangle. */
                angle += game.Players.Count > 2 ? 90 : 180;
            }
        }

        private static void DrawPlayerHand(Game game, IPlayer player, Graphics g, Rectangle clientRectangle, int angle)
        {
            var count = player.Cards.Count;

            if (count == 0)
            {
                return;
            }

            /* Upright cards use their normal width for spacing. Sideways cards are spaced along their rotated width. */
            var sideways = angle == 90 || angle == 270;

            var cardWidth = _cardWidth;
            var cardHeight = _cardHeight;

            var spacing = sideways ? cardHeight * 0.17f : cardWidth * 0.30f; /* Was quarter (0.25) */
            var available = sideways ? clientRectangle.Height - 40f : clientRectangle.Width - 40f;

            if (count > 1)
            {
                spacing = Math.Min(spacing, (available - cardWidth) / (count - 1));
            }

            if (spacing < 0f)
            {
                spacing = 0f;
            }

            var totalWidth = cardWidth + (count - 1) * spacing;
            var totalHeight = cardHeight + (count - 1) * spacing;

            for (var i = 0; i <= count - 1; i++)
            {
                /* Show card back if player is not human (or show all card fronts if game over) */
                var card = GetCardImage(player.Cards[i]);
                Image image = player is HumanPlayer ? card : game.GameRunning ? CardBack : card;

                /* Add a small - Y offset on a card being drawn if hovering over it (human player only) */
                var highlightOffsetY = player.Cards[i] == game.HighlightCard ? 23f : 0f;

                if (image == null)
                {
                    /* This shouldn't happen */
                    continue;
                }

                float x;
                float y;
                if (!sideways)
                {
                    x = clientRectangle.Left + (clientRectangle.Width - totalWidth) / 2f + i * spacing;
                    y = angle == 0 ? (clientRectangle.Bottom - highlightOffsetY) - cardHeight - 120f : clientRectangle.Top + 50f;
                    DrawCard(g, image, clientRectangle, new PointF(x - clientRectangle.Left, y - clientRectangle.Top));
                }
                else
                {
                    x = angle == 90 ? clientRectangle.Left + cardWidth / 2f + 50f : clientRectangle.Right - cardWidth / 2f - 50f;
                    y = clientRectangle.Top + (clientRectangle.Height - totalHeight) / 2f + i * spacing + cardHeight / 2f - cardWidth / 2f;
                    DrawRotatedCard(g, image, x, y, angle);
                }
                /* Set card region */
                if (player is HumanPlayer)
                {
                    /* This is used for the hit test on the human player's cards */
                    player.Cards[i].Region = new RectangleF(x, y, i < count - 1 ? spacing : _cardWidth, _cardHeight);
                }
            }
        }

        private static RectangleF[] GetPlayerHandBounds(Rectangle clientRectangle, float cardWidth, float cardHeight)
        {
            const float edgeMargin = 20f;
            const float nameHeight = 28f;
            const float nameGap = 6f;

            /* Player order: bottom, left, top, right. */
            var handBounds = new RectangleF[4];

            /* Bottom hand */
            handBounds[0] = new RectangleF(clientRectangle.Left + (clientRectangle.Width - cardWidth * 2) / 2f,
                clientRectangle.Bottom - cardHeight - 120f, cardWidth * 2, cardHeight);

            /* Left hand: cards rotated 90 degrees, centred vertically. */
            handBounds[1] = new RectangleF(clientRectangle.Left + edgeMargin,
                clientRectangle.Top + (clientRectangle.Height - cardWidth) / 2f, cardHeight, cardWidth);

            /* Top hand: leave room for its name above it. */
            handBounds[2] = new RectangleF(clientRectangle.Left + (clientRectangle.Width - cardWidth * 2) / 2f,
                clientRectangle.Top + edgeMargin + nameHeight + nameGap, cardWidth * 2, cardHeight);

            /* Right hand: cards rotated 90 degrees, centred vertically. */
            handBounds[3] = new RectangleF(clientRectangle.Right - cardHeight - edgeMargin,
                clientRectangle.Top + (clientRectangle.Height - cardWidth) / 2f, cardHeight, cardWidth);

            return handBounds;
        }
        #endregion

        #region Player names
        private static void DrawPlayerNames(Game game, Graphics g, IReadOnlyList<RectangleF> handBounds, Rectangle clientRectangle)
        {
            const float gap = 6f;
            const float nameHeight = 28f;
            const float sideNameWidth = 245f;

            if (handBounds == null || handBounds.Count < 4)
            {
                return;
            }

            var nameBounds = new RectangleF[4];

            /* Bottom name: centred horizontally under the bottom hand. */
            nameBounds[0] = new RectangleF(clientRectangle.Left + (clientRectangle.Width - sideNameWidth) / 2f,
                handBounds[0].Bottom + gap, sideNameWidth, nameHeight);

            /* Left name: immediately to the right of the left hand. */
            nameBounds[1] = new RectangleF(handBounds[1].Right + gap + 10f, handBounds[1].Top, sideNameWidth, nameHeight);

            /* Top name: centred horizontally below the top hand. */
            nameBounds[2] = new RectangleF(clientRectangle.Left + (clientRectangle.Width - sideNameWidth) / 2f,
                handBounds[2].Bottom + gap, sideNameWidth, nameHeight);

            /* Right name: immediately to the left of the right hand. */
            nameBounds[3] = new RectangleF(handBounds[3].Left - sideNameWidth - gap - 10f, handBounds[3].Top, sideNameWidth, nameHeight);

            var playerCount = Math.Min(game.Players.Count, 4);

            for (var i = 0; i < playerCount; i++)
            {
                var pos = playerCount == 2 && i == 1 ? 2 : i; /* If there's only two players, we draw them top and bottom of screen */
                g.DrawText(game.Players[i].ToString(),
                    game.CurrentPlayer == game.Players[i] ? Color.DeepSkyBlue : Color.FromArgb(255, 225, 140),
                    nameBounds[pos],
                    playerCount > 2
                        ? i == 1 ? StringAlignment.Near :
                        i == 3 ? StringAlignment.Far : StringAlignment.Center
                        : StringAlignment.Center);
            }
        }
        #endregion
        #endregion

        #region Card drawing methods
        private static void DrawCard(Graphics g, Image cardImage, Rectangle clientRectangle, PointF position)
        {
            /* This draws a card anywhere on screen (related to "position") scaled; and is kept track of on resize */
            g.DrawImage(cardImage, new RectangleF(clientRectangle.Left + position.X, clientRectangle.Top + position.Y, _cardWidth, _cardHeight));
        }

        private static void DrawRotatedCard(Graphics g, Image cardImage, float centerX, float centerY, float angle)
        {
            var state = g.Save();

            try
            {
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(angle);
                g.DrawImage(cardImage, new RectangleF(-_cardWidth / 2f, -_cardHeight / 2f, _cardWidth, _cardHeight));
            }
            finally
            {
                g.Restore(state);
            }
        }
        #endregion
        
        #region Card image helpers
        private static Bitmap GetCardImage(Card card)
        {
            /* This returns the bitmap image related to the UNO! card */
            var key = new CardKey(card.Color, card.Type, card.Value);
            return CardImages.TryGetValue(key, out var image) ? image : null;
        }

        private static void AddCardImage(CardColor color, CardType type, int value, Bitmap image)
        {
            /* Create a new dictionary entry in CardImages based on parameters */
            var key = new CardKey(color, type, value);
            CardImages.Add(key, image);
        }
        #endregion
    }
}
