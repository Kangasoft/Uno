/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.Threading.Tasks;
using ClassicUno.Classes.Logic;
using ClassicUno.Classes.Logic.Player;
using ClassicUno.Classes.Shared.Management;

namespace ClassicUno.Classes.UI
{
    public interface IGame
    {
        /* "Contract" interface between Game.cs and UI */
        Task AnimateCardToHandAsync(IPlayer player, Card card, bool faceUp = false, int duration = 300);

        Task AnimateCardToDiscardAsync(IPlayer player, Card card, int duration = 300);

        Task AnimateStartingDiscardAsync(Card card, int duration = 300);

        Task AnimateDeckStackAsync(int duration = 900);

        void SoundEffectRequest(SoundType type);

        void VoiceRequest(VoiceType type, IPlayer player);

        void ShowButtons(bool visible);

        void EnableDraw(bool enabled);

        void EnablePass(bool enabled);

        void EnablePlay(bool enabled);

        void ShowColorButtons(bool visible);

        void UpdateStatus(string text);

        void UpdateLogText(string text);

        void GameStateChanged();

        void GameOver(IPlayer player, int score);

        void UpdateUndo();

        void Invalidate();
    }
}
