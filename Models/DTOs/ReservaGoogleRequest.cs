using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace FutMatchApp.Models.DTOs
{
    public class ReservaGoogleRequest
    {
        [Required(ErrorMessage = "O GooglePlaceId é obrigatório")]
        [SwaggerSchema("Identificador único da quadra no Google Places")]
        public string GooglePlaceId { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome da quadra é obrigatório")]
        [StringLength(200)]
        [SwaggerSchema("Nome completo da quadra de futebol")]
        public string CourtName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [SwaggerSchema("Endereço completo incluindo cidade e estado")]
        public string CourtLocation { get; set; } = string.Empty;

        [Required]
        [SwaggerSchema("Data e hora do início da reserva no formato ISO 8601")]
        public DateTime DataHora { get; set; }

        [Range(1, 8, ErrorMessage = "A duração deve ser entre 1 e 8 horas")]
        [SwaggerSchema("Quantidade de horas da reserva (mínimo 1, máximo 8)")]
        public int DuracaoHoras { get; set; } = 1;

        [StringLength(1000)]
        [SwaggerSchema("Comentários ou observações opcionais sobre a reserva")]
        public string? Observacoes { get; set; }
    }
}
