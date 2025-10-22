using System.ComponentModel.DataAnnotations;

namespace FutMatchApp.Models.ViewModels
{
    public class CreateTeamViewModel
    {
        [Required(ErrorMessage = "Nome do time é obrigatório")]
        [StringLength(50, ErrorMessage = "Nome deve ter no máximo 50 caracteres")]
        public string Nome { get; set; }

        [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
        public string? Descricao { get; set; }

        public IFormFile? FotoEscudo { get; set; }

        [Display(Name = "Cor Primária")]
        public string? CorPrimaria { get; set; } = "#007bff";

        [Display(Name = "Cor Secundária")]
        public string? CorSecundaria { get; set; } = "#ffffff";

        [Display(Name = "Idade Mínima")]
        [Range(16, 60, ErrorMessage = "Idade mínima deve ser entre 16 e 60 anos")]
        public int? IdadeMinima { get; set; }

        [Display(Name = "Idade Máxima")]
        [Range(16, 60, ErrorMessage = "Idade máxima deve ser entre 16 e 60 anos")]
        public int? IdadeMaxima { get; set; }
    }
}
