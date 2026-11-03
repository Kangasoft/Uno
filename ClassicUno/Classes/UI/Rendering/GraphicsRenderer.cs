/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.Shared;
using ClassicUno.Classes.Shared.Management;
using ClassicUno.Classes.UI.Imaging;
using ClassicUno.Properties;
using Timer = System.Windows.Forms.Timer; /* Only declared like this as System.Threading is declared above, giving a different meaning to "Timer" */

namespace ClassicUno.Classes.UI.Rendering
{
    public class GraphicsRenderer : CardRegions
    {
        /*
         * A programmer walks into a bar and orders 1.000 beers 🍺.
         * The bartender says, "Is that one beer or a thousand?"
         * The programmer replies, "Yes." 😄 🍺
         */
        #region Pick-up pile animation
        /* Array class for pick-up pile shuffle animation thingamajig */
        private sealed class DeckCardAnimation
        {
            public PointF Start;
            public PointF End;
            public float StartAngle;
            public float EndAngle;
        }
        #endregion

        #region Fields
        /* Card image resources */
        public CardImages Cards = new CardImages();

        /* Tournament management class */
        protected readonly TournamentManager Tournament = new TournamentManager();

        public int CardBackIndex { get; set; }

        /* Game log text rendering */
        public GameLogRenderer LogRenderer { get; set; }

        /* UNO! and winner GIF animators */
        private UnoAnimation[] _unoAnimation = new UnoAnimation[4];
        private UnoAnimation _winner;

        private bool _unoAnimationsInitialized;

        private readonly Color _textColor = Color.FromArgb(255, 225, 140);

        /* Card animation */
        private readonly Timer _cardAnimationTimer = new Timer();
        private readonly Stopwatch _cardAnimationWatch = new Stopwatch();
        private readonly SemaphoreSlim _cardAnimationLock = new SemaphoreSlim(1, 1);

        private TaskCompletionSource<bool> _cardAnimationCompletion;

        private Image _animatedCardImage;
        private PointF _animationStart;
        private PointF _animationEnd;
        private PointF _animatedCardCenter;

        private float _animationStartAngle;
        private float _animationEndAngle;
        private float _animatedCardAngle;

        private bool _cardAnimationActive;
        private int _animationDuration;

        /* Pick-up pile animation */
        private readonly List<DeckCardAnimation> _deckAnimationCards = new List<DeckCardAnimation>();

        private readonly Timer _deckAnimationTimer = new Timer();
        private readonly Stopwatch _deckAnimationWatch = new Stopwatch();
        private readonly SemaphoreSlim _deckAnimationLock = new SemaphoreSlim(1, 1);

        private TaskCompletionSource<bool> _deckAnimationCompletion;

        private bool _deckAnimationActive;
        private int _deckAnimationDuration;
        private int _deckAnimationStagger;

        /* Cached static game background. */
        private Bitmap _backgroundBuffer;
        private bool _backgroundDirty = true;
        #endregion

        #region Constructor
        public GraphicsRenderer()
        {
            LogRenderer = new GameLogRenderer(_textColor);

            _cardAnimationTimer.Interval = 16;
            _cardAnimationTimer.Tick += CardAnimationTimer;

            _deckAnimationTimer.Interval = 16;
            _deckAnimationTimer.Tick += DeckAnimationTimer;
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

                if (_backgroundBuffer != null)
                {
                    _backgroundBuffer.Dispose();
                    _backgroundBuffer = null;
                }

                /* Card animation clean up */
                _cardAnimationTimer.Stop();
                _cardAnimationTimer.Tick -= CardAnimationTimer;
                _cardAnimationTimer.Dispose();

                _cardAnimationWatch.Stop();

                _cardAnimationCompletion?.TrySetResult(true);
                _cardAnimationCompletion = null;

                /* Pick-up pile animation clean up */
                _deckAnimationTimer.Stop();
                _deckAnimationTimer.Tick -= DeckAnimationTimer;
                _deckAnimationTimer.Dispose();

                _deckAnimationWatch.Stop();

                _deckAnimationCompletion?.TrySetResult(true);
                _deckAnimationCompletion = null;

                Cards?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Scaling
        protected override void OnResize(EventArgs e)
        {
            ReScale();
            InvalidateBackground();

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
                    _unoAnimation[i] = new UnoAnimation(this, UnoIndicatorBounds[i], (Image)Resources.animatedUno.Clone())
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
                    _unoAnimation[i].Bounds = UnoIndicatorBounds[i];
                }
            }

