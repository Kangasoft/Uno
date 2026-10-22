/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using ClassicUno.Classes.Helpers;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.UI.Controls;
using ClassicUno.Properties;

namespace ClassicUno.Classes.UI
{
    public class GraphicsRenderer : FeltForm
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
        public Game CurrentGame { get; set; }

        public GameLogRenderer LogRenderer { get; set; }

        /* UNO! card images - this separates current game play card data from the actual bitmaps. This makes it easier
         * to create a smaller "game-save" file without including the images in the binary data file. */
        private readonly Dictionary<CardKey, Bitmap> _cardImages = new Dictionary<CardKey, Bitmap>();

        private float _scale;
        private float _cardWidth;
        private float _cardHeight;

        /* Cached layout geometry. These rectangles/positions are calculated by Rescale()
         * and then reused by the paint methods. */
        private PointF _statusCenter;
        private RectangleF _directionBounds;
        private RectangleF _directionTextBounds;
        private RectangleF _orbBounds;
        private PointF _pickupPilePosition;
        private PointF _discardPilePosition;
        private RectangleF[] _playerHandBounds = new RectangleF[4];
        private RectangleF[] _playerNameBounds = new RectangleF[4];

        private UnoAnimation[] _unoAnimation = new UnoAnimation[4];
        private UnoAnimation _winner;

        private RectangleF[] _unoIndicatorBounds = new RectangleF[4];
        private bool _unoAnimationsInitialized;

        private readonly Color _textColor = Color.FromArgb(255, 225, 140);

        /* Back of the UNO! cards - this is displayed on the UI when cards are "face-down" */
        private const int MaxBackImages = 8;

        public int CardBackIndex;

        public List<Image> CardBackImages = new List<Image>();
        #endregion

        #region Initialization
        public GraphicsRenderer()
        {
            LogRenderer = new GameLogRenderer(_textColor);

            BuildCardImages();
            BuildCardBacks();
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_unoAnimation != null)
                {
                    foreach (var animation in _unoAnimation)
                    {
                        animation?.Dispose();
                    }

                    _unoAnimation = null;
                }

                if (_winner != null)
                {
                    _winner?.Dispose();
                    _winner = null;
                }

                LogRenderer.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Scaling
        protected override void OnResize(EventArgs e)
        {
            ReScale();

            if (LogRenderer != null)
            {
                LogRenderer.UpdateBounds(ClientRectangle);

                Invalidate(LogRenderer.Bounds);
            }

            if (!_unoAnimationsInitialized)
            {
                /* Set UNO animation class */
                for (var i = 0; i <= 3; i++)
                {
                    _unoAnimation[i] = new UnoAnimation(this, _unoIndicatorBounds[i], (Image)Resources.animatedUno.Clone())
                    {
                        Opacity = 0.8f
                    };
                }

                _unoAnimationsInitialized = true;
            }
            else
            {
                /* Set the new bounds to each UNO animation class */
                for (var i = 0; i <= 3; i++)
                {
                    _unoAnimation[i].Bounds = _unoIndicatorBounds[i];
                }
            }

            base.OnResize(e);
        }

