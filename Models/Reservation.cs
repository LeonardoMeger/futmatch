using FutMatchApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FutMatchApp.Models
{    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public DateTime DataHora { get; set; }

        [Required]
        public int DuracaoHoras { get; set; } = 1;

        public StatusReservation Status { get; set; } = StatusReservation.Pendente;

        public string? Observacoes { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int TeamId { get; set; }
        public virtual Team Team { get; set; }

        public int CourtId { get; set; }
        public virtual Court Court { get; set; }

        public int? OpponentTeamId { get; set; }
        public virtual Team? OpponentTeam { get; set; }

        public int? OpponentUserId { get; set; }
        public virtual User? OpponentUser { get; set; }
        public ResultadoPartida? ResultadoTime1 { get; set; }  
        public ResultadoPartida? ResultadoTime2 { get; set; }  
        public int? GolsTime1 { get; set; }
        public int? GolsTime2 { get; set; }
        public string? ObservacoesPartida { get; set; }
        public DateTime? DataResultado { get; set; }
        public bool ResultadoInformadoTime1 { get; set; } = false;
        public bool ResultadoInformadoTime2 { get; set; } = false;

        public DateTime? DataResultadoTime1 { get; set; }
        public DateTime? DataResultadoTime2 { get; set; }

        public int? GolsTime1_InformadoPeloTime1 { get; set; }
        public int? GolsTime2_InformadoPeloTime1 { get; set; }

        public int? GolsTime1_InformadoPeloTime2 { get; set; }
        public int? GolsTime2_InformadoPeloTime2 { get; set; }
        public int? GolsTime1Final { get; set; }
        public int? GolsTime2Final { get; set; }
        public ResultadoPartida? ResultadoTime1Final { get; set; }
        public ResultadoPartida? ResultadoTime2Final { get; set; }
        public StatusResultado StatusResultado { get; set; } = StatusResultado.Pendente;
        public DateTime? DataResultadoFinal { get; set; }
        public bool ResultadoInformado => ResultadoInformadoTime1 && ResultadoInformadoTime2;
        public bool ResultadoConfirmado => StatusResultado == StatusResultado.Confirmado;
        public bool TemConflito => StatusResultado == StatusResultado.Conflito;
    }
}