            base.OnResize(e);
        }
        #endregion

        #region Background buffer
        /*
         * Call this after changing anything represented in the static background:
         * player hands, selected/highlighted card, player turn, deck/discard piles,
         * current colour/direction, or game state.
         *
         * Animation timer callbacks should continue to call Invalidate() only.
         */
        public void InvalidateBackground()
        {
            _backgroundDirty = true;
            Invalidate();
        }

        private void EnsureBackgroundBuffer()
        {
            var width = ClientRectangle.Width;
            var height = ClientRectangle.Height;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            if (_backgroundBuffer == null || _backgroundBuffer.Width != width || _backgroundBuffer.Height != height)
            {
                _backgroundBuffer?.Dispose();

                _backgroundBuffer = new Bitmap(width, height);
                _backgroundDirty = true;
            }

            if (!_backgroundDirty)
            {
                return;
            }

            using (var g = Graphics.FromImage(_backgroundBuffer))
            {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                DrawTournamentInfo(g);

                DrawGameCenter(g);
                DrawPlayerCards(g);
                DrawPlayerNames(g, PlayerNameBounds);
            }

            _backgroundDirty = false;
        }
        #endregion

        #region Animation methods
        public async Task AnimateCardToHandAsync(IPlayer player, Card card, bool faceUp = false, int duration = 300)
        {
            if (CurrentGame == null || player == null)
            {
                return;
            }

            var destination = GetPlayerCardDestination(player, out var angle);

            var start = GetPickupPileCenter();

            var cardImage = faceUp && card != null ? Cards.GetCardImage(card) : Cards.CardBackImages[CardBackIndex];

            await AnimateCardAsync(cardImage, start, destination, 0f, angle, duration);

            /* The card has reached the player's hand, so refresh the background buffer. */
            InvalidateBackground();
        }

        public async Task AnimateCardToDiscardAsync(IPlayer player, Card card, int duration = 300)
        {
            if (CurrentGame == null || player == null || card == null)
            {
                return;
            }

            /* Find the player's position around the table. */
            var playerIndex = CurrentGame.GameData.Players.IndexOf(player);

            if (playerIndex < 0 || playerIndex >= 4)
            {
                return;
            }

            /* Determine the player's card rotation. */
            var startAngle = CurrentGame.GameData.Players.Count > 2
                ? playerIndex * 90f
                : playerIndex * 180f;

            /* Get the centre of the card's last known hand position. */
            var start = new PointF(
                card.Region.Left + card.Region.Width / 2f,
                card.Region.Top + card.Region.Height / 2f);

            /* Centre of the discard pile. */
            var end = new PointF(
                ClientRectangle.Left + DiscardPilePosition.X + CardWidth / 2f,
                ClientRectangle.Top + DiscardPilePosition.Y + CardHeight / 2f);

            /* Refresh the background buffer, so the card appears to be removed from the player's hand. */
            InvalidateBackground();

            /* Animate the actual face-up card. */
            await AnimateCardAsync(Cards.GetCardImage(card), start, end, startAngle, card.DrawAngle, duration);

            /* The card has reached the discard pile, so refresh the background buffer. */
            InvalidateBackground();
        }

        public async Task AnimateStartingDiscardAsync(Card card, int duration = 500)
        {
            if (card == null)
            {
                return;
            }

            var start = new PointF(
                ClientRectangle.Left + PickupPilePosition.X + CardWidth / 2f,
                ClientRectangle.Top + PickupPilePosition.Y + CardHeight / 2f);

            var end = new PointF(
                ClientRectangle.Left + DiscardPilePosition.X + CardWidth / 2f,
                ClientRectangle.Top + DiscardPilePosition.Y + CardHeight / 2f);

            await AnimateCardAsync(Cards.GetCardImage(card), start, end, 0f, card.DrawAngle, duration);

            /* The card has reached the discard pile, so refresh the background buffer. */
            InvalidateBackground();
        }