        private void ReScale()
        {
            const float referenceWidth = 1600f;
            const float referenceHeight = 850f;

            _scale = Math.Min(ClientRectangle.Width / referenceWidth, ClientRectangle.Height / referenceHeight);

            _cardWidth = 110f * _scale;
            _cardHeight = 160f * _scale;

            /* Central status area shared by the direction indicator, orb and piles. */
            _statusCenter = new PointF(ClientRectangle.Width / 2f, (ClientRectangle.Height - _cardHeight * 2f) / 2f);

            var directionHeight = _cardHeight * (64f / 216f);
            var directionWidth = directionHeight * (Resources.clockwise.Width / (float)Resources.clockwise.Height);
            var directionY = _statusCenter.Y;

            _directionBounds = new RectangleF(_statusCenter.X - directionWidth / 2f, directionY, directionWidth, directionHeight);
            _directionTextBounds = new RectangleF(_statusCenter.X - 100f, directionY + directionHeight, 200f, 28f);

            var orbSize = _cardHeight * (64f / 216f);
            _orbBounds = new RectangleF(_statusCenter.X - orbSize / 2f, _statusCenter.Y + _cardHeight - orbSize, orbSize, orbSize);

            _pickupPilePosition = new PointF(_statusCenter.X - (_cardWidth + 20f) * 2f, _statusCenter.Y);
            _discardPilePosition = new PointF(_statusCenter.X + (_cardWidth - 20f) * 2f, _statusCenter.Y);

            _playerHandBounds = GetPlayerHandBounds(_cardWidth, _cardHeight);
            _playerNameBounds = GetPlayerNameBounds(_playerHandBounds);

            /* Calculate UNO animation bounds for each player's hand.
             * Player order: bottom, left, top, right. */
            _unoIndicatorBounds = new RectangleF[4];

            /* Scale the animation relative to the card dimensions. */
            var unoWidth = _cardWidth * 0.75f;
            var unoHeight = _cardHeight * (64f / 144f) * 0.75f;

            for (var i = 0; i <= 3; i++)
            {
                var hand = _playerHandBounds[i];

                float x;
                float y;

                switch (i)
                {
                    case 0: /* Bottom player */
                        x = hand.Left + (hand.Width - unoWidth) / 2f;
                        y = hand.Bottom - unoHeight * 0.65f - 12f;
                        break;

                    case 1: /* Left player - centre UNO over left-aligned name */
                        x = _playerNameBounds[1].Left;
                        y = _playerNameBounds[1].Top - unoHeight * _scale;
                        break;

                    case 2: /* Top player */
                        x = hand.Left + (hand.Width - unoWidth) / 2f;
                        y = hand.Bottom - unoHeight * 0.65f - 12f;
                        break;

                    default: /* Right player - centre UNO over right-aligned name */
                        x = _playerNameBounds[3].Right - unoWidth;
                        y = _playerNameBounds[3].Top - unoHeight * _scale;
                        break;
                }

                _unoIndicatorBounds[i] = new RectangleF(
                    (int)Math.Round(x),
                    (int)Math.Round(y),
                    (int)Math.Round(unoWidth),
                    (int)Math.Round(unoHeight));
            }
        }
        #endregion

        #region Public methods
        public void ResetAnimation()
        {
            foreach (var animation in _unoAnimation.Where(animation => animation.IsAnimating))
            {
                animation.Stop();
            }

            if (_winner?.IsAnimating != true)
            {
                return;
            }
            _winner.Stop();
            _winner.Dispose();
            _winner = null;
        }
        #endregion

        #region Drawing methods
        protected override void OnPaint(PaintEventArgs e)
        {
            /* Draw the current game in progress */
            if (CurrentGame == null)
            {
                base.OnPaint(e);
                return;
            }

            DrawGameCenter(e.Graphics);
            DrawPlayerCards(e.Graphics);
            DrawPlayerNames(e.Graphics, _playerNameBounds);

            LogRenderer?.Draw(e.Graphics);

            for (var i = 0; i <= 3; i++)
            {
                _unoAnimation[i].Draw(e.Graphics);
            }

            /* Draw winner GIF */
            _winner?.Draw(e.Graphics);

            if (CurrentGame.GameState != GameState.Paused)
            {
                base.OnPaint(e);
                return;
            }

            const string text = "GAME PAUSED";
            const int fontSize = 28;

            SizeF size;
            using (var font = new Font("Arial", fontSize, FontStyle.Bold))
            {
                size = e.Graphics.MeasureString(text, font);
            }

            var bounds = new RectangleF(_statusCenter.X - size.Width / 2f, ClientRectangle.Height / 2f + size.Height,
                size.Width, size.Height);
            e.Graphics.DrawText(text, _textColor, bounds, StringAlignment.Center, fontSize);

            base.OnPaint(e);
        }

        #region Game center
        private void DrawGameCenter(Graphics g)
        {
            /* This draws the central area of the game of the pick-up and discard piles, the current color orb as well as
             * the play direction indicator. */
            Image directionImage = CurrentGame.GameData.CurrentDirection == PlayDirection.Clockwise ? Resources.clockwise : Resources.anticlockwise;

            g.DrawImage(directionImage, _directionBounds);

            g.DrawText(Utils.GetDescriptionFromEnumValue(CurrentGame.GameData.CurrentDirection), _textColor, _directionTextBounds, StringAlignment.Center);

            DrawOrbColor(g);
            DrawPickupPile(g);
            DrawDiscardPile(g);
        }
        #endregion

        #region Pick-up pile
        private void DrawPickupPile(Graphics g)
        {
            /* Draw the pick-up pile - Get the center of the clientRectangle */
            var totalCards = CurrentGame.GameData.Deck.Count;

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
                DrawCard(g, CardBackImages[CardBackIndex], new PointF(_pickupPilePosition.X + offset, _pickupPilePosition.Y + offset));
                offset++;
            }

