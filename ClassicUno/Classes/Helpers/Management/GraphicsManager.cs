/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        private static Rectangle _clientRectangle;

        private static float _scale;
        private static float _cardWidth;
        private static float _cardHeight;

        /*
         * Cached layout geometry. These rectangles/positions are calculated by Rescale()
         * and then reused by the paint methods.
         */
        private static PointF _statusCenter;
        private static RectangleF _directionBounds;
        private static RectangleF _directionTextBounds;
        private static RectangleF _orbBounds;
        private static PointF _pickupPilePosition;
        private static PointF _discardPilePosition;
        private static RectangleF[] _playerHandBounds = new RectangleF[4];
        private static RectangleF[] _playerNameBounds = new RectangleF[4];
        private static RectangleF[] _unoIndicatorBounds = new RectangleF[4]; /* TODO */

        private static readonly Color TextColor = Color.FromArgb(255, 225, 140);

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
        public static void Rescale(Rectangle clientRectangle)
        {
            _clientRectangle = clientRectangle;

            const float referenceWidth = 1600f;
            const float referenceHeight = 850f;

            _scale = Math.Min(_clientRectangle.Width / referenceWidth, _clientRectangle.Height / referenceHeight);

            _cardWidth = 110f * _scale;
            _cardHeight = 160f * _scale;

            /*
             * OLD LAYOUT CALCULATIONS (moved here from the drawing methods):
             *
             * var position = new PointF(_clientRectangle.Width / 2f,
             *     (_clientRectangle.Height - _cardHeight * 2) / 2f);
             *
             * var handBounds = GetPlayerHandBounds(_cardWidth, _cardHeight);
             *
             * var directionHeight = _cardHeight * (64f / 216f);
             * var directionWidth = directionHeight *
             *     (directionImage.Width / (float)directionImage.Height);
             *
             * var orbSize = _cardHeight * (64f / 216f);
             *
             * var centerX = position.X;
             * position.X = centerX - (_cardWidth + 20f) * 2f; // pickup pile
             * position.X = centerX + (_cardWidth - 20f) * 2f; // discard pile
             */

            /* Central status area shared by the direction indicator, orb and piles. */
            /* Status drawing uses client-relative coordinates, as the original methods did. */
            _statusCenter = new PointF(
                _clientRectangle.Width / 2f,
                (_clientRectangle.Height - _cardHeight * 2f) / 2f);

            var directionHeight = _cardHeight * (64f / 216f);
            var directionWidth = directionHeight * (Resources.clockwise.Width / (float)Resources.clockwise.Height);
            var directionY = _statusCenter.Y;

            _directionBounds = new RectangleF(
                _statusCenter.X - directionWidth / 2f,
                directionY,
                directionWidth,
                directionHeight);

            _directionTextBounds = new RectangleF(
                _statusCenter.X - 100f,
                directionY + directionHeight,
                200f,
                28f);

            var orbSize = _cardHeight * (64f / 216f);
            _orbBounds = new RectangleF(
                _statusCenter.X - orbSize / 2f,
                _statusCenter.Y + _cardHeight - orbSize,
                orbSize,
                orbSize);

            _pickupPilePosition = new PointF(
                _statusCenter.X - (_cardWidth + 20f) * 2f,
                _statusCenter.Y);

            _discardPilePosition = new PointF(
                _statusCenter.X + (_cardWidth - 20f) * 2f,
                _statusCenter.Y);

            _playerHandBounds = GetPlayerHandBounds(_cardWidth, _cardHeight);
            _playerNameBounds = GetPlayerNameBounds(_playerHandBounds);
        }
        #endregion

        #region Drawing methods
        public static void DrawGame(Game game, Graphics g)
        {
            /* Draw the current game in progress */
            if (game == null)
            {
                return;
            }

            /*
             * OLD:
             * var position = new PointF(_clientRectangle.Width / 2f,
             *     (_clientRectangle.Height - _cardHeight * 2) / 2f);
             * DrawPlayerNames(game, g, GetPlayerHandBounds(_cardWidth, _cardHeight));
             */

            DrawStatus(game, g);
            DrawPlayerCards(game, g);
            DrawPlayerNames(game, g, _playerNameBounds);

            if (game.GameState != GameState.Paused)
            {
                return;
            }

            const string text = "GAME PAUSED";
            const int fontSize = 28;

            SizeF size;
            using (var font = new Font("Arial", fontSize, FontStyle.Bold))
            {
                size = g.MeasureString(text, font);
            }

            /*
             * OLD:
             * var bounds = new RectangleF(position.X - size.Width / 2f,
             *     _clientRectangle.Height / 2f + size.Height, size.Width, size.Height);
             */
            var bounds = new RectangleF(
                _statusCenter.X - size.Width / 2f,
                _clientRectangle.Height / 2f + size.Height,
                size.Width,
                size.Height);
            g.DrawText(text, TextColor, bounds, StringAlignment.Center, fontSize);
        }

        #region Game status
        private static void DrawStatus(Game game, Graphics g)
        {
            /*
             * OLD:
             * var cardsTop = position.Y;
             * var cardsBottom = position.Y + _cardHeight;
             * var directionHeight = _cardHeight * (64f / 216f);
             * var directionWidth = directionHeight *
             *     (directionImage.Width / (float)directionImage.Height);
             * var orbSize = _cardHeight * (64f / 216f);
             * var directionX = position.X - directionWidth / 2f;
             * var directionY = cardsTop;
             * var orbX = position.X - orbSize / 2f;
             * var orbY = cardsBottom - orbSize;
             * var centerX = position.X;
             * position.X = centerX - (_cardWidth + 20f) * 2f;
             * position.X = centerX + (_cardWidth - 20f) * 2f;
             */

            Image directionImage = game.GameData.CurrentDirection == PlayDirection.Clockwise
                ? Resources.clockwise
                : Resources.anticlockwise;

            g.DrawImage(directionImage, _directionBounds);

            g.DrawText(
                Utils.GetDescriptionFromEnumValue(game.GameData.CurrentDirection),
                TextColor,
                _directionTextBounds,
                StringAlignment.Center);

            DrawOrbColor(game, g);
            DrawPickupPile(game, g);
            DrawDiscardPile(game, g);
        }
        #endregion

        #region Pick-up pile
        private static void DrawPickupPile(Game game, Graphics g)
        {
            /* Draw the pick-up pile - Get the center of the clientRectangle */
            var totalCards = game.GameData.Deck.Count;

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
            var offset = 0;
            for (var i = 0; i < visualLayers; i++)
            {
                var position = new PointF(_pickupPilePosition.X + offset, _pickupPilePosition.Y + offset);
                DrawCard(g, CardBack, position);
                //_pickupPilePosition.X += 1;
                //_pickupPilePosition.Y += 1;
                offset++;
            }

            /* Draw card count text under deck - not sure I like or want to keep this code... */
            g.DrawText($"{game.GameData.Deck.Count} (108)", TextColor,
                new RectangleF(_pickupPilePosition.X - 50f, _pickupPilePosition.Y + _cardHeight + 6f, 200f, 28f),
                StringAlignment.Center);
        }
        #endregion

        #region Color orb
        private static void DrawOrbColor(Game game, Graphics g)
        {
            /* When a wild card/wild draw four is the top card, after the color has been assigned - give the user
             * a visual cue/marker. */
            if (game.GameData.CurrentColor == CardColor.None)
            {
                return;
            }

            Bitmap orb = null;

            switch (game.GameData.CurrentColor)
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

            /*
             * OLD:
             * g.DrawImage(orb, position.X, position.Y, orbSize, orbSize);
             */
            g.DrawImage(orb, _orbBounds);
        }
        #endregion

        #region Discard pile
        private static void DrawDiscardPile(Game game, Graphics g)
        {
            var count = game.GameData.DiscardPile.Count;

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

                    var x = _clientRectangle.Left + _discardPilePosition.X - offset * 2
                            + _cardWidth / 2f;
                    var y = _clientRectangle.Top + _discardPilePosition.Y - offset * 2
                            + _cardHeight / 2f;

                    card = game.GameData.DiscardPile[cardIndex];

                    DrawRotatedCard(g, CardBack, x, y, card.DrawAngle);
                }
            }

            /* Draw the face-up top card, slightly crooked. */
            var topCentreX = _clientRectangle.Left + _discardPilePosition.X + _cardWidth / 2f;
            var topCentreY = _clientRectangle.Top + _discardPilePosition.Y + _cardHeight / 2f;

            card = game.GameData.DiscardPile[game.GameData.DiscardPile.Count - 1];
            DrawRotatedCard(g, GetCardImage(card), topCentreX, topCentreY, card.DrawAngle);
        }
        #endregion

        #region Player card drawing
        private static void DrawPlayerCards(Game game, Graphics g)
        {
            /* Clockwise: bottom, left, top, right. */
            var angle = 0;
            var count = game.GameData.Players.Count;
            for (var playerIndex = 0; playerIndex < count; playerIndex++)
            {
                DrawPlayerHand(game, game.GameData.Players[playerIndex], g, angle, count == 2 && playerIndex == 1 ? 2 : playerIndex);

                /* If player count is greater than 2, we add 90 degrees; otherwise we just display the players
                 * top and bottom of the client rectangle. */
                angle += game.GameData.Players.Count > 2 ? 90 : 180;
            }
        }

        private static void DrawPlayerHand(Game game, IPlayer player, Graphics g, int angle, int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= _playerHandBounds.Length)
            {
                return;
            }

            var count = player.Cards.Count;
            if (count == 0)
            {
                return;
            }

            var sideways = angle == 90 || angle == 270;
            var handBounds = _playerHandBounds[playerIndex];

            /*
             * OLD POSITION CALCULATIONS:
             *
             * var spacing = sideways ? _cardHeight * 0.17f : _cardWidth * 0.30f;
             * var available = sideways ? _clientRectangle.Height - 40f : _clientRectangle.Width - 40f;
             * if (count > 1)
             *     spacing = Math.Min(spacing, (available - _cardWidth) / (count - 1));
             * if (spacing < 0f) spacing = 0f;
             * var totalWidth = _cardWidth + (count - 1) * spacing;
             * var totalHeight = _cardHeight + (count - 1) * spacing;
             *
             * Upright hand start:
             * x = _clientRectangle.Left + (_clientRectangle.Width - totalWidth) / 2f + i * spacing;
             * y = angle == 0 ? (_clientRectangle.Bottom - selectedOffsetY) - _cardHeight - 120f
             *                : _clientRectangle.Top + 50f;
             *
             * Sideways hand start:
             * x = angle == 90 ? _clientRectangle.Left + _cardWidth / 2f + 50f
             *                 : _clientRectangle.Right - _cardWidth / 2f - 50f;
             * y = _clientRectangle.Top + (_clientRectangle.Height - totalHeight) / 2f
             *     + i * spacing + _cardHeight / 2f - _cardWidth / 2f;
             */

            var spacing = sideways ? _cardHeight * 0.17f : _cardWidth * 0.30f;
            var available = sideways ? _clientRectangle.Height - 40f : _clientRectangle.Width - 40f;

            if (count > 1)
            {
                spacing = Math.Min(spacing, (available - _cardWidth) / (count - 1));
            }

            spacing = Math.Max(0f, spacing);

            var totalWidth = _cardWidth + (count - 1) * spacing;
            var totalHeight = _cardHeight + (count - 1) * spacing;

            for (var i = 0; i < count; i++)
            {
                var card = player.Cards[i];
                var cardImage = GetCardImage(card);
                Image image = player is HumanPlayer
                    ? cardImage
                    : game.GameState != GameState.Stopped ? CardBack : cardImage;

                var selectedOffsetY = card == game.SelectedCard ? 23f : 0f;

                if (image == null)
                {
                    continue;
                }

                float x;
                float y;

                if (!sideways)
                {
                    x = handBounds.Left + (handBounds.Width - totalWidth) / 2f + i * spacing;
                    y = handBounds.Top - selectedOffsetY;

                    DrawCard(g, image, new PointF(x - _clientRectangle.Left, y - _clientRectangle.Top));

                    if (player is HumanPlayer &&
                        game.HighlightedCard != null &&
                        game.HighlightedCard != game.SelectedCard &&
                        game.HighlightedCard == card)
                    {
                        DrawHoverCardHighlight(g, game.HighlightedCard);
                    }
                }
                else
                {
                    x = handBounds.Left + handBounds.Width / 2f;
                    y = handBounds.Top + (handBounds.Height - totalHeight) / 2f
                        + i * spacing + _cardHeight / 2f - _cardWidth / 2f;

                    DrawRotatedCard(g, image, x, y, angle);
                }

                if (player is HumanPlayer)
                {
                    /* Existing hit-test region behaviour retained. */
                    card.Region = new RectangleF(
                        x,
                        y,
                        i < count - 1 ? spacing : _cardWidth,
                        _cardHeight);
                }
            }
        }

        private static RectangleF[] GetPlayerHandBounds(float cardWidth, float cardHeight)
        {
            /*
             * OLD:
             * const float edgeMargin = 20f;
             * const float nameHeight = 28f;
             * const float nameGap = 6f;
             * var handBounds = new RectangleF[4];
             * handBounds[0] = new RectangleF(_clientRectangle.Left + (_clientRectangle.Width - cardWidth * 2) / 2f,
             *     _clientRectangle.Bottom - cardHeight - 120f, cardWidth * 2, cardHeight);
             * handBounds[1] = new RectangleF(_clientRectangle.Left + edgeMargin,
             *     _clientRectangle.Top + (_clientRectangle.Height - cardWidth) / 2f, cardHeight, cardWidth);
             * handBounds[2] = new RectangleF(_clientRectangle.Left + (_clientRectangle.Width - cardWidth * 2) / 2f,
             *     _clientRectangle.Top + edgeMargin + nameHeight + nameGap, cardWidth * 2, cardHeight);
             * handBounds[3] = new RectangleF(_clientRectangle.Right - cardHeight - edgeMargin,
             *     _clientRectangle.Top + (_clientRectangle.Height - cardWidth) / 2f, cardHeight, cardWidth);
             * return handBounds;
             */

            const float edgeMargin = 20f;
            const float nameHeight = 28f;
            const float nameGap = 6f;

            return new[]
            {
                /* Bottom hand */
                new RectangleF(
                    _clientRectangle.Left + (_clientRectangle.Width - cardWidth * 2f) / 2f,
                    _clientRectangle.Bottom - cardHeight - 120f,
                    cardWidth * 2f,
                    cardHeight),

                /* Left hand */
                new RectangleF(
                    _clientRectangle.Left + edgeMargin,
                    _clientRectangle.Top + (_clientRectangle.Height - cardWidth) / 2f,
                    cardHeight,
                    cardWidth),

                /* Top hand */
                new RectangleF(
                    _clientRectangle.Left + (_clientRectangle.Width - cardWidth * 2f) / 2f,
                    _clientRectangle.Top + edgeMargin + nameHeight + nameGap,
                    cardWidth * 2f,
                    cardHeight),

                /* Right hand */
                new RectangleF(
                    _clientRectangle.Right - cardHeight - edgeMargin,
                    _clientRectangle.Top + (_clientRectangle.Height - cardWidth) / 2f,
                    cardHeight,
                    cardWidth)
            };
        }

        private static RectangleF[] GetPlayerNameBounds(IReadOnlyList<RectangleF> handBounds)
        {
            /*
             * OLD NAME-BOUND CALCULATIONS were inside DrawPlayerNames().
             * They are moved here so they run only when Rescale() is called.
             */
            const float gap = 6f;
            const float nameHeight = 28f;
            const float sideNameWidth = 245f;

            return new[]
            {
                /* Bottom name */
                new RectangleF(
                    _clientRectangle.Left + (_clientRectangle.Width - sideNameWidth) / 2f,
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
                    _clientRectangle.Left + (_clientRectangle.Width - sideNameWidth) / 2f,
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

        private static void DrawHoverCardHighlight(Graphics g, Card card)
        {
            /* This method is becoming a pain in my ass! */
            var r = card.Region;
            const float radius = 18f;
            const float diameter = radius * 2;

            var isLastCard = Math.Abs(r.Width - _cardWidth) < 0.1f;

            /* Use the full card width for ALL cards so they maintain their natural rounded rectangle shape.
             * Overlapping cards will naturally cover the rest of it. */
            var renderWidth = isLastCard ? r.Width : _cardWidth;

            using (var path = new GraphicsPath())
            {
                /* Top-left rounded corner */
                path.AddArc(r.X, r.Y, diameter, diameter, 180, 90);

                if (isLastCard)
                {
                    /* Top-right rounded corner */
                    path.AddArc(r.X + renderWidth - diameter, r.Y, diameter, diameter, 270, 90);

                    /* Bottom-right rounded corner */
                    path.AddArc(r.X + renderWidth - diameter, r.Bottom - diameter, diameter, diameter, 0, 90);
                }
                else
                {
                    /* Top-right rounded corner using full card width */
                    path.AddArc(r.X + renderWidth - diameter, r.Y, diameter, diameter, 270, 90);

                    /* Bottom-right rounded corner using full card width */
                    path.AddArc(r.X + renderWidth - diameter, r.Bottom - diameter, diameter, diameter, 0, 90);
                }

                /* Bottom-left rounded corner */
                path.AddArc(r.X, r.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                using (var brush = new SolidBrush(Color.FromArgb(80, Color.DeepSkyBlue)))
                {
                    g.FillPath(brush, path);
                }
            }
        }
        #endregion

        #region Player names
        private static void DrawPlayerNames(Game game, Graphics g, IReadOnlyList<RectangleF> nameBounds)
        {
            /*
             * OLD:
             * const float gap = 6f;
             * const float nameHeight = 28f;
             * const float sideNameWidth = 245f;
             * var nameBounds = new RectangleF[4];
             * // Each of the four RectangleF values was calculated here from handBounds.
             */

            if (nameBounds == null || nameBounds.Count < 4)
            {
                return;
            }

            var playerCount = Math.Min(game.GameData.Players.Count, 4);

            for (var i = 0; i < playerCount; i++)
            {
                var pos = playerCount == 2 && i == 1 ? 2 : i;
                g.DrawText(
                    game.GameData.Players[i].ToString(),
                    game.GameData.CurrentPlayer == game.GameData.Players[i] ? Color.DeepSkyBlue : TextColor,
                    nameBounds[pos],
                    playerCount > 2
                        ? i == 1 ? StringAlignment.Near
                        : i == 3 ? StringAlignment.Far
                        : StringAlignment.Center
                        : StringAlignment.Center);
            }
        }
        #endregion
        #endregion

        #region Card drawing methods
        private static void DrawCard(Graphics g, Image cardImage, PointF position)
        {
            /* This draws a card anywhere on screen (related to "position") scaled; and is kept track of on resize */
            g.DrawImage(cardImage, new RectangleF(_clientRectangle.Left + position.X, _clientRectangle.Top + position.Y, _cardWidth, _cardHeight));
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
