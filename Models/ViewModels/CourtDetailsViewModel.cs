namespace FutMatchApp.Models.ViewModels
{
    public class CourtDetailsViewModel
    {
        public Court Court { get; set; }
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();
        public List<string> TimeSlots { get; set; } = new List<string>();
        public Dictionary<DateTime, TimeSpan> ExistingReservations { get; set; } = new Dictionary<DateTime, TimeSpan>();
        public Team UserTeam { get; set; }
    }
}
