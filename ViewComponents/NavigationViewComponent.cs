using FutMatchApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace FutMatchApp.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        //private readonly ITeamService _teamService;

        //public NavigationViewComponent(ITeamService teamService)
        //{
        //    _teamService = teamService;
        //}

        //public async Task<IViewComponentResult> InvokeAsync()
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var userTeam = await _teamService.GetTeamByUserIdAsync(User.Identity.Name);
        //        return View(userTeam);
        //    }

        //    return View();
        //}
    }
}
