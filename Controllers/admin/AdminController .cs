using FutMatchApp.Data;
using FutMatchApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutMatchApp.Controllers.admin
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGooglePlacesService _googlePlacesService;

        public AdminController(ApplicationDbContext context, IGooglePlacesService googlePlacesService)
        {
            _context = context;
            _googlePlacesService = googlePlacesService;
        }

        public async Task<IActionResult> ManageCourts()
        {
            var courts = await _context.Courts
                .OrderByDescending(c => c.IsFromGoogle)
                .ThenBy(c => c.Nome)
                .ToListAsync();

            return View(courts);
        }

        [HttpPost]
        public async Task<IActionResult> DiscoverCourts(string location, int radius = 10000)
        {
            try
            {
                var discoveredCourts = await _googlePlacesService.SearchFootballCourtsAsync(location, radius);
                var newCourts = 0;

                var existingCourts = await _context.Courts.ToListAsync();

                foreach (var court in discoveredCourts)
                {
                    var isDuplicate = existingCourts.Any(existing =>
                        existing.GooglePlaceId == court.GooglePlaceId ||
                        (existing.Nome.Equals(court.Nome, StringComparison.OrdinalIgnoreCase) &&
                         AreSimilarLocations(existing.Localizacao, court.Localizacao))
                    );

                    if (!isDuplicate)
                    {
                        court.Ativa = false;
                        court.IsFromGoogle = true;
                        _context.Courts.Add(court);
                        newCourts++;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Descobertas {discoveredCourts.Count} quadras, {newCourts} foram adicionadas como inativas para revisão.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao buscar quadras: {ex.Message}";
            }

            return RedirectToAction("ManageCourts");
        }

        private bool AreSimilarLocations(string location1, string location2)
        {
            if (string.IsNullOrEmpty(location1) || string.IsNullOrEmpty(location2))
                return false;

            var normalized1 = NormalizeLocation(location1);
            var normalized2 = NormalizeLocation(location2);

            var words1 = normalized1.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var words2 = normalized2.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var commonWords = words1.Intersect(words2).Count();
            return commonWords >= 2;
        }

        private string NormalizeLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
                return string.Empty;

            var normalized = location.ToLowerInvariant()
                .Replace("á", "a").Replace("à", "a").Replace("ã", "a").Replace("â", "a")
                .Replace("é", "e").Replace("ê", "e")
                .Replace("í", "i").Replace("î", "i")
                .Replace("ó", "o").Replace("ô", "o").Replace("õ", "o")
                .Replace("ú", "u").Replace("û", "u")
                .Replace("ç", "c");

            return System.Text.RegularExpressions.Regex.Replace(normalized, @"[^\w\s]", " ")
                .Trim()
                .Replace("  ", " ");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCourtStatus(int id)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court != null)
            {
                court.Ativa = !court.Ativa;
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Quadra {court.Nome} {(court.Ativa ? "ativada" : "desativada")} com sucesso.";
            }

            return RedirectToAction("ManageCourts");
        }
    }
}
