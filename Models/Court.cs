using System.ComponentModel.DataAnnotations;

namespace FutMatchApp.Models
{
    public class Court
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        public string? Descricao { get; set; }

        [Required]
        public string Localizacao { get; set; }

        public decimal PrecoPorHora { get; set; }

        public bool Ativa { get; set; } = true;

        public string? GooglePlaceId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public decimal? GoogleRating { get; set; }
        public bool IsFromGoogle { get; set; } = false;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}