        public async Task AnimateDeckStackAsync(int duration = 500)
        {
            await _deckAnimationLock.WaitAsync();

            try
            {
                _deckAnimationCards.Clear();

                var centerX = PickupPilePosition.X + CardWidth / 2f;
                var centerY = PickupPilePosition.Y + CardHeight / 2f;

                const int cardCount = 10;

                _deckAnimationDuration = Math.Max(1, duration);
                _deckAnimationStagger = 45;

                for (var i = 0; i < cardCount; i++)
                {
                    var finalOffset = cardCount - i - 1;

                    _deckAnimationCards.Add(new DeckCardAnimation
                    {
                        Start = new PointF(
                            centerX + RandomEx.Shared.Next(-35, 36),
                            centerY + RandomEx.Shared.Next(-35, 36)),

                        End = new PointF(
                            centerX + finalOffset,
                            centerY + finalOffset),

                        StartAngle = RandomEx.Shared.Next(-15, 16),
                        EndAngle = 0f
                    });
                }

                _deckAnimationCompletion = new TaskCompletionSource<bool>();
                _deckAnimationActive = true;

                _deckAnimationWatch.Restart();
                _deckAnimationTimer.Start();

                Invalidate();

                await _deckAnimationCompletion.Task;
            }
            finally
            {
                _deckAnimationLock.Release();
            }
        }

        public void ResetAnimation()
        {
            /* UNO GIF reset */
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

            EnsureBackgroundBuffer();

            if (_backgroundBuffer != null)
            {
                e.Graphics.DrawImageUnscaled(_backgroundBuffer, ClientRectangle.Left, ClientRectangle.Top);
            }

            /* Dynamic foreground animations are drawn over the cached background. */
            if (_deckAnimationActive)
            {
                DrawDeckStackAnimation(e.Graphics);
            }

            DrawAnimatedCard(e.Graphics);

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

            var bounds = new RectangleF(StatusCenter.X - size.Width / 2f, ClientRectangle.Height / 2f + size.Height, size.Width, size.Height);

            e.Graphics.DrawText(text, _textColor, bounds, StringAlignment.Center, fontSize);

            base.OnPaint(e);
        }

        #region Tournament information
        private void DrawTournamentInfo(Graphics graphics)
        {
            if (!Tournament.IsActive)
            {
                return;
            }

            const int iconSize = 48;
            const int padding = 10;
            const int topPadding = 18;
            const int textWidth = 160;

            /* Draw tournament icon. */
            var iconBounds = new Rectangle(padding, topPadding + padding, iconSize, iconSize);

            graphics.DrawImage(Resources.tournamentIcon, iconBounds);

            /* Draw round title alongside the icon. */
            var textBounds = new RectangleF(iconBounds.Right + padding, iconBounds.Y, textWidth, iconSize);

            graphics.DrawText(Tournament.ToString(), _textColor, textBounds, StringAlignment.Near);
        }
        #endregion

        #region Game center
        private void DrawGameCenter(Graphics g)
        {
            /* This draws the central area of the game of the pick-up and discard piles, the current color orb as well as
             * the play direction indicator. */
            var game = CurrentGame.GameData;
            Image directionImage = game.CurrentDirection == PlayDirection.Clockwise ? Resources.clockwise : Resources.anticlockwise;

            g.DrawImage(directionImage, DirectionBounds);

            g.DrawText(Misc.GetDescriptionFromEnumValue(game.CurrentDirection), _textColor, DirectionTextBounds, StringAlignment.Center);

            DrawOrbColor(g);
            DrawPickupPile(g);
            DrawDiscardPile(g);
        }
        #endregion

        #region Pick-up pile
        private void DrawPickupPile(Graphics g)
        {
            /* Draw the pick-up pile - Get the center of the clientRectangle */
            var game = CurrentGame.GameData;
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
            var offset = 0;
            for (var i = 0; i < visualLayers; i++)
            {
                DrawCard(g, Cards.CardBackImages[CardBackIndex], new PointF(PickupPilePosition.X + offset, PickupPilePosition.Y + offset));
                offset++;
            }

            /* Draw card count text under deck - not sure I like or want to keep this code... */
            g.DrawText($"{game.Deck.Count} (108)", _textColor,
                new RectangleF(PickupPilePosition.X - 50f, PickupPilePosition.Y + CardHeight + 6f, 200f, 28f),
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

            g.DrawImage(orb, OrbBounds);
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

                    var x = ClientRectangle.Left + DiscardPilePosition.X - offset * 2 + CardWidth / 2f;
                    var y = ClientRectangle.Top + DiscardPilePosition.Y - offset * 2 + CardHeight / 2f;

                    card = CurrentGame.GameData.DiscardPile[cardIndex];

                    DrawRotatedCard(g, Resources.blankCard, x, y, card.DrawAngle);
                }
            }

