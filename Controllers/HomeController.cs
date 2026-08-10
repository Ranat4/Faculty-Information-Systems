using FacultyInformationSystem_FIS_.Models;
using FacultyInformationSystem_FIS_.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FacultyInformationSystem_FIS_.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IEmailService emailService, ILogger<HomeController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet("/")]
        [HttpGet("/home")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Faculty Information System";
            ViewData["ActivePage"] = "Home";
            return View();
        }

        [HttpGet("/about")]
        public IActionResult About()
        {
            ViewData["Title"] = "About Us";
            ViewData["ActivePage"] = "About";
            return View();
        }

        [HttpGet("/faq")]
        public IActionResult Faq()
        {
            ViewData["Title"] = "FAQ";
            ViewData["ActivePage"] = "Faq";
            return View();
        }

        [HttpGet("/contact")]
        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact";
            ViewData["ActivePage"] = "Contact";
            return View(new ContactFormViewModel());
        }

        [HttpPost("/contact")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactFormViewModel model)
        {
            ViewData["Title"] = "Contact";
            ViewData["ActivePage"] = "Contact";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _emailService.SendAsync(
                    subject: $"New contact form message from {model.Name}",
                    plainTextBody: $"Name: {model.Name}\nEmail: {model.Email}\n\nMessage:\n{model.Message}",
                    replyToEmail: model.Email,
                    replyToName: model.Name);

                TempData["FormSuccess"] = "Thanks — your message has been sent. We'll get back to you soon.";
                return RedirectToAction(nameof(Contact));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact form email.");
                ModelState.AddModelError("", "Something went wrong sending your message. Please try again later.");
                return View(model);
            }
        }

        [HttpGet("/request-demo")]
        public IActionResult RequestDemo()
        {
            ViewData["Title"] = "Request a Demo";
            ViewData["ActivePage"] = "RequestDemo";
            return View(new DemoRequestViewModel());
        }

        [HttpPost("/request-demo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestDemo(DemoRequestViewModel model)
        {
            ViewData["Title"] = "Request a Demo";
            ViewData["ActivePage"] = "RequestDemo";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var fields = new (string Label, string Value)[]
                {
                    ("Name", model.Name),
                    ("Email", model.Email),
                    ("Institution", model.Institution),
                    ("What they want to see", model.Message)
                };
                var html = EmailTemplateBuilder.Build(
                    heading: "New Demo Request",
                    intro: $"{model.Name} from {model.Institution} requested a demo.",
                    fields: fields);
                var plainText = $"Name: {model.Name}\nEmail: {model.Email}\nInstitution: {model.Institution}\n\nWhat they want to see:\n{model.Message}";

                await _emailService.SendAsync(
                    subject: $"New demo request from {model.Name} ({model.Institution})",
                    plainTextBody: plainText,
                    replyToEmail: model.Email,
                    replyToName: model.Name,
                    htmlBody: html);

                TempData["FormSuccess"] = "Thanks — your demo request has been sent. We'll be in touch soon.";
                return RedirectToAction(nameof(RequestDemo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send demo request email.");
                ModelState.AddModelError("", "Something went wrong sending your request. Please try again later.");
                return View(model);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
