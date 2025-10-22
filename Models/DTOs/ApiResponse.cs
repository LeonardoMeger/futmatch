using Swashbuckle.AspNetCore.Annotations;

namespace FutMatchApp.Models.DTOs
{
    public class ApiResponse
    {
        [SwaggerSchema("Status da operação (true = sucesso, false = erro)")]
        public bool Success { get; set; }

        [SwaggerSchema("Mensagem detalhada sobre o resultado da operação")]
        public string Message { get; set; } = string.Empty;

        [SwaggerSchema("Título da notificação ou alerta")]
        public string? Title { get; set; }

        [SwaggerSchema("Objeto com dados específicos da operação")]
        public object? Data { get; set; }
    }

}
