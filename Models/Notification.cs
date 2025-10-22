using FutMatchApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FutMatchApp.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        [Required]
        public string Mensagem { get; set; }

        public TipoNotificacao Tipo { get; set; }

        public bool Lida { get; set; } = false;

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int? ReservationId { get; set; }
        public virtual Reservation? Reservation { get; set; }

        public string? DadosExtras { get; set; }

        public bool PermiteAcao { get; set; } = false;
        public string? AcaoPositiva { get; set; }    
        public string? AcaoNegativa { get; set; }     
        public string? UrlAcao { get; set; }   
    }
}
