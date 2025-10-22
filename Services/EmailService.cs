namespace FutMatchApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            // Por enquanto, apenas loga o link. Implemente com SMTP real depois
            _logger.LogInformation($"Link de reset de senha para {email}: {resetLink}");

            // TODO: Implementar envio real de email usando SMTP
            // Exemplo com MailKit ou System.Net.Mail

            await Task.CompletedTask;
        }
    }
}
