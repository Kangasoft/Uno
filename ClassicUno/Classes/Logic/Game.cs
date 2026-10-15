/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using ClassicUno.Classes.Helpers;
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Classes.Helpers.UI;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Logic
{
    #region Enums
    public enum GameState
    {
        Stopped = 0,
        Dealing = 1,
        Running = 2,
        Paused = 3
    }

    [Serializable]
    public enum PlayDirection
    {
        [Description("Clockwise")]
        Clockwise = 0,

        [Description("Anti-Clockwise")]
        AntiClockwise = 1
    }
    #endregion

    #region Game play data class
    public class GameData : ICloneable
    {
        /* I separated this to its own class from the main class as to add a "restart" feature
         * at a later date. */
        public List<IPlayer> Players { get; set; }

        public List<Card> Deck { get; set; }

        public List<Card> DiscardPile { get; set; }

        public CardColor CurrentColor { get; set; }

        public IPlayer CurrentPlayer { get; set; }

        public PlayDirection CurrentDirection { get; set; }

        public object Clone()
        {
            return new GameData
            {
                Players = Players.Select(p => p.Clone()).ToList(),
                Deck = Deck.Select(c => (Card)c.Clone()).ToList(),
                DiscardPile = DiscardPile.Select(c => (Card)c.Clone()).ToList(),
                CurrentColor = CurrentColor,
                CurrentDirection = CurrentDirection
            };
        }
    }
    #endregion

    public class Game : IDisposable
    {
        #region Fields
        private readonly IGame _ui;

        private BackgroundWorker _worker;

        private bool _stop;

        private int _cardsToDraw;
        private IPlayer _playerToForceDraw;
        private IPlayer _playerWhoForced;
        private bool _forceDrawChooseColor;

        private bool _waitToStart;

        private string _gameText;

        private static readonly Random Rnd = new Random();

        private GameData _restart;

        public GameData GameData;

        public bool Restartable => _restart != null;

        public GameState GameState { get; set; }

        public Card HighlightedCard { get; set; }

        public Card SelectedCard { get; set; }

        public int CurrentTime { get; set; }
        #endregion

        #region Constructors

        public Game(IGame ui, GameStateData state)
        {
            /* Restore a previous game state */
            _ui = ui;

            GameData = GameStateManager.RestoreGameState(state);

            CurrentTime = state.CurrentTime;

            AddPlayerCallbacks();
        }

        public Game(IGame ui, GameOptionData data)
        {
            _ui = ui;

            GameData = new GameData();
            _waitToStart = true;

            GameData.Players = new List<IPlayer>();
            GameData.Deck = new List<Card>();
            GameData.DiscardPile = new List<Card>();

            GameData.CurrentDirection = PlayDirection.Clockwise;

            BuildDeck();
            CreatePlayers(data);
            BeginDeal();
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            /* If a new game is requested while this one is still dealing - we need to stop the dealing loop */
            if (_worker?.IsBusy == true)
            {
                _stop = true;
                _worker.CancelAsync();
            }

            _worker?.Dispose();

            /* Remove handlers */
            RemovePlayerCallbacks();
        }
        #endregion

        #region Public methods

        public void Start()
        {
            var card = GameData.DiscardPile[GameData.DiscardPile.Count - 1];

            if (card.Type != CardType.Wild)
            {
                GameData.CurrentColor = card.Color;
            }

            GameData.CurrentPlayer.BeginTurn(card, GameData.CurrentColor);
            GameState = GameState.Running;
        }

        public void Restart()
        {
            if (_restart == null)
            {
                return;
            }
            RemovePlayerCallbacks();
            GameData = (GameData) _restart.Clone();
            AddPlayerCallbacks();
            
            CurrentTime = 0;
            _waitToStart = true;
            GameData.CurrentPlayer = GameData.Players[0];

            Start();
        }

        public void HumanDrawCard()
        {
            if (!(GameData.CurrentPlayer is HumanPlayer))
            {
                /* Not your turn */
                return;
            }

            var c = Draw();
            GameData.CurrentPlayer.DrawCard(c);
            _ui.EnableDraw(false);
            _ui.EnablePass(true);
            _ui.Invalidate();
        }

        public void HumanPass()
        {
            if (!(GameData.CurrentPlayer is HumanPlayer))
            {
                /* Not your turn */
                return;
            }
            GameData.CurrentPlayer.Pass();
            GameData.CurrentPlayer.EndTurn();
        }

        public void HumanPlaysCard()
        {
            if (!(GameData.CurrentPlayer is HumanPlayer))
            {
                /* Not your turn */
                return;
            }
            /* Now, we evaluate the selected card can even be played */
            if (Validation.IsCardPlayable(GameData.DiscardPile[GameData.DiscardPile.Count - 1], SelectedCard, GameData.CurrentColor, GameData.CurrentPlayer.Cards))
            {
                /* Play the card */
                GameData.CurrentPlayer.PlayCard(SelectedCard);
            }
            else
            {
                /* Cannot play card */
                _ui.SoundEffectRequest(SoundType.Error);

                _gameText = "Selected card cannot be played. Choose another, or draw.";
                _ui.UpdateStatus(_gameText);
            }
        }

        public void PlayerChoosesColor(IPlayer player, CardColor color)
        {
            /* Change color */
            GameData.CurrentColor = color;
            _ui.UpdateStatus(_gameText);
            _ui.Invalidate();

            if (player is HumanPlayer)
            {
                player.EndTurn();
            }
        }
        #endregion

        #region IPlayer callbacks
        #region Draws
        private void PlayerDraw(IPlayer player)
        {
            var c = Draw();
            player.DrawCard(c);

            _gameText = $"{player} draws a card.";
            _ui.UpdateStatus(_gameText);
        }
        #endregion

        #region Plays card
        private void PlayerPlaysCard(IPlayer player, Card card)
        {
            /* Here is the logic for most of the game play. Player chooses a card in their hand, it goes to the discard pile and card is evaluated here
             * to determine coarse of action. */
            GameData.CurrentColor = card.Color;

            /* Set the angle for drawing on the discard pile */
            card.DrawAngle = (float)(Rnd.NextDouble() * 10.0 - 5.0);
            GameData.DiscardPile.Add(card);

            _ui.SoundEffectRequest(SoundType.Drop);
            _ui.Invalidate();

            /* Does player have UNO!? */
            if (player.Cards.Count == 1)
            {
                /* Raise audio request */
                _ui.VoiceRequest(VoiceType.Uno, player);
            }

            var nextPlayer = GetNextPlayer(player, GameData.CurrentDirection);

            _gameText = GetGameText(player, nextPlayer, card);
            _ui.UpdateStatus(_gameText);

            switch (card.Type)
            {
                case CardType.Wild:
                    if (GameData.CurrentPlayer is HumanPlayer)
                    {
                        /* Human played the card */
                        _ui.EnableDraw(false);
                        _ui.EnablePass(false);
                        _ui.EnablePlay(false);
                        _ui.ShowColorButtons(true);
                        return;
                    }
                    break;

                case CardType.Skip:
                    /* Next player in line is skipped */
                    nextPlayer.IsSkipped = true;
                    break;

                case CardType.Reverse:
                    /* Change direction */
                    GameData.CurrentDirection = GameData.CurrentDirection == PlayDirection.Clockwise
                        ? PlayDirection.AntiClockwise
                        : PlayDirection.Clockwise;
                    break;

                case CardType.DrawTwo:
                    nextPlayer.IsSkipped = true;
                    BeginForceDraw(player, nextPlayer, 2, false);
                    return;

                case CardType.DrawFour:
                    nextPlayer.IsSkipped = true;
                    BeginForceDraw(player, nextPlayer, 4, player is HumanPlayer);
                    return;
            }

            if (player is HumanPlayer)
            {
                player.EndTurn();
            }
        }
        #endregion

        #region Passes
        private void PlayerPasses(IPlayer player)
        {
            /* Raise audio request */
            _ui.VoiceRequest(VoiceType.Pass, player);

            _gameText = $"{player} passes.";
            _ui.UpdateStatus(_gameText);
        }
        #endregion

        #region Begin turn

        private void PlayerBeginTurn(IPlayer player)
        {
            /* Loop back mainly for the human player so the UI can be update by showing the "DRAW" and "PASS" buttons */
            _ui.ShowColorButtons(false);
            _ui.Invalidate();
            if (!(player is HumanPlayer))
            {
                return;
            }

            /* Evaluted top card on discard pile */
            var c = GameData.DiscardPile[GameData.DiscardPile.Count - 1];
            if (_waitToStart)
            {
                _waitToStart = false;
                GameData.CurrentColor = c.Color;

                _gameText = "Round started!";
                _ui.UpdateStatus(_gameText);

                switch (c.Type)
                {
                    case CardType.Skip:
                        /* You are skipped */
                        PlayerEndTurn(player);
                        return;

                    case CardType.DrawTwo:
                        /* You have to draw two cards - and are skipped */
                        BeginForceDraw(player, player, 2, false);
                        return;

                    case CardType.Reverse:
                        /* Change direction */
                        GameData.CurrentDirection = GameData.CurrentDirection == PlayDirection.Clockwise
                            ? PlayDirection.AntiClockwise
                            : PlayDirection.Clockwise;
                        break;
                }
            }

            if (player.IsSkipped)
            {
                return;
            }

            _ui.ShowButtons(true);

            var hasDrawn = player.HasDrawn;
            _ui.EnableDraw(!hasDrawn);
            _ui.EnablePass(hasDrawn);

            if (GameData.CurrentColor == CardColor.None && (c.Type == CardType.Wild || c.Type == CardType.DrawFour))
            {
                /* Human needs to choose a color */
                _ui.ShowColorButtons(true);
            }
        }
        #endregion

        #region End turn
        private void PlayerEndTurn(IPlayer player)
        {
            /* Play moves to next player - dependant on direction */
            _ui.UpdateStatus(_gameText);
            if (player is HumanPlayer)
            {
                _ui.ShowButtons(false);
                _ui.EnableDraw(false);
                _ui.EnablePass(false);
                _ui.EnablePlay(false);
                _ui.ShowColorButtons(false);
                HighlightedCard = null;
                SelectedCard = null;
            }

            if (player.Cards.Count == 0)
            {
                /* Check if they have no cards, then they've won */
                GameState = GameState.Stopped;
                _ui.GameStateChanged();

                _gameText = $"Game over! {player} won!";
                _ui.UpdateStatus(_gameText);

                _ui.GameOver(player, CalculateRemainingScore());
                return;
            }

            var card = GameData.DiscardPile[GameData.DiscardPile.Count - 1];
            /* If there are only two players, and last played card is a reverse, this acts as a skip as well - stay on same player */
            if (GameData.Players.Count == 2 && card.Type == CardType.Reverse && !player.HasPassed)
            {
                /* Added "HasPassed", or else this gets stuck in a loop (on passing) until the computer draws a card it can use... */
                player.BeginTurn(card, GameData.CurrentColor);
                _ui.Invalidate();
                return;
            }

            GameData.CurrentPlayer = GetNextPlayer(player, GameData.CurrentDirection);
            GameData.CurrentPlayer.BeginTurn(card, GameData.CurrentColor);

            _ui.Invalidate();
        }
        #endregion
        #endregion

        #region Game setup
        #region Build pick-up pile
        private void BuildDeck()
        {
            /* Build the deck of 108 cards (four wilds/four wild draw four's) */
            for (var count = 0; count <= 1; count++)
            {
                /* There's two sets of each color numbers 1-9 */
                for (var color = 0; color <= 3; color++)
                {
                    /* Colors red, yellow, green, blue */
                    for (var value = 0; value <= 13; value++)
                    {
                        Card card = null;
                        switch (value)
                        {
                            case 10:
                                /* Skip */
                                card = new Card
                                {
                                    Color = (CardColor) color + 1,
                                    Type = CardType.Skip
                                };
                                break;

                            case 11:
                                /* Reverse */
                                card = new Card
                                {
                                    Color = (CardColor) color + 1,
                                    Type = CardType.Reverse
                                };
                                break;

                            case 12:
                                /* Draw two */
                                card = new Card
                                {
                                    Color = (CardColor) color + 1,
                                    Type = CardType.DrawTwo
                                };
                                break;

                            case 13:
                                /* Wild / Wild Draw Four - simplified the code from how I had it.
                                 * Originially, this was in a separate loop executed after this one.
                                 * Streamlining it in to this loop == less CPU time and makes more sense. */
                                if (color < 2 && count == 0)
                                {
                                    /* Add the four wilds/four wild draw four's */
                                    for (var wild = 0; wild <= 3; wild++)
                                    {
                                        card = color == 0
                                            ? new Card {Color = CardColor.None, Type = CardType.Wild}
                                            : new Card {Color = CardColor.None, Type = CardType.DrawFour};
                                        GameData.Deck.Add(card);
                                    }
                                    continue;
                                }
                                break;

                            default:
                                /* Numeric 0 - 9 */
                                if (value == 0 && count == 1)
                                {
                                    /* We do not add more than one 0 of each color (four total) */
                                    continue;
                                }
                                card = new Card
                                {
                                    Color = (CardColor) color + 1,
                                    Type = CardType.Numeric,
                                    Value = value
                                };
                                break;
                        }
                        /* Add card to deck */
                        if (card != null)
                        {
                            GameData.Deck.Add(card);
                        }
                    }
                }
            }
            /* Shuffle the deck - sanity check it is greater than 0, which it should be... */
            if (GameData.Deck.Count > 0)
            {
                GameData.Deck.Shuffle();
            }
        }
        #endregion

        #region Create player array
        private void CreatePlayers(GameOptionData data)
        {
            /* Add players: min 2 max 4. First add the human player */
            GameData.Players.Add(new HumanPlayer
            {
                NameData = data.NameData, VoiceIndex = AudioManager.GetRandomVoice(data.NameData.Gender), AudioPan = 50
            });

            /* Now we add the remaining computer AI players */
            int[] panValues = { 45, 52, 65 };
            for (var players = 0; players <= data.NumberOfPlayers - 2; players++)
            {
                var name = ComputerNames.GetRandomName();
                var player = new ComputerPlayer
                {
                    NameData = name,
                    VoiceIndex = AudioManager.GetRandomVoice(name.Gender),
                    AudioPan = panValues[players] /* Give the player's voice a slight pan left or right depending on position */
                };
                GameData.Players.Add(player);
            }

            /* Add callbacks */
            AddPlayerCallbacks();
        }
        #endregion
        #endregion

        #region Deal
        private void BeginDeal()
        {
            GameState = GameState.Dealing;
            _ui.GameStateChanged();

            _worker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _worker.DoWork += DealWorkerCallback;
            _worker.RunWorkerCompleted += DealEnded;
            _worker.RunWorkerAsync();
        }

        private void DealWorkerCallback(object sender, DoWorkEventArgs e)
        {
            /* Give a slight delay before beginning dealing. This gives time for the WM_PAINT event to update the screen with the pile of pick-up cards,
             * player names, etc. */
            _gameText = "Dealing new round!";
            _ui.UpdateStatus(_gameText);
            Thread.Sleep(2000);
            /* We loop 7 times (7 cards per hand) */
            for (var dealt = 0; dealt <= 6; dealt++)
            {
                /* During each iteration we loop each player and add a card to their hand */
                if (_stop)
                {
                    return;
                }
                foreach (var p in GameData.Players)
                {
                    if (_stop)
                    {
                        return;
                    }
                    var card = GameData.Deck[0];
                    p.Cards.Add(card);
                    GameData.Deck.RemoveAt(0);
                    /* Play sound and update UI */
                    _ui.SoundEffectRequest(SoundType.Deal);
                    _ui.Invalidate();
                    /* Slight delay to allow WM_PAINT and sound event to complete before next iteration */
                    Thread.Sleep(300);
                }
            }
            /* Top card */
            var topCard = GameData.Deck[0];
            GameData.Deck.RemoveAt(0);
            /* We make sure the top card is not a draw four */
            Card startingCard;
            if (topCard.Type == CardType.DrawFour)
            {
                /* Use LINQ to find the first card that is NOT a Wild Draw Four */
                startingCard = GameData.Deck.FirstOrDefault(c => c.Type != CardType.DrawFour);
                if (startingCard != null)
                {
                    /* Remove that specific card from the deck directly */
                    GameData.Deck.Remove(startingCard);
                    /* Put the original card back at the top */
                    GameData.Deck.Add(topCard);
                    /* Reshuffle */
                    GameData.Deck.Shuffle();
                }
                else
                {
                    /* Fallback safety case if every card in the deck is a Wild Draw Four */
                    startingCard = topCard;
                }
            }
            else
            {
                /* The top card wasn't a Wild Draw Four, so use it directly */
                startingCard = topCard;
            }

            startingCard.DrawAngle = (float)(Rnd.NextDouble() * 10.0 - 5.0);
            GameData.DiscardPile.Add(startingCard);

            Thread.Sleep(300);
        }

        private void DealEnded(object sender, RunWorkerCompletedEventArgs e)
        {
            /* Dealing ended - game came begin */
            _ui.SoundEffectRequest(SoundType.Drop);

            _worker.DoWork -= DealWorkerCallback;
            _worker.RunWorkerCompleted -= DealEnded;
            _worker.Dispose();
            _worker = null;

            /*At some point I may add a bit of code to include who the dealer is and who the next player is; for now, we'll start with the human player */
            var card = GameData.DiscardPile[GameData.DiscardPile.Count - 1];
            GameData.CurrentPlayer = GameData.Players[0];

            if (card.Type != CardType.Wild)
            {
                GameData.CurrentColor = card.Color;
            }

            /* Store a restart game point */
            _restart = (GameData)GameData.Clone();

            GameState = GameState.Running;
            _ui.GameStateChanged();

            GameData.CurrentPlayer.BeginTurn(card, GameData.CurrentColor);
        }
        #endregion

        #region Force card drawing
        /* These methods are just so the UI can "slowly" show each card being drawn, rather than just "appearing" out of nowhere. */
        private void BeginForceDraw(IPlayer player, IPlayer nextPlayer, int cards, bool chooseColor)
        {
            _playerWhoForced = player;
            _playerToForceDraw = nextPlayer;
            _cardsToDraw = cards;
            _forceDrawChooseColor = chooseColor;
            
            _worker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _worker.DoWork += BackgroundForceDraw;
            _worker.RunWorkerCompleted += BackgroundForceDrawEnd;
            _worker.RunWorkerAsync();
        }

        private void BackgroundForceDraw(object sender, DoWorkEventArgs e)
        {
            for (var count = 0; count < _cardsToDraw; count++)
            {
                var card = Draw();
                _playerToForceDraw.DrawCard(card);
                Thread.Sleep(300);
            }
        }

        private void BackgroundForceDrawEnd(object sender, RunWorkerCompletedEventArgs e)
        {
            _worker.DoWork -= BackgroundForceDraw;
            _worker.RunWorkerCompleted -= BackgroundForceDrawEnd;
            _worker.Dispose();
            _worker = null;

            if (!(_playerToForceDraw is HumanPlayer))
            {
                if (_forceDrawChooseColor)
                {
                    _ui.EnableDraw(false);
                    _ui.EnablePass(false);
                    _ui.EnablePlay(false);
                    _ui.ShowColorButtons(true);
                    return;
                }
            }

            _playerToForceDraw = null;
            _cardsToDraw = 0;

            if (_playerWhoForced is HumanPlayer)
            {
                _playerWhoForced.EndTurn();
            }

            _playerWhoForced = null;
        }
        #endregion

        #region Private helpers
        private void AddPlayerCallbacks()
        {
            /* Add callbacks */
            foreach (var p in GameData.Players)
            {
                p.PlayerDraw += PlayerDraw;
                p.PlayerChoosesColor += PlayerChoosesColor;
                p.PlayerPlaysCard += PlayerPlaysCard;
                p.PlayerPass += PlayerPasses;
                p.PlayerBeginTurn += PlayerBeginTurn;
                p.PlayerEndTurn += PlayerEndTurn;
            }
        }

        private void RemovePlayerCallbacks()
        {
            /* Add callbacks */
            foreach (var p in GameData.Players)
            {
                p.PlayerDraw -= PlayerDraw;
                p.PlayerChoosesColor -= PlayerChoosesColor;
                p.PlayerPlaysCard -= PlayerPlaysCard;
                p.PlayerPass -= PlayerPasses;
                p.PlayerBeginTurn -= PlayerBeginTurn;
                p.PlayerEndTurn -= PlayerEndTurn;
            }
        }

        private IPlayer GetNextPlayer(IPlayer currentPlayer, PlayDirection direction)
        {
            var index = GameData.Players.IndexOf(currentPlayer);
            return GameData.Players[(index + (direction == PlayDirection.Clockwise ? 1 : -1) + GameData.Players.Count) % GameData.Players.Count];
        }

        private Card Draw()
        {
            /* This method ensures during game play that once the deck get's down to one card, it is refreshed from the
             * discard pile and shuffled */
            if (GameData.Deck.Count == 1)
            {
                /* Keep the top card */
                var count = GameData.DiscardPile.Count - 1;
                var topCard = GameData.DiscardPile[count];
                GameData.DiscardPile.RemoveAt(count);

                /* Copy the rest of the discard pile*/
                GameData.Deck.AddRange(GameData.DiscardPile);
                GameData.DiscardPile.Clear();
                GameData.DiscardPile.Add(topCard);

                GameData.Deck.Shuffle();
            }

            _ui.SoundEffectRequest(SoundType.Draw);
            _ui.Invalidate();

            var card = GameData.Deck[0];
            GameData.Deck.RemoveAt(0);
            return card;
        }

        private string GetGameText(IPlayer player, IPlayer nextPlayer, Card card)
        {
            var sb = new StringBuilder();
            sb.Append($"{player} played a ");
            switch (card.Type)
            {
                case CardType.Wild:
                    sb.Append("Wild card");
                    break;

                case CardType.Skip:
                    sb.Append($"{Utils.GetDescriptionFromEnumValue(card.Color)} Skip. {nextPlayer} will be skipped.");
                    break;

                case CardType.Reverse:
                    sb.Append($"{Utils.GetDescriptionFromEnumValue(card.Color)} Reverse.");
                    break;

                case CardType.DrawTwo:
                    sb.Append($"{Utils.GetDescriptionFromEnumValue(card.Color)} Draw Two. {nextPlayer} draws two cards and will be skipped.");
                    break;

                case CardType.DrawFour:
                    sb.Append($"Wild Draw Four. {nextPlayer} draws four cards and will be skipped.");
                    break;

                default:
                    /* Numeric */
                    sb.Append($"{Utils.GetDescriptionFromEnumValue(card.Color)} {card.Value}.");
                    break;
            }

            if (player.Cards.Count == 1)
            {
                sb.Append($" {player} has UNO!");
            }

            return sb.ToString();
        }

        private int CalculateRemainingScore()
        {
            var totalScore = 0;

            foreach (var card in GameData.Players.Where(player => player.Cards != null).SelectMany(player => player.Cards.Where(card => card != null)))
            {
                switch (card.Type)
                {
                    case CardType.Numeric:
                        totalScore += card.Value;
                        break;

                    case CardType.Skip:
                    case CardType.Reverse:
                    case CardType.DrawTwo:
                        totalScore += 20;
                        break;

                    case CardType.Wild:
                    case CardType.DrawFour:
                        totalScore += 50;
                        break;
                }
            }

            return totalScore;
        }
        #endregion
    }
}
