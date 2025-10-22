namespace FutMatchApp.Models.ViewModels
{
    public class AvailableReservationsViewModel
    {
        public IEnumerable<Reservation> Reservations { get; set; } = new List<Reservation>();
        public List<Court> Courts { get; set; } = new List<Court>();
        public List<string> Locations { get; set; } = new List<string>();
        public List<string> TimeSlots { get; set; } = new List<string>();

        public string? SelectedLocation { get; set; }
        public int? SelectedCourtId { get; set; }
        public string? SelectedTimeSlot { get; set; }
        public DateTime? SelectedDate { get; set; }
    }
}
