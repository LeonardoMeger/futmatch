using FutMatchApp.Models.Enums;

namespace FutMatchApp.Models.ViewModels
{
    public class GoogleCourtDetailsViewModel
    {
        public Court Court { get; set; }
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();
        public List<string> TimeSlots { get; set; } = new List<string>();
        public Team UserTeam { get; set; }
        public int CurrentUserId { get; set; }

        public List<Reservation> AllReservations { get; set; } = new List<Reservation>();

        public List<Reservation> GetReservationsForDateTime(DateTime date, string time)
        {
            var timeSpan = TimeSpan.Parse(time);

            return AllReservations
                .Where(r => r.DataHora.Date == date.Date && r.DataHora.TimeOfDay == timeSpan)
                .ToList();
        }

        public bool IsSlotAvailable(DateTime date, string time)
        {
            var reservations = GetReservationsForDateTime(date, time);
            return reservations.Count < 2;
        }

        public bool CanUserChallenge(DateTime date, string time)
        {
            var reservations = GetReservationsForDateTime(date, time);
            if (reservations.Count != 1) return false;

            var reservation = reservations.First();
            return reservation.UserId != CurrentUserId &&
                   reservation.Status == StatusReservation.Pendente &&
                   reservation.OpponentTeamId == null;
        }
        public bool IsSlotFullyOccupied(DateTime date, string time)
        {
            var dateTime = date.Add(TimeSpan.Parse(time));
            var reservationsAtTime = AllReservations
                .Where(r => r.DataHora.Date == date.Date &&
                           r.DataHora.TimeOfDay == TimeSpan.Parse(time) &&
                           r.Status != StatusReservation.Cancelada)
                .ToList();

            return reservationsAtTime.Count >= 2 ||
                   reservationsAtTime.Any(r => r.Status == StatusReservation.Confirmada);
        }

        public bool IsSlotEmpty(DateTime date, string time)
        {
            var reservationsAtTime = GetReservationsForDateTime(date, time);
            return !reservationsAtTime.Any();
        }

        public bool IsSlotConfirmed(DateTime date, string time)
        {
            var reservationsAtTime = GetReservationsForDateTime(date, time);
            return reservationsAtTime.Any(r => r.Status == StatusReservation.Confirmada &&
                                              r.OpponentTeamId.HasValue);
        }
    }
}
