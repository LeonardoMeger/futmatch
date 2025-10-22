using FutMatchApp.Models.Enums;

namespace FutMatchApp.Models.ViewModels
{
    public class ConfirmResultViewModel
    {
        public Reservation Reservation { get; set; }
        public bool IsTeam1 { get; set; }
        public int CurrentUserId { get; set; }

        public string MyTeamName => IsTeam1 ? Reservation.Team.Nome : Reservation.OpponentTeam?.Nome ?? "";
        public string OpponentTeamName => IsTeam1 ? Reservation.OpponentTeam?.Nome ?? "" : Reservation.Team.Nome;

        public bool HasMyResult => IsTeam1 ? Reservation.ResultadoTime1.HasValue : Reservation.ResultadoTime2.HasValue;
        public bool HasOpponentResult => IsTeam1 ? Reservation.ResultadoTime2.HasValue : Reservation.ResultadoTime1.HasValue;

        public int? MyGoals => IsTeam1 ? Reservation.GolsTime1 : Reservation.GolsTime2;
        public int? OpponentGoals => IsTeam1 ? Reservation.GolsTime2 : Reservation.GolsTime1;

        public ResultadoPartida? MyResult => IsTeam1 ? Reservation.ResultadoTime1 : Reservation.ResultadoTime2;
        public ResultadoPartida? OpponentResult => IsTeam1 ? Reservation.ResultadoTime2 : Reservation.ResultadoTime1;
    }
}
