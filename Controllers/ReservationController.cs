using FutMatchApp.Data;
using FutMatchApp.Models.ViewModels;
using FutMatchApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FutMatchApp.Models.Enums;

namespace FutMatchApp.Controllers
{
    [Authorize]
    public class ReservationController : BaseController
    {
        public ReservationController(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IActionResult> Index(string courtName, DateTime? startDate, DateTime? endDate, string opponentTeamName)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var query = _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .Where(r => r.UserId == userId || r.OpponentUserId == userId);

            if (!string.IsNullOrEmpty(courtName))
            {
                query = query.Where(r => r.Court.Nome.Contains(courtName));
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.DataHora.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.DataHora.Date <= endDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(opponentTeamName))
            {
                query = query.Where(r => r.OpponentTeam != null &&
                                        r.OpponentTeam.Nome.Contains(opponentTeamName));
            }

            var reservations = await query
                .OrderByDescending(r => r.DataHora)
                .ToListAsync();

            ViewBag.SelectedCourtName = courtName;
            ViewBag.SelectedStartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedEndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedOpponentTeamName = opponentTeamName;

            return View(reservations);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.Teams)
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Teams.Count == 0)
            {
                TempData["Error"] = "Você precisa criar pelo menos um time antes de fazer uma reserva.";
                return RedirectToAction("Index", "Profile");
            }

            var viewModel = new ReservationViewModel
            {
                AvailableCourts = await _context.Courts.Where(c => c.Ativa).ToListAsync(),
                UserTeams = user.Teams.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReservationViewModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
            {
                ModelState.AddModelError("", "Selecione um time no seu perfil antes de fazer uma reserva.");
            }

            var dataFim = model.DataHora.AddHours(model.DuracaoHoras);
            var conflictingReservation = await _context.Reservations
                .Where(r => r.CourtId == model.CourtId &&
                           r.Status != StatusReservation.Cancelada &&
                           ((r.DataHora <= model.DataHora && r.DataHora.AddHours(r.DuracaoHoras) > model.DataHora) ||
                            (r.DataHora < dataFim && r.DataHora >= model.DataHora)))
                .FirstOrDefaultAsync();

            if (conflictingReservation != null)
            {
                ModelState.AddModelError("DataHora", "Este horário já está reservado.");
            }

            if (ModelState.IsValid)
            {
                var reservation = new Reservation
                {
                    UserId = userId,
                    TeamId = user.SelectedTeam.Id,
                    CourtId = model.CourtId,
                    DataHora = model.DataHora,
                    DuracaoHoras = model.DuracaoHoras,
                    Observacoes = model.Observacoes,
                    Status = StatusReservation.Pendente
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Reserva criada com sucesso! Aguardando outro time aceitar o desafio.";
                return RedirectToAction("Index");
            }

            model.AvailableCourts = await _context.Courts.Where(c => c.Ativa).ToListAsync();
            model.UserTeams = await _context.Teams.Where(t => t.UserId == userId).ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> Available(string location, int? courtId, string timeSlot, DateTime? date)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
            {
                TempData["Error"] = "Selecione um time no seu perfil para aceitar desafios.";
                return RedirectToAction("Index", "Profile");
            }

            var query = _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.Team)
                .Include(r => r.User)
                .Where(r => r.Status == StatusReservation.Pendente &&
                           r.UserId != userId &&
                           r.DataHora > DateTime.Now);

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(r => r.Court.Localizacao.Contains(location));
            }

            if (courtId.HasValue)
            {
                query = query.Where(r => r.CourtId == courtId.Value);
            }

            if (!string.IsNullOrEmpty(timeSlot))
            {
                var time = TimeSpan.Parse(timeSlot);
                query = query.Where(r => r.DataHora.TimeOfDay == time);
            }

            if (date.HasValue)
            {
                query = query.Where(r => r.DataHora.Date == date.Value.Date);
            }

            var reservations = await query.OrderBy(r => r.DataHora).ToListAsync();

            var allCourts = await _context.Courts.Where(c => c.Ativa).ToListAsync();
            var allLocations = allCourts.Select(c => c.Localizacao).Distinct().ToList();
            var timeSlots = new List<string>
            {
                "06:00", "07:00", "08:00", "09:00", "10:00", "11:00",
                "12:00", "13:00", "14:00", "15:00", "16:00", "17:00",
                "18:00", "19:00", "20:00", "21:00", "22:00"
            };

            var viewModel = new AvailableReservationsViewModel
            {
                Reservations = reservations,
                Courts = allCourts,
                Locations = allLocations,
                TimeSlots = timeSlots,
                SelectedLocation = location,
                SelectedCourtId = courtId,
                SelectedTimeSlot = timeSlot,
                SelectedDate = date
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptChallenge(int reservationId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
            {
                TempData["Error"] = "Selecione um time no seu perfil.";
                return RedirectToAction("Available");
            }

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId &&
                                        r.Status == StatusReservation.Pendente);

            if (reservation == null)
            {
                TempData["Error"] = "Reserva não encontrada ou não disponível.";
                return RedirectToAction("Available");
            }

            reservation.OpponentUserId = userId;
            reservation.OpponentTeamId = user.SelectedTeam.Id;
            reservation.Status = StatusReservation.Confirmada;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Desafio aceito! A partida foi confirmada.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int reservationId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId &&
                                        (r.UserId == userId || r.OpponentUserId == userId));

            if (reservation == null)
            {
                TempData["Error"] = "Reserva não encontrada.";
                return RedirectToAction("Index");
            }

            reservation.Status = StatusReservation.Cancelada;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reserva cancelada com sucesso.";
            return RedirectToAction("Index");
        }
    }
}
