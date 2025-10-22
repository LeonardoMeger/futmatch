using System.ComponentModel.DataAnnotations;

namespace FutMatchApp.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; }

        public string? Descricao { get; set; }

        public string? FotoUrl { get; set; }

        [StringLength(20)]
        public string? CorPrimaria { get; set; }

        [StringLength(20)]
        public string? CorSecundaria { get; set; }

        [Range(16, 60)]
        public int? IdadeMinima { get; set; }

        [Range(16, 60)]
        public int? IdadeMaxima { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public string FaixaIdadeDisplay =>
            (IdadeMinima.HasValue || IdadeMaxima.HasValue)
                ? $"{IdadeMinima ?? 16} - {IdadeMaxima ?? 60} anos"
                : "Todas as idades";
    }
}
