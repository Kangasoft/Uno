/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.Settings.SettingsData;

namespace ClassicUno.Classes.Helpers.Management
{
    [Serializable]
    public class GameStateData
    {
        public List<Card> Deck { get; set; }

        public List<Card> DiscardPile { get; set; }

        public List<PlayerState> Players { get; set; }

        public CardColor CurrentColor { get; set; }

        public int CurrentPlayerIndex { get; set; }

        public PlayDirection CurrentDirection { get; set; }

        public int CurrentTime { get; set; }
    }

    [Serializable]
    public class PlayerState
    {
        public string PlayerType { get; set; }

        public PlayerNameData NameData { get; set; }

        public int VoiceIndex { get; set; }

        public int AudioPan { get; set; }

        public List<Card> Cards { get; set; }

        public bool IsSkipped { get; set; }

        public bool HasPassed { get; set; }

        public bool HasDrawn { get; set; }

        public bool HasRequestedDraw { get; set; }
    }

    public static class GameStateManager
    {
        public static GameStateData CreateGameState(GameData data, int currentTime)
        {
            return new GameStateData
            {
                Deck = data.Deck.Select(c => (Card) c.Clone()).ToList(),
                DiscardPile = data.DiscardPile.Select(c => (Card) c.Clone()).ToList(),

                Players = data.Players.Select(p => new PlayerState
                {
                    PlayerType = p.GetType().Name,
                    NameData = p.NameData,
                    VoiceIndex = p.VoiceIndex,
                    AudioPan = p.AudioPan,
                    Cards = p.Cards.Select(c => (Card) c.Clone()).ToList(),
                    IsSkipped = p.IsSkipped,
                    HasPassed = p.HasPassed,
                    HasDrawn = p.HasDrawn
                }).ToList(),

                CurrentColor = data.CurrentColor,
                CurrentPlayerIndex = data.Players.IndexOf(data.CurrentPlayer),
                CurrentDirection = data.CurrentDirection,
                CurrentTime = currentTime
            };
        }

        public static GameData RestoreGameState(GameStateData state)
        {
            var players = state?.Players.Select(RestorePlayer).ToList();

            if (players?.Any(p => p == null) != false)
            {
                return null;
            }

            var data = new GameData
            {
                Deck = state.Deck.Select(c => (Card) c.Clone()).ToList(),
                DiscardPile = state.DiscardPile.Select(c => (Card) c.Clone()).ToList(),

                Players = players,

                CurrentColor = state.CurrentColor,
                CurrentDirection = state.CurrentDirection
            };

            if (state.CurrentPlayerIndex < 0 || state.CurrentPlayerIndex >= data.Players.Count)
            {
                return null;
            }

            data.CurrentPlayer = data.Players[state.CurrentPlayerIndex];

            return data;
        }

        private static IPlayer RestorePlayer(PlayerState state)
        {
            IPlayer player;

            switch (state.PlayerType)
            {
                case nameof(ComputerPlayer):
                    player = new ComputerPlayer();
                    break;

                case nameof(HumanPlayer):
                    player = new HumanPlayer();
                    break;

                default:
                    return null;
            }

            player.NameData = state.NameData;
            player.VoiceIndex = state.VoiceIndex;
            player.AudioPan = state.AudioPan;
            player.Cards = state.Cards.Select(c => (Card)c.Clone()).ToList();
            player.IsSkipped = state.IsSkipped;
            player.HasPassed = state.HasPassed;
            player.HasDrawn = state.HasDrawn;

            return player;
        }
    }
}