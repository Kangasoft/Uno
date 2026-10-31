/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.Settings.SettingsData;
using ClassicUno.Classes.Shared;
using ClassicUno.Classes.Shared.Management;
using ClassicUno.Classes.UI;

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
            var players = Players.Select(p => p.Clone()).ToList();
            var currentPlayerIndex = Players.IndexOf(CurrentPlayer);

            return new GameData
            {
                Players = players,
                Deck = Deck.Select(c => (Card)c.Clone()).ToList(),
                DiscardPile = DiscardPile.Select(c => (Card)c.Clone()).ToList(),
                CurrentColor = CurrentColor,
                CurrentPlayer = currentPlayerIndex >= 0
                    ? players[currentPlayerIndex]
                    : null,
                CurrentDirection = CurrentDirection
            };
        }
    }
    #endregion

    public class Game : IDisposable
    {
        #region Fields
        private readonly IGame _ui;

        private bool _undoStateSavedThisTurn;

        private bool _waitToStart;

        private GameData _restart;

        public GameData GameData;

        public UndoManager Undo = new UndoManager();

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

        public void UndoLastMove()
        {
            if (!(GameData.CurrentPlayer is HumanPlayer))
            {
                return;
            }

            if (!Undo.CanUndo)
            {
                return;
            }

            var restoredState = Undo.Undo();

            if (restoredState == null)
            {
                return;
            }

            RemovePlayerCallbacks();
            GameData = restoredState;
            AddPlayerCallbacks();

            _undoStateSavedThisTurn = false;

            HighlightedCard = null;
            SelectedCard = null;

            _ui.ShowColorButtons(false);
            _ui.ShowButtons(true);
            _ui.EnableDraw(!GameData.CurrentPlayer.HasDrawn);
            _ui.EnablePass(GameData.CurrentPlayer.HasDrawn);
            _ui.EnablePlay(false);
            _ui.Invalidate();

            _ui.UpdateUndo();
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

        private void PlayerChoosesColor(IPlayer player, CardColor color)
        {
            /* Change color */
            GameData.CurrentColor = color;

            _ui.UpdateLogText($"{player} choses color {color}.");

            _ui.Invalidate();

            if (player is HumanPlayer)
            {
                player.EndTurn();
            }
        }

        public void SaveUndoState()
        {
            if (_undoStateSavedThisTurn)
            {
                return;
            }

            Undo.SaveState(GameData);
            _undoStateSavedThisTurn = true;
        }
        #endregion

        #region IPlayer callbacks
        #region Draws
        private async void PlayerDraw(IPlayer player)
        {
            var c = Draw();

            await _ui.AnimateCardToHandAsync(player, c, player is HumanPlayer);

            player.CardDrawn(c);

            if (player is HumanPlayer)
            {
                _ui.EnableDraw(false);
                _ui.EnablePass(true);
            }

            _ui.UpdateLogText($"{player} draws a card.");
            _ui.Invalidate();
            _ui.UpdateUndo();
        }
        #endregion

        #region Plays card
        private async void PlayerPlaysCard(IPlayer player, Card card)
        {
            /* Here is the logic for most of the game play. Player chooses a card in their hand, it goes to the discard pile and card is evaluated here
             * to determine coarse of action. */

            /* Set the angle for drawing on the discard pile. */
            card.DrawAngle = (float)(RandomEx.Shared.NextDouble() * 10.0 - 5.0);

            /* Animate the played card from the player's hand to the discard pile. */
            await _ui.AnimateCardToDiscardAsync(player, card);

            /* Now add the card to the discard pile. */
            GameData.CurrentColor = card.Color;
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

            _ui.UpdateLogText(GetGameText(player, nextPlayer, card));
            
            switch (card.Type)
            {
                case CardType.Wild:
                    if (player is HumanPlayer)
                    {
                        /* Human played the card */
                        _ui.EnableDraw(false);
                        _ui.EnablePass(false);
                        _ui.EnablePlay(false);
                        _ui.ShowColorButtons(true);
                        return;
                    }

                    /* Computer player chooses a colour */
                    player.ChooseColor(CardColor.None);
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
                    await BeginForceDraw(player, nextPlayer, 2, false);
                    return;

                case CardType.DrawFour:
                    nextPlayer.IsSkipped = true;
                    await BeginForceDraw(player, nextPlayer, 4, player is HumanPlayer);

                    if (player is HumanPlayer)
                    {
                        return;
                    }
                    
                    /* Computer player chooses a colour */
                    player.ChooseColor(CardColor.None);
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

            _ui.UpdateLogText($"{player} passes.");
        }
        #endregion

        #region Begin turn
        private async void PlayerBeginTurn(IPlayer player)
        {
            /* Loop back mainly for the human player so the UI can be update by showing the "DRAW" and "PASS" buttons */
            _ui.ShowColorButtons(false);
            _ui.Invalidate();

            _ui.UpdateUndo();

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

                _ui.UpdateStatus("Round started!");
                _ui.UpdateLogText($"Game starts with {player}.");

                var nextPlayer = GetNextPlayer(player, GameData.CurrentDirection);

                switch (c.Type)
                {
                    case CardType.Skip:
                        /* You are skipped */
                        _ui.UpdateLogText($"{player} is skipped. Play continues with {nextPlayer}");
                        PlayerEndTurn(player);
                        return;

                    case CardType.DrawTwo:
                        /* You have to draw two cards - and are skipped */
                        await BeginForceDraw(player, player, 2, false);
                        _ui.UpdateLogText($"{player} draws two cards and is skipped. Play continues with {nextPlayer}");
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
            _ui.UpdateStatus(string.Empty);

            if (player is HumanPlayer)
            {
                _undoStateSavedThisTurn = false;

                _ui.UpdateUndo();

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

                _ui.UpdateLogText($"{player} wins!");

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
            var human = new HumanPlayer
            {
                NameData = data.NameData,
                VoiceIndex = data.NameData.SelectVoice ? data.NameData.VoiceIndex : AudioManager.GetRandomVoice(data.NameData.Gender),
                AudioPan = 50
            };
            GameData.Players.Add(human);
            
            /* Now we add the remaining computer AI players */
            int[] panValues = { 45, 52, 65 };
            for (var players = 0; players <= data.NumberOfPlayers - 2; players++)
            {
                var name = ComputerNames.GetRandomName();
                var player = new ComputerPlayer
                {
                    NameData = name,
                    VoiceIndex = AudioManager.GetRandomVoice(name.Gender),
                    Difficulty = data.Difficulty,
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
        private async void BeginDeal()
        {
            GameState = GameState.Dealing;
            _ui.GameStateChanged();

            Card card;

            _ui.UpdateStatus("Dealing new round!");

            /* Allow the initial screen to paint before dealing starts. */
            await Task.Delay(2000);

            /* Deal seven cards to each player, one at a time. */
            for (var dealt = 0; dealt < 7; dealt++)
            {
                foreach (var player in GameData.Players.TakeWhile(player => GameData.Deck.Count != 0))
                {
                    /* Take the next card from the deck, but don't add it
                     * to the player's hand until its animation finishes. */
                    card = GameData.Deck[0];

                    _ui.SoundEffectRequest(SoundType.Deal);

                    await _ui.AnimateCardToHandAsync(player, card, player is HumanPlayer);

                    /* Animation finished - now update the actual game data. */
                    GameData.Deck.RemoveAt(0);
                    player.Cards.Add(card);

                    _ui.Invalidate();
                }
            }

            /* Select the starting discard card. */
            var topCard = GameData.Deck[0];
            GameData.Deck.RemoveAt(0);

            /* Make sure the starting card is not a Wild Draw Four. */
            Card startingCard;

            if (topCard.Type == CardType.DrawFour)
            {
                startingCard = GameData.Deck.FirstOrDefault(c => c.Type != CardType.DrawFour);

                if (startingCard != null)
                {
                    GameData.Deck.Remove(startingCard);

                    /* Return the original card to the deck and reshuffle. */
                    GameData.Deck.Add(topCard);
                    GameData.Deck.Shuffle();
                }
                else
                {
                    startingCard = topCard;
                }
            }
            else
            {
                startingCard = topCard;
            }

            startingCard.DrawAngle = (float)(RandomEx.Shared.NextDouble() * 10.0 - 5.0);
            GameData.DiscardPile.Add(startingCard);

            await Task.Delay(300);

            /* Dealing has finished - begin the game. */
            _ui.SoundEffectRequest(SoundType.Drop);

            card = GameData.DiscardPile[GameData.DiscardPile.Count - 1];

            GameData.CurrentPlayer = GameData.Players[0];

            if (card.Type != CardType.Wild)
            {
                GameData.CurrentColor = card.Color;
            }

            /* Store a restart game point. */
            _restart = (GameData)GameData.Clone();

            GameState = GameState.Running;
            _ui.GameStateChanged();

            GameData.CurrentPlayer.BeginTurn(card, GameData.CurrentColor);
        }
        #endregion
        
        #region Force card drawing
        private async Task BeginForceDraw(IPlayer player, IPlayer nextPlayer, int cards, bool chooseColor)
        {
            for (var count = 0; count < cards; count++)
            {
                var card = Draw();

                await _ui.AnimateCardToHandAsync(nextPlayer, card, nextPlayer is HumanPlayer);

                nextPlayer.CardDrawn(card);
                _ui.Invalidate();
            }

            nextPlayer.HasDrawn = false; /* IMPORTANT! */

            if (!(nextPlayer is HumanPlayer))
            {
                if (chooseColor)
                {
                    _ui.EnableDraw(false);
                    _ui.EnablePass(false);
                    _ui.EnablePlay(false);
                    _ui.ShowColorButtons(true);
                    return;
                }
            }

            if (player is HumanPlayer)
            {
                player.EndTurn();
            }
        }
        #endregion

        #region Private helpers
        private void AddPlayerCallbacks()
        {
            /* Add callbacks */
            foreach (var p in GameData.Players)
            {
                p.PlayerDraw += PlayerDraw;
                p.PlayerPlaysCard += PlayerPlaysCard;
                p.PlayerChoosesColor += PlayerChoosesColor;
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
                p.PlayerPlaysCard -= PlayerPlaysCard;
                p.PlayerChoosesColor -= PlayerChoosesColor;
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

                _ui.SoundEffectRequest(SoundType.Shuffle);
                _ui.UpdateLogText("Deck re-shuffled!");
            }

            _ui.SoundEffectRequest(SoundType.Draw);
            _ui.Invalidate();

            var card = GameData.Deck[0];
            GameData.Deck.RemoveAt(0);
            return card;
        }

        private static string GetGameText(IPlayer player, IPlayer nextPlayer, Card card)
        {
            var sb = new StringBuilder();
            sb.Append($"{player} played a ");
            switch (card.Type)
            {
                case CardType.Wild:
                    sb.Append("Wild card");
                    break;

                case CardType.Skip:
                    sb.Append($"{Misc.GetDescriptionFromEnumValue(card.Color)} Skip. {nextPlayer} will be skipped.");
                    break;

                case CardType.Reverse:
                    sb.Append($"{Misc.GetDescriptionFromEnumValue(card.Color)} Reverse.");
                    break;

                case CardType.DrawTwo:
                    sb.Append($"{Misc.GetDescriptionFromEnumValue(card.Color)} Draw Two. {nextPlayer} draws two cards and will be skipped.");
                    break;

                case CardType.DrawFour:
                    sb.Append($"Wild Draw Four. {nextPlayer} draws four cards and will be skipped.");
                    break;

                default:
                    /* Numeric */
                    sb.Append($"{Misc.GetDescriptionFromEnumValue(card.Color)} {card.Value}.");
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
