using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace FutMatchApp.Models.DTOs
{
    public class ResultadoPartidaRequest
    {
        [Required]
        [Range(0, 50, ErrorMessage = "Número de gols deve estar entre 0 e 50")]
        [SwaggerSchema("Quantidade de gols marcados pelo seu time")]
        public int MyGoals { get; set; }

        [Required]
        [Range(0, 50, ErrorMessage = "Número de gols deve estar entre 0 e 50")]
        [SwaggerSchema("Quantidade de gols marcados pelo adversário")]
        public int OpponentGoals { get; set; }
    }

}
