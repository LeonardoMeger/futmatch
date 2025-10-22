using FutMatchApp.Data;
using FutMatchApp.Models.Enums;
using FutMatchApp.Models.ViewModels;
using FutMatchApp.Models;
using FutMatchApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace FutMatchApp.Controllers
{
    public class MatchController : BaseController  
    {
        private readonly INotificationService _notificationService;
        public MatchController(ApplicationDbContext context, INotificationService notificationService)
            : base(context) 
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmResult(int reservationId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var reservation = await _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .Include(r => r.User)
                .Include(r => r.OpponentUser)
                .FirstOrDefaultAsync(r => r.Id == reservationId &&
                                        (r.UserId == userId || r.OpponentUserId == userId) &&
                                        r.Status == StatusReservation.Confirmada);

            if (reservation == null)
            {
                TempData["Error"] = "Partida não encontrada ou você não tem permissão para acessá-la.";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new ConfirmResultViewModel
            {
                Reservation = reservation,
                IsTeam1 = reservation.UserId == userId,
                CurrentUserId = userId
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmResult(int reservationId, ResultadoPartida resultado, int gols)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                //var success = await _notificationService.ConfirmarResultado(reservationId, userId, resultado, gols);
                var success = true;


                if (success)
                {
                    var reservation = await _context.Reservations
                        .Include(r => r.Team)
                        .Include(r => r.OpponentTeam)
                        .FirstOrDefaultAsync(r => r.Id == reservationId);

                    if (reservation?.ResultadoConfirmado == true)
                    {
                        await _notificationService.CriarNotificacaoResultadoPartida(
                            reservation.UserId,
                            reservationId,
                            $"Partida finalizada! {reservation.Team.Nome} {reservation.GolsTime1} x {reservation.GolsTime2} {reservation.OpponentTeam?.Nome}"
                        );

                        if (reservation.OpponentUserId.HasValue)
                        {
                            await _notificationService.CriarNotificacaoResultadoPartida(
                                reservation.OpponentUserId.Value,
                                reservationId,
                                $"Partida finalizada! {reservation.OpponentTeam?.Nome} {reservation.GolsTime2} x {reservation.GolsTime1} {reservation.Team.Nome}"
                            );
                        }
                    }

                    return Json(new
                    {
                        success = true,
                        message = "Resultado confirmado com sucesso!",
                        finalizado = reservation?.ResultadoConfirmado == true
                    });
                }

                return Json(new { success = false, message = "Não foi possível confirmar o resultado." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao confirmar resultado: {ex.Message}");
                return Json(new { success = false, message = "Erro interno. Tente novamente." });
            }
        }


        public async Task<IActionResult> History()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var matches = await _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .Include(r => r.User)
                .Include(r => r.OpponentUser)
                .Where(r => (r.UserId == userId || r.OpponentUserId == userId) &&
                           r.Status == StatusReservation.Finalizada)
                .OrderByDescending(r => r.DataHora)
                .ToListAsync();

            return View(matches);
        }


        public async Task<IActionResult> Pending()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var pendingMatches = await _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .Include(r => r.User)
                .Include(r => r.OpponentUser)
                .Where(r => (r.UserId == userId || r.OpponentUserId == userId) &&
                           r.Status == StatusReservation.Confirmada &&
                           r.DataHora < DateTime.Now && 
                           !r.ResultadoConfirmado) 
                .OrderByDescending(r => r.DataHora)
                .ToListAsync();

            return View(pendingMatches);
        }


        public async Task<IActionResult> Upcoming()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var upcomingMatches = await _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .Include(r => r.User)
                .Include(r => r.OpponentUser)
                .Where(r => (r.UserId == userId || r.OpponentUserId == userId) &&
                           r.Status == StatusReservation.Confirmada &&
                           r.DataHora >= DateTime.Now)
                .OrderBy(r => r.DataHora)
                .ToListAsync();

            return View(upcomingMatches);
        }


        public async Task<IActionResult> Details(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var match = await _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .Include(r => r.User)
                .Include(r => r.OpponentUser)
                .FirstOrDefaultAsync(r => r.Id == id &&
                                        (r.UserId == userId || r.OpponentUserId == userId));

            if (match == null)
            {
                TempData["Error"] = "Partida não encontrada.";
                return RedirectToAction("History");
            }

            return View(match);
        }


        [HttpPost]
        public async Task<IActionResult> CancelMatch(int reservationId, string motivo = "")
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Team)
                    .Include(r => r.OpponentTeam)
                    .FirstOrDefaultAsync(r => r.Id == reservationId &&
                                            (r.UserId == userId || r.OpponentUserId == userId) &&
                                            r.Status == StatusReservation.Confirmada &&
                                            r.DataHora > DateTime.Now);

                if (reservation == null)
                {
                    return Json(new { success = false, message = "Partida não pode ser cancelada." });
                }

                reservation.Status = StatusReservation.Cancelada;
                reservation.ObservacoesPartida = $"Cancelada: {motivo}";

                var otherUserId = reservation.UserId == userId ? reservation.OpponentUserId : reservation.UserId;
                var cancelingTeam = reservation.UserId == userId ? reservation.Team.Nome : reservation.OpponentTeam?.Nome;

                if (otherUserId.HasValue)
                {
                    var notification = new Notification
                    {
                        UserId = otherUserId.Value,
                        ReservationId = reservationId,
                        Tipo = TipoNotificacao.DesafioCancelado,
                        Titulo = "Partida Cancelada",
                        Mensagem = $"A partida contra {cancelingTeam} foi cancelada. {(string.IsNullOrEmpty(motivo) ? "" : "Motivo: " + motivo)}",
                        PermiteAcao = false
                    };

                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Partida cancelada com sucesso. O time adversário foi notificado."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao cancelar partida: {ex.Message}");
                return Json(new { success = false, message = "Erro ao cancelar partida." });
            }
        }

        public async Task<IActionResult> Stats()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var matches = await _context.Reservations
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .Where(r => (r.UserId == userId || r.OpponentUserId == userId) &&
                           r.Status == StatusReservation.Finalizada)
                .ToListAsync();

            var stats = new MatchStatsViewModel
            {
                TotalMatches = matches.Count,
                Victories = matches.Count(m => {
                    var isTeam1 = m.UserId == userId;
                    var myResult = isTeam1 ? m.ResultadoTime1 : m.ResultadoTime2;
                    return myResult == ResultadoPartida.Vitoria;
                }),
                Draws = matches.Count(m => {
                    var isTeam1 = m.UserId == userId;
                    var myResult = isTeam1 ? m.ResultadoTime1 : m.ResultadoTime2;
                    return myResult == ResultadoPartida.Empate;
                }),
                Defeats = matches.Count(m => {
                    var isTeam1 = m.UserId == userId;
                    var myResult = isTeam1 ? m.ResultadoTime1 : m.ResultadoTime2;
                    return myResult == ResultadoPartida.Derrota;
                }),
                TotalGoalsScored = matches.Sum(m => {
                    var isTeam1 = m.UserId == userId;
                    return (isTeam1 ? m.GolsTime1 : m.GolsTime2) ?? 0;
                }),
                TotalGoalsConceded = matches.Sum(m => {
                    var isTeam1 = m.UserId == userId;
                    return (isTeam1 ? m.GolsTime2 : m.GolsTime1) ?? 0;
                }),
                RecentMatches = matches.OrderByDescending(m => m.DataHora).Take(5).ToList()
            };

            return View(stats);
        }

        [HttpPost]
        public async Task<IActionResult> AddMatchNote(int reservationId, string observacao)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.Id == reservationId &&
                                            (r.UserId == userId || r.OpponentUserId == userId) &&
                                            r.Status == StatusReservation.Finalizada);

                if (reservation == null)
                {
                    return Json(new { success = false, message = "Partida não encontrada." });
                }

                reservation.ObservacoesPartida = observacao;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Observação adicionada com sucesso!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao adicionar observação: {ex.Message}");
                return Json(new { success = false, message = "Erro ao salvar observação." });
            }
        }
    }
}
