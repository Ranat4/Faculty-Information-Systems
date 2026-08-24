using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacultyInformationSystem_FIS.Controllers
{
    [Authorize(Roles = "Administrator,Faculty")]
    public class PanelController : Controller
    {
        [HttpGet("/panel")]
        public IActionResult Index()
        {
            return View();
        }
    }
}