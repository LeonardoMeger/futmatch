using FutMatchApp.Data;
using FutMatchApp.Models;
using FutMatchApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FutMatchApp.ViewComponents
{
    public class UserNavViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context; 

        public UserNavViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsPrincipal = (ClaimsPrincipal)User;

            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

                if (int.TryParse(userIdString, out int userId))
                {
                    var team = await _context.Teams.FirstOrDefaultAsync(t => t.UserId == userId);

                    var model = new UserNavViewModel
                    {
                        UserName = claimsPrincipal.Identity.Name,
                        TeamPhoto = team?.FotoUrl
                    };
                    return View(model);
                }
            }

            return View(new UserNavViewModel { UserName = "Visitante" });
        }
    }
}