            /* Draw the face-up top card, slightly crooked. */
            var topCentreX = ClientRectangle.Left + DiscardPilePosition.X + CardWidth / 2f;
            var topCentreY = ClientRectangle.Top + DiscardPilePosition.Y + CardHeight / 2f;

            card = CurrentGame.GameData.DiscardPile[CurrentGame.GameData.DiscardPile.Count - 1];
            DrawRotatedCard(g, Cards.GetCardImage(card), topCentreX, topCentreY, card.DrawAngle);
        }
        #endregion

        #region Player card drawing
        private void DrawPlayerCards(Graphics g)
        {
            /* Clockwise: bottom, left, top, right. */
            var angle = 0;
            var game = CurrentGame.GameData;
            var count = game.Players.Count;
            for (var playerIndex = 0; playerIndex < count; playerIndex++)
            {
                DrawPlayerHand(game.Players[playerIndex], g, angle, count == 2 && playerIndex == 1 ? 2 : playerIndex);

                /* If player count is greater than 2, we add 90 degrees; otherwise we just display the players
                 * top and bottom of the client rectangle. */
                angle += game.Players.Count > 2 ? 90 : 180;
            }
        }

        private void DrawPlayerHand(IPlayer player, Graphics g, int angle, int playerIndex)
        {
            /* Draw a player's card hand */
            if (playerIndex < 0 || playerIndex >= PlayerHandBounds.Length)
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

                if (CurrentGame.GameState == GameState.Dealing)
                {
                    return;
                }

                if (_winner != null)
                {
                    /* If we don't do this check, it will try to continously create the class on each invalidate - hanging the application */
                    return;
                }

                /* Show who the winner is */
                _winner = new UnoAnimation(this, UnoIndicatorBounds[playerIndex], (Image)Resources.winner.Clone())
                {
                    Opacity = 1f
                };
                _winner.Start();

                return;
            }

            var sideways = angle == 90 || angle == 270;
            var handBounds = PlayerHandBounds[playerIndex];

            var spacing = sideways ? CardHeight * 0.17f : CardWidth * 0.30f;
            var available = sideways ? ClientRectangle.Height - 40f : ClientRectangle.Width - 40f;

            if (count > 1)
            {
                spacing = Math.Min(spacing, (available - CardWidth) / (count - 1));
            }

            spacing = Math.Max(0f, spacing);

            var totalWidth = CardWidth + (count - 1) * spacing;
            var totalHeight = CardHeight + (count - 1) * spacing;

            for (var i = 0; i < count; i++)
            {
                var card = player.Cards[i];
                var cardImage = Cards.GetCardImage(card);

                var image = player is HumanPlayer ? cardImage :
                    CurrentGame.GameState != GameState.Stopped ? Cards.CardBackImages[CardBackIndex] : cardImage;

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
                    y = handBounds.Top + (handBounds.Height - totalHeight) / 2f + i * spacing + CardHeight / 2f -
                        CardWidth / 2f;

                    DrawRotatedCard(g, image, x, y, angle);
                }

                /* Store the card's screen position for hit testing and animation. */
                if (!sideways)
                {
                    card.Region = new RectangleF(
                        x,
                        y,
                        i < count - 1 ? spacing : CardWidth,
                        CardHeight);
                }
                else
                {
                    card.Region = new RectangleF(
                        x - CardHeight / 2f,
                        y - CardWidth / 2f,
                        CardHeight,
                        CardWidth);
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

            var isLastCard = Math.Abs(r.Width - CardWidth) < 0.1f;

            /* Use the full card width for ALL cards so they maintain their natural rounded rectangle shape.
             * Overlapping cards will naturally cover the rest of it. */
            var renderWidth = isLastCard ? r.Width : CardWidth;

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

            var game = CurrentGame.GameData;
            var playerCount = Math.Min(game.Players.Count, 4);

            for (var i = 0; i < playerCount; i++)
            {
                var pos = playerCount == 2 && i == 1 ? 2 : i;
                g.DrawText(
                    game.Players[i].ToString(),
                    game.CurrentPlayer == game.Players[i] ? Color.DeepSkyBlue : _textColor,
                    nameBounds[pos],
                    playerCount > 2
                        ? i == 1 ? StringAlignment.Near
                        : i == 3 ? StringAlignment.Far
                        : StringAlignment.Center
                        : StringAlignment.Center);
            }
        }
        #endregion

        #region Card animation
        private async Task AnimateCardAsync(Image image, PointF start, PointF end, float startAngle = 0f, float endAngle = 0f, int duration = 300)
        {
            await _cardAnimationLock.WaitAsync();

            try
            {
                _animationStart = start;
                _animationEnd = end;

                _animationStartAngle = startAngle;
                _animationEndAngle = endAngle;

                _animatedCardCenter = start;
                _animatedCardAngle = startAngle;

                _animatedCardImage = image;
                _animationDuration = Math.Max(1, duration);

                _cardAnimationCompletion = new TaskCompletionSource<bool>();

                _cardAnimationActive = true;

                _cardAnimationWatch.Restart();
                _cardAnimationTimer.Start();

                Invalidate();

                await _cardAnimationCompletion.Task;
            }
            finally
            {
                _cardAnimationLock.Release();
            }
        }

        private void DrawDeckStackAnimation(Graphics g)
        {
            if (!_deckAnimationActive)
            {
                return;
            }

            var elapsed = _deckAnimationWatch.ElapsedMilliseconds;

            for (var i = 0; i < _deckAnimationCards.Count; i++)
            {
                var card = _deckAnimationCards[i];

                var progress = Math.Max(0f, Math.Min(1f,
                    (float)(elapsed - i * _deckAnimationStagger) / _deckAnimationDuration));

                /* Smooth start and finish. */
                var eased = progress * progress * (3f - 2f * progress);

                var x = card.Start.X + (card.End.X - card.Start.X) * eased;
                var y = card.Start.Y + (card.End.Y - card.Start.Y) * eased;

                var angle = card.StartAngle +
                            (card.EndAngle - card.StartAngle) * eased;

                DrawRotatedCard(g, Cards.CardBackImages[CardBackIndex], x, y, angle);
            }
        }
        #endregion

        #region Card drawing methods
        private void DrawCard(Graphics g, Image cardImage, PointF position)
        {
            /* This draws a card anywhere on screen (related to "position") scaled; and is kept track of on resize */
            g.DrawImage(cardImage, new RectangleF(ClientRectangle.Left + position.X, ClientRectangle.Top + position.Y, CardWidth, CardHeight));
        }

        private void DrawAnimatedCard(Graphics g)
        {
            if (!_cardAnimationActive || _animatedCardImage == null)
            {
                return;
            }

            DrawRotatedCard(g, _animatedCardImage, _animatedCardCenter.X, _animatedCardCenter.Y, _animatedCardAngle);
        }

        private void DrawRotatedCard(Graphics g, Image cardImage, float centerX, float centerY, float angle)
        {
            var state = g.Save();

            try
            {
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(angle);
                g.DrawImage(cardImage, new RectangleF(-CardWidth / 2f, -CardHeight / 2f, CardWidth, CardHeight));
            }
            finally
            {
                g.Restore(state);
            }
        }
        #endregion
        #endregion

        #region Card animation timer callbacks
        private void CardAnimationTimer(object sender, EventArgs e)
        {
            var elapsed = _cardAnimationWatch.ElapsedMilliseconds;

            var progress = Math.Min(1f, (float)elapsed / _animationDuration);

            /* Smooth start and finish. */
            var eased = progress * progress * (3f - 2f * progress);

            _animatedCardCenter = new PointF(
                _animationStart.X + (_animationEnd.X - _animationStart.X) * eased,
                _animationStart.Y + (_animationEnd.Y - _animationStart.Y) * eased);

            _animatedCardAngle =
                _animationStartAngle +
                (_animationEndAngle - _animationStartAngle) * eased;

            Invalidate();

            if (progress < 1f)
            {
                return;
            }
            _cardAnimationTimer.Stop();
            _cardAnimationWatch.Stop();

            _cardAnimationActive = false;
            _animatedCardImage = null;

            var completion = _cardAnimationCompletion;
            _cardAnimationCompletion = null;

            completion?.TrySetResult(true);
        }

        private void DeckAnimationTimer(object sender, EventArgs e)
        {
            var elapsed = _deckAnimationWatch.ElapsedMilliseconds;

            var totalDuration = _deckAnimationDuration +
                                (_deckAnimationCards.Count - 1) * _deckAnimationStagger;

            Invalidate();

            if (elapsed < totalDuration)
            {
                return;
            }

            _deckAnimationTimer.Stop();
            _deckAnimationWatch.Stop();

            _deckAnimationActive = false;
            _deckAnimationCards.Clear();

            var completion = _deckAnimationCompletion;
            _deckAnimationCompletion = null;

            completion?.TrySetResult(true);
        }
        #endregion
    }
}