            /* Draw card count text under deck - not sure I like or want to keep this code... */
            g.DrawText($"{CurrentGame.GameData.Deck.Count} (108)", _textColor,
                new RectangleF(_pickupPilePosition.X - 50f, _pickupPilePosition.Y + _cardHeight + 6f, 200f, 28f),
                StringAlignment.Center);
        }
        #endregion

        #region Color orb
        private void DrawOrbColor(Graphics g)
        {
            /* Give the user a constant visual reminder of what the current card color is (this is useful if a computer player played
             * a wild or Wild Draw Four and they've momentarily looked away from the screen - and we don't end up in a situation of
             * "what's the color again?". */
            if (CurrentGame.GameData.CurrentColor == CardColor.None)
            {
                return;
            }

            Bitmap orb = null;

            switch (CurrentGame.GameData.CurrentColor)
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

            g.DrawImage(orb, _orbBounds);
        }
        #endregion

        #region Discard pile
        private void DrawDiscardPile(Graphics g)
        {
            /* Obvious what this is - we draw the throw-out card pile, but we make it look a little more
             * realistic as a "pile" of messy cards. */
            var count = CurrentGame.GameData.DiscardPile.Count;

            if (count == 0)
            {
                return;
            }

            /* Draw a few offset cards to simulate a stack.
             * Draw up to three slightly rotated card backs underneath top card. */
            Card card;

            if (count > 1)
            {
                var indices = count <= 3 ? Enumerable.Range(0, count - 1).ToArray() : new[] { 0, count - 3, count - 2 };

                for (var i = 0; i < indices.Length; i++)
                {
                    var cardIndex = indices[i];
                    var offset = indices.Length - i;

                    var x = ClientRectangle.Left + _discardPilePosition.X - offset * 2 + _cardWidth / 2f;
                    var y = ClientRectangle.Top + _discardPilePosition.Y - offset * 2 + _cardHeight / 2f;

                    card = CurrentGame.GameData.DiscardPile[cardIndex];

                    DrawRotatedCard(g, CardBackImages[CardBackIndex], x, y, card.DrawAngle);
                }
            }

            /* Draw the face-up top card, slightly crooked. */
            var topCentreX = ClientRectangle.Left + _discardPilePosition.X + _cardWidth / 2f;
            var topCentreY = ClientRectangle.Top + _discardPilePosition.Y + _cardHeight / 2f;

            card = CurrentGame.GameData.DiscardPile[CurrentGame.GameData.DiscardPile.Count - 1];
            DrawRotatedCard(g, GetCardImage(card), topCentreX, topCentreY, card.DrawAngle);
        }
        #endregion

        #region Player card drawing
        private void DrawPlayerCards(Graphics g)
        {
            /* Clockwise: bottom, left, top, right. */
            var angle = 0;
            var count = CurrentGame.GameData.Players.Count;
            for (var playerIndex = 0; playerIndex < count; playerIndex++)
            {
                DrawPlayerHand(CurrentGame.GameData.Players[playerIndex], g, angle, count == 2 && playerIndex == 1 ? 2 : playerIndex);

                /* If player count is greater than 2, we add 90 degrees; otherwise we just display the players
                 * top and bottom of the client rectangle. */
                angle += CurrentGame.GameData.Players.Count > 2 ? 90 : 180;
            }
        }

        private void DrawPlayerHand(IPlayer player, Graphics g, int angle, int playerIndex)
        {
            /* Draw a player's card hand */
            if (playerIndex < 0 || playerIndex >= _playerHandBounds.Length)
            {
                return;
            }

            var count = player.Cards.Count;
            if (count == 0)
            {
                if (_unoAnimation[playerIndex].IsAnimating)
                {
                    _unoAnimation[playerIndex].Stop();
                }

                if (CurrentGame.GameState == GameState.Dealing )
                {
                    return;
                }

                if (_winner != null)
                {
                    /* If we don't do this check, it will try to continously create the class on each invalidate - hanging the application */
                    return;
                }

                /* Show who the winner is */
                _winner = new UnoAnimation(this, _unoIndicatorBounds[playerIndex], (Image) Resources.winner.Clone())
                {
                    Opacity = 1f
                };
                _winner.Start();

                return;
            }

            var sideways = angle == 90 || angle == 270;
            var handBounds = _playerHandBounds[playerIndex];

            var spacing = sideways ? _cardHeight * 0.17f : _cardWidth * 0.30f;
            var available = sideways ? ClientRectangle.Height - 40f : ClientRectangle.Width - 40f;

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
                Image image = player is HumanPlayer ? cardImage :
                    CurrentGame.GameState != GameState.Stopped ? CardBackImages[CardBackIndex] : cardImage;

                var selectedOffsetY = card == CurrentGame.SelectedCard ? 23f : 0f;

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

                    DrawCard(g, image, new PointF(x - ClientRectangle.Left, y - ClientRectangle.Top));

                    if (player is HumanPlayer &&
                        CurrentGame.HighlightedCard != null &&
                        CurrentGame.HighlightedCard != CurrentGame.SelectedCard &&
                        CurrentGame.HighlightedCard == card)
                    {
                        DrawHoverCardHighlight(g, CurrentGame.HighlightedCard);
                    }
                }
                else
                {
                    x = handBounds.Left + handBounds.Width / 2f;
                    y = handBounds.Top + (handBounds.Height - totalHeight) / 2f + i * spacing + _cardHeight / 2f -
                        _cardWidth / 2f;

                    DrawRotatedCard(g, image, x, y, angle);
                }

                if (player is HumanPlayer)
                {
                    /* Set the region of this card for mouse hit test */
                    card.Region = new RectangleF(x, y, i < count - 1 ? spacing : _cardWidth, _cardHeight);
                }

                /* UNO! animation */
                if (count == 1 && CurrentGame.GameState != GameState.Dealing)
                {
                    _unoAnimation[playerIndex].Start();
                }
                else
                {
                    if (_unoAnimation[playerIndex] == null)
                    {
                        continue;
                    }
                    if (_unoAnimation[playerIndex].IsAnimating)
                    {
                        _unoAnimation[playerIndex].Stop();
                    }
                }
            }
        }

        private void DrawHoverCardHighlight(Graphics g, Card card)
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
        private void DrawPlayerNames(Graphics g, IReadOnlyList<RectangleF> nameBounds)
        {
            /* Draw each of the player's names on screen just under (or next to) their cards */
            if (nameBounds == null || nameBounds.Count < 4)
            {
                return;
            }

            var playerCount = Math.Min(CurrentGame.GameData.Players.Count, 4);

            for (var i = 0; i < playerCount; i++)
            {
                var pos = playerCount == 2 && i == 1 ? 2 : i;
                g.DrawText(
                    CurrentGame.GameData.Players[i].ToString(),
                    CurrentGame.GameData.CurrentPlayer == CurrentGame.GameData.Players[i] ? Color.DeepSkyBlue : _textColor,
                    nameBounds[pos],
                    playerCount > 2
                        ? i == 1 ? StringAlignment.Near
                        : i == 3 ? StringAlignment.Far
                        : StringAlignment.Center
                        : StringAlignment.Center);
            }
        }
        #endregion

        #region Card drawing methods
        private void DrawCard(Graphics g, Image cardImage, PointF position)
        {
            /* This draws a card anywhere on screen (related to "position") scaled; and is kept track of on resize */
            g.DrawImage(cardImage, new RectangleF(ClientRectangle.Left + position.X, ClientRectangle.Top + position.Y, _cardWidth, _cardHeight));
        }

        private void DrawRotatedCard(Graphics g, Image cardImage, float centerX, float centerY, float angle)
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
        #endregion

        #region Bounds helpers
        private RectangleF[] GetPlayerHandBounds(float cardWidth, float cardHeight)
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

        private RectangleF[] GetPlayerNameBounds(IReadOnlyList<RectangleF> handBounds)
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
        #endregion

        #region Card image helpers
        #region Card image building
        private void BuildCardImages()
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

        private void BuildCardBacks()
        {
            var image = Resources.cardBacks;
            var startX = 0;
            for (var i = 0; i < MaxBackImages; i++)
            {
                var rect = new Rectangle(startX, 0, 144, 216);
                var cardImg = image.Clone(rect, image.PixelFormat);
                CardBackImages.Add(cardImg);
                startX += 144;
            }
        }
        #endregion

        private Bitmap GetCardImage(Card card)
        {
            /* This returns the bitmap image related to the UNO! card */
            var key = new CardKey(card.Color, card.Type, card.Value);
            return _cardImages.TryGetValue(key, out var image) ? image : null;
        }

        private void AddCardImage(CardColor color, CardType type, int value, Bitmap image)
        {
            /* Create a new dictionary entry in CardImages based on parameters */
            var key = new CardKey(color, type, value);
            _cardImages.Add(key, image);
        }
        #endregion
    }
}
