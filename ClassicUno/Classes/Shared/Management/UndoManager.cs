/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using ClassicUno.Classes.Logic;

namespace ClassicUno.Classes.Shared.Management
{
    public class UndoManager
    {
        private GameData _snapshot;

        public bool CanUndo => _snapshot != null;

        public void SaveState(GameData data)
        {
            _snapshot = (GameData)data.Clone();
        }

        public GameData Undo()
        {
            if (_snapshot == null)
            {
                return null;
            }

            var snapshot = _snapshot;
            _snapshot = null;

            return (GameData)snapshot.Clone();
        }

        public void Clear()
        {
            _snapshot = null;
        }
    }
}
