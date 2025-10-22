namespace FutMatchApp.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Court> Courts { get; set; } = new List<Court>();
        public List<Court> GoogleCourts { get; set; } = new List<Court>();
        public User? User { get; set; }
        public string? UserLocation { get; set; }

        public string? SelectedCidade { get; set; }
        public string? SelectedBairro { get; set; }
        public int? SelectedQuadraId { get; set; }
        public string? SelectedHorario { get; set; }

        public List<string> HorariosDisponiveis { get; set; } = new List<string>
        {
            "06:00", "07:00", "08:00", "09:00", "10:00", "11:00",
            "12:00", "13:00", "14:00", "15:00", "16:00", "17:00",
            "18:00", "19:00", "20:00", "21:00", "22:00"
        };
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }

        public List<DateTime> DatasDisponiveis { get; set; } = new();

        public HomeViewModel()
        {
            DatasDisponiveis = new List<DateTime>();
            for (int i = 0; i < 30; i++)
            {
                DatasDisponiveis.Add(DateTime.Now.Date.AddDays(i));
            }
            
            DataInicio = DateTime.Now.Date;
            DataFim = DateTime.Now.Date;
        }
    }
}