using FutMatchApp.Data;
using FutMatchApp.Models;
using FutMatchApp.Models.Enums;
using FutMatchApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FutMatchApp.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.Teams)
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var now = DateTime.Now;
            List<Reservation> userReservations = new List<Reservation>();
            TeamStatsViewModel? teamStats = null;


            if (user.SelectedTeam != null)
            {
                userReservations = await _context.Reservations
                    .Include(r => r.Court)
                    .Include(r => r.Team)
                    .Include(r => r.OpponentTeam)
                    .Include(r => r.OpponentUser)
                    .Include(r => r.User)
                    .Where(r => (r.TeamId == user.SelectedTeam.Id || r.OpponentTeamId == user.SelectedTeam.Id) &&
                               r.Status != StatusReservation.Cancelada)
                    .OrderBy(r => r.DataHora)
                    .ToListAsync();

                teamStats = await CalculateTeamStats(user.SelectedTeam.Id);
            }

            var viewModel = new ProfileViewModel
            {
                User = user,
                UserTeams = user.Teams.ToList(),
                SelectedTeam = user.SelectedTeam,
                UpcomingGames = userReservations.Where(r => r.DataHora >= now).ToList(),
                PastGames = userReservations.Where(r => r.DataHora < now).ToList(),
                TeamStats = teamStats
            };

            return View(viewModel);
        }

        private async Task<TeamStatsViewModel> CalculateTeamStats(int teamId)
        {
            var completedMatches = await _context.Reservations
            .Include(r => r.Team)
            .Include(r => r.OpponentTeam)
            .Where(r => (r.TeamId == teamId || r.OpponentTeamId == teamId) &&
                       r.Status == StatusReservation.Finalizada &&
                       r.StatusResultado == StatusResultado.Confirmado) 
            .ToListAsync();

            var stats = new TeamStatsViewModel
            {
                TotalGames = completedMatches.Count,
                Victories = 0,
                Defeats = 0,
                Draws = 0,
                GoalsScored = 0,
                GoalsConceded = 0
            };

            foreach (var match in completedMatches)
            {
                bool isTeam1 = match.TeamId == teamId;
                var myResult = isTeam1 ? match.ResultadoTime1 : match.ResultadoTime2;
                var myGoals = isTeam1 ? match.GolsTime1 : match.GolsTime2;
                var opponentGoals = isTeam1 ? match.GolsTime2 : match.GolsTime1;

                switch (myResult)
                {
                    case ResultadoPartida.Vitoria:
                        stats.Victories++;
                        break;
                    case ResultadoPartida.Derrota:
                        stats.Defeats++;
                        break;
                    case ResultadoPartida.Empate:
                        stats.Draws++;
                        break;
                }

                stats.GoalsScored += myGoals ?? 0;
                stats.GoalsConceded += opponentGoals ?? 0;
            }

            return stats;
        }

        [HttpPost]
        public async Task<IActionResult> SelectTeam(int teamId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.UserId == userId);

            if (user == null || team == null)
            {
                TempData["Error"] = "Time não encontrado";
                return RedirectToAction("Index");
            }

            user.SelectedTeamId = teamId;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Time '{team.Nome}' selecionado!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeam(string nome, string descricao)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (string.IsNullOrWhiteSpace(nome))
            {
                TempData["Error"] = "Nome do time é obrigatório";
                return RedirectToAction("Index");
            }

            var team = new Team
            {
                Nome = nome.Trim(),
                Descricao = descricao?.Trim(),
                UserId = userId
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Time criado com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTeam(int teamId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.UserId == userId);

            if (team == null)
            {
                TempData["Error"] = "Time não encontrado";
                return RedirectToAction("Index");
            }

            var upcomingGames = await _context.Reservations
                .Where(r => (r.TeamId == teamId || r.OpponentTeamId == teamId) &&
                           r.DataHora > DateTime.Now &&
                           r.Status == StatusReservation.Confirmada)
                .CountAsync();

            if (upcomingGames > 0)
            {
                TempData["Error"] = $"Não é possível excluir o time. Há {upcomingGames} jogos confirmados futuros.";
                return RedirectToAction("Index");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.SelectedTeamId == teamId)
            {
                user.SelectedTeamId = null;
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Time removido com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var viewModel = new EditProfileViewModel
            {
                Nome = user.Nome,
                Email = user.Email,
                Telefone = user.Telefone,
                Cidade = user.Cidade
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            // Verifica se o email já está sendo usado por outro usuário
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email && u.Id != userId);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Este email já está em uso por outro usuário.");
                return View(model);
            }

            // Atualiza os dados do usuário
            user.Nome = model.Nome.Trim();
            user.Email = model.Email.Trim();
            user.Telefone = model.Telefone?.Trim();
            user.Cidade = model.Cidade?.Trim();

            await _context.SaveChangesAsync();

            TempData["Success"] = "Dados pessoais atualizados com sucesso!";
            return RedirectToAction("Index");
        }
    }
}
