/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using ClassicUno.Classes.Helpers.Management;
using ClassicUno.Classes.Logic.Player;

namespace ClassicUno.Classes.Helpers.UI
{
    public interface IGame
    {
        /* "Contract" interface between Game.cs and UI */
        void SoundEffectRequest(SoundType type);

        void VoiceRequest(VoiceType type, IPlayer player);

        void ShowButtons(bool visible);

        void EnableDraw(bool enabled);

        void EnablePass(bool enabled);

        void ShowColorButtons(bool visible);

        void UpdateStatus(string text);

        void GameOver(IPlayer player, int score);

        void Invalidate();
    }
}
