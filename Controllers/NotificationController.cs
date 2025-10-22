using FutMatchApp.Data;
using FutMatchApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FutMatchApp.Controllers
{
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        public NotificationController(ApplicationDbContext context, INotificationService notificationService)
            : base(context)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var notifications = await _notificationService.BuscarNotificacoesUsuario(userId);

            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptChallenge(int notificationId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var success = await _notificationService.AceitarDesafio(notificationId, userId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Desafio aceito com sucesso! Partida confirmada.",
                        title = "Desafio Aceito!"
                    });
                }

                return Json(new { success = false, message = "Não foi possível aceitar o desafio." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao aceitar desafio: {ex.Message}");
                return Json(new { success = false, message = "Erro interno. Tente novamente." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectChallenge(int notificationId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var success = await _notificationService.RejeitarDesafio(notificationId, userId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Desafio rejeitado. O time foi notificado.",
                        title = "Desafio Rejeitado"
                    });
                }

                return Json(new { success = false, message = "Não foi possível rejeitar o desafio." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao rejeitar desafio: {ex.Message}");
                return Json(new { success = false, message = "Erro interno. Tente novamente." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                await _notificationService.MarcarComoLida(notificationId, userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao marcar como lida: {ex.Message}");
                return Json(new { success = false, message = "Erro ao marcar notificação." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var notifications = await _notificationService.BuscarNotificacoesUsuario(userId);
                var unreadCount = await _notificationService.ContarNotificacoesNaoLidas(userId);

                var result = notifications.Take(10).Select(n => new
                {
                    id = n.Id,
                    titulo = n.Titulo,
                    mensagem = n.Mensagem,
                    tipo = n.Tipo.ToString(),
                    lida = n.Lida,
                    dataCriacao = n.DataCriacao.ToString("dd/MM HH:mm"),
                    reservationId = n.ReservationId,
                    permiteAcao = n.PermiteAcao,
                    acaoPositiva = n.AcaoPositiva,
                    acaoNegativa = n.AcaoNegativa,
                    urlAcao = n.UrlAcao
                }).ToList();

                return Json(new
                {
                    success = true,
                    notifications = result,
                    unreadCount = unreadCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar notificações: {ex.Message}");
                return Json(new { success = false, message = "Erro ao carregar notificações." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                await _notificationService.MarcarTodasComoLidas(userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao marcar todas como lidas: {ex.Message}");
                return Json(new { success = false, message = "Erro ao marcar notificações." });
            }
        }
    }
}
