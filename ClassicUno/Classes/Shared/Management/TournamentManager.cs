/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
namespace ClassicUno.Classes.Shared.Management
{
    public class TournamentManager
    {
        private int _displayRound;

        public bool IsActive { get; private set; }

        public int CurrentRound { get; private set; }

        public int TotalRounds { get; } = 3;

        public int TotalScore { get; private set; }

        public int Bonus { get; } = 300; /* 300 point bonus award for winning all 3 games */

        public void Start()
        {
            IsActive = true;
            CurrentRound = 1;
            _displayRound = 1;
            TotalScore = 0;
        }

        public void BeginNextRound()
        {
            _displayRound = CurrentRound;
        }

        /* Records a completed game and updates tournament progress.
         * Returns true if the human player has won the entire tournament.
         * Returns false if the tournament continues or has been lost.
         * Check IsActive to distinguish those outcomes. */
        public bool RecordResult(bool humanWon, int score)
        {
            if (!IsActive)
            {
                return false;
            }

            if (!humanWon)
            {
                End();
                return false;
            }

            TotalScore += score;

            if (CurrentRound >= TotalRounds)
            {
                TotalScore += Bonus;
                IsActive = false;
                return true;
            }

            CurrentRound++;
            return false;
        }

        public void End()
        {
            IsActive = false;
            TotalScore = 0;
            CurrentRound = 0;
            _displayRound = 0;
        }

        public int GetPlayerCount()
        {
            return CurrentRound + 1;
        }

        public void RestoreState(bool isActive, int round, int score)
        {
            IsActive = isActive;
            CurrentRound = isActive ? round : 0;
            _displayRound = CurrentRound;
            TotalScore = isActive ? score : 0;
        }

        public override string ToString()
        {
            switch (_displayRound)
            {
                case 1:
                    return "First Round";

                case 2:
                    return "Second Round";

                case 3:
                    return "Final Round";
            }

            return string.Empty;
        }
    }
}
