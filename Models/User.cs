using System.ComponentModel.DataAnnotations;

namespace FutMatchApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Senha { get; set; }

        public string? Telefone { get; set; }

        [StringLength(100)]
        public string? Cidade { get; set; }

        [StringLength(100)]
        public string? Estado { get; set; }

        [StringLength(10)]
        public string? CEP { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public int? SelectedTeamId { get; set; }
        public virtual Team? SelectedTeam { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
