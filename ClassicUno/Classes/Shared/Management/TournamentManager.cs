/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
namespace ClassicUno.Classes.Shared.Management
{
    public class TournamentManager
    {
        public bool IsActive { get; private set; }

        public int CurrentRound { get; private set; }

        public int TotalRounds { get; } = 3;

        public int TotalScore { get; private set; }

        public int Bonus { get; } = 300; /* 300 point bonus award for winning all 3 games */

        public void Start()
        {
            IsActive = true;
            CurrentRound = 1;
            TotalScore = 0;
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
        }

        public int GetPlayerCount()
        {
            return CurrentRound + 1;
        }

        public void RestoreState(bool isActive, int round, int score)
        {
            IsActive = isActive;
            CurrentRound = isActive ? round : 0;
            TotalScore = isActive ? score : 0;
        }
    }
}
