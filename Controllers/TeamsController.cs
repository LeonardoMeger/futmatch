using FutMatchApp.Data;
using FutMatchApp.Models.ViewModels;
using FutMatchApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace FutMatchApp.Controllers
{
    [Authorize]
    public class TeamsController : BaseController
    {
        private readonly IWebHostEnvironment _environment;

        public TeamsController(ApplicationDbContext context, IWebHostEnvironment environment) : base(context)
        {
            _environment = environment;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (model.IdadeMinima.HasValue && model.IdadeMaxima.HasValue &&
                    model.IdadeMinima > model.IdadeMaxima)
                {
                    ModelState.AddModelError("IdadeMaxima", "Idade máxima deve ser maior que a idade mínima");
                    return View(model);
                }

                var team = new Team
                {
                    Nome = model.Nome,
                    Descricao = model.Descricao,
                    UserId = userId,
                    CorPrimaria = model.CorPrimaria,
                    CorSecundaria = model.CorSecundaria,
                    IdadeMinima = model.IdadeMinima,
                    IdadeMaxima = model.IdadeMaxima
                };

                if (model.FotoEscudo != null && model.FotoEscudo.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "teams");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.FotoEscudo.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.FotoEscudo.CopyToAsync(fileStream);
                    }

                    team.FotoUrl = "/uploads/teams/" + uniqueFileName;
                }

                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Time '{team.Nome}' criado com sucesso!";
                return RedirectToAction("Index", "Profile");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (team == null)
            {
                TempData["Error"] = "Time não encontrado";
                return RedirectToAction("Index", "Profile");
            }

            var viewModel = new CreateTeamViewModel
            {
                Nome = team.Nome,
                Descricao = team.Descricao,
                CorPrimaria = team.CorPrimaria,
                CorSecundaria = team.CorSecundaria,
                IdadeMinima = team.IdadeMinima,
                IdadeMaxima = team.IdadeMaxima
            };

            ViewBag.Team = team; 
            return View("Create", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateTeamViewModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (team == null)
            {
                TempData["Error"] = "Time não encontrado";
                return RedirectToAction("Index", "Profile");
            }

            if (ModelState.IsValid)
            {
                if (model.IdadeMinima.HasValue && model.IdadeMaxima.HasValue &&
                    model.IdadeMinima > model.IdadeMaxima)
                {
                    ModelState.AddModelError("IdadeMaxima", "Idade máxima deve ser maior que a idade mínima");
                    ViewBag.Team = team;
                    return View("Create", model);
                }

                team.Nome = model.Nome;
                team.Descricao = model.Descricao;
                team.CorPrimaria = model.CorPrimaria;
                team.CorSecundaria = model.CorSecundaria;
                team.IdadeMinima = model.IdadeMinima;
                team.IdadeMaxima = model.IdadeMaxima;

                if (model.FotoEscudo != null && model.FotoEscudo.Length > 0)
                {
                    if (!string.IsNullOrEmpty(team.FotoUrl))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, team.FotoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "teams");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.FotoEscudo.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.FotoEscudo.CopyToAsync(fileStream);
                    }

                    team.FotoUrl = "/uploads/teams/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Time '{team.Nome}' atualizado com sucesso!";
                return RedirectToAction("Index", "Profile");
            }

            ViewBag.Team = team;
            return View("Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (team == null)
            {
                TempData["Error"] = "Time não encontrado";
                return RedirectToAction("Index", "Profile");
            }

            if (!string.IsNullOrEmpty(team.FotoUrl))
            {
                var filePath = Path.Combine(_environment.WebRootPath, team.FotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.SelectedTeamId == id)
            {
                user.SelectedTeamId = null;
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Time '{team.Nome}' removido com sucesso!";
            return RedirectToAction("Index", "Profile");
        }
    }
}
