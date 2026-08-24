using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacultyInformationSystem_FIS_.Controllers
{
    [Authorize(Roles = "Administrator,Faculty")]
    public class PanelController : Controller
    {
        [HttpGet("/panel")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["ActivePanelPage"] = "Dashboard";
            return View();
        }
    }
}