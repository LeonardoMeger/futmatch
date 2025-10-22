namespace FutMatchApp.Models.ViewModels
{
    public class MatchStatsViewModel
    {
        public int TotalMatches { get; set; }
        public int Victories { get; set; }
        public int Draws { get; set; }
        public int Defeats { get; set; }
        public int TotalGoalsScored { get; set; }
        public int TotalGoalsConceded { get; set; }
        public List<Reservation> RecentMatches { get; set; } = new List<Reservation>();

        public double WinRate => TotalMatches > 0 ? Math.Round((double)Victories / TotalMatches * 100, 1) : 0;
        public double GoalAverage => TotalMatches > 0 ? Math.Round((double)TotalGoalsScored / TotalMatches, 1) : 0;
        public int GoalDifference => TotalGoalsScored - TotalGoalsConceded;
    }
}
