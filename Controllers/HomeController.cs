using FacultyInformationSystem_FIS_.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FacultyInformationSystem_FIS_.Controllers
{

    public class HomeController : Controller
    {
      
        [Route("/")]
        [Route("/home")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Faculty Information System";
            ViewData["ActivePage"] = "Home";
            return View();
        }


        [Route("/about")]
        public IActionResult About()
        {
            ViewData["Title"] = "About Us";
            ViewData["ActivePage"] = "About";
            return View();
        }


        [Route("/faq")]
        public IActionResult Faq()
        {
            ViewData["Title"] = "FAQ";
            ViewData["ActivePage"] = "Faq";
            return View();
        }


        [Route("/contact")]
        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact";
            ViewData["ActivePage"] = "Contact";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
