using System.ComponentModel.DataAnnotations;

namespace FutMatchApp.Models.ViewModels
{
    public class ReservationViewModel
    {
        [Required(ErrorMessage = "Selecione uma quadra")]
        public int CourtId { get; set; }

        [Required(ErrorMessage = "Selecione data e hora")]
        public DateTime DataHora { get; set; }

        [Required(ErrorMessage = "Selecione a duração")]
        [Range(1, 8, ErrorMessage = "Duração deve ser entre 1 e 8 horas")]
        public int DuracaoHoras { get; set; } = 1;

        public string? Observacoes { get; set; }

        public List<Court> AvailableCourts { get; set; } = new List<Court>();
        public List<Team> UserTeams { get; set; } = new List<Team>();
    }
}
