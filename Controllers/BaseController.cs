using FutMatchApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FutMatchApp.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override ViewResult View(object? model)
        {
            LoadUserDataForNavBar();
            return base.View(model);
        }

        public override ViewResult View(string? viewName, object? model)
        {
            LoadUserDataForNavBar();
            return base.View(viewName, model);
        }

        private void LoadUserDataForNavBar()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUser = _context.Users
                    .Include(u => u.SelectedTeam)
                    .FirstOrDefault(u => u.Id == userId);

                ViewBag.CurrentUser = currentUser;
                ViewBag.CurrentTeam = currentUser?.SelectedTeam;
            }
        }
    }
}
