namespace FutMatchApp.Models.ViewModels
{
    public class TeamStatsViewModel
    {
        public int TotalGames { get; set; }
        public int Victories { get; set; }
        public int Defeats { get; set; }
        public int Draws { get; set; }
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }

        public double WinRate => TotalGames > 0 ? Math.Round((double)Victories / TotalGames * 100, 1) : 0;
        public int GoalDifference => GoalsScored - GoalsConceded;
        public double GoalAverage => TotalGames > 0 ? Math.Round((double)GoalsScored / TotalGames, 1) : 0;
        public string PerformanceStatus => WinRate >= 60 ? "Excelente" : WinRate >= 40 ? "Boa" : "Regular";
    }
}
