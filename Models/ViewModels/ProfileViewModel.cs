namespace FutMatchApp.Models.ViewModels
{
    public class ProfileViewModel
    {
        public User User { get; set; }
        public List<Team> UserTeams { get; set; } = new List<Team>();
        public Team? SelectedTeam { get; set; }

        public List<Reservation> UpcomingGames { get; set; } = new List<Reservation>();
        public List<Reservation> PastGames { get; set; } = new List<Reservation>();
        public TeamStatsViewModel? TeamStats { get; set; }

        public int TotalGamesScheduled => UpcomingGames.Count + PastGames.Count;
        public int GamesAsChallenger => UpcomingGames.Count(g => g.UserId == User.Id) + PastGames.Count(g => g.UserId == User.Id);
        public int GamesAsOpponent => UpcomingGames.Count(g => g.OpponentUserId == User.Id) + PastGames.Count(g => g.OpponentUserId == User.Id);
    }
}
