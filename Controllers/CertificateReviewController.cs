using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using FacultyInformationSystem_FIS_.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyInformationSystem_FIS_.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CertificateReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public CertificateReviewController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet("/certificate-review")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Certificate Submissions";
            var certificates = await _context.Certificates
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(certificates);
        }

        [HttpGet("/certificate-review/{id}")]
        public async Task<IActionResult> Review(int id)
        {
            var certificate = await _context.Certificates.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (certificate == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Review Certificate";
            return View(certificate);
        }

        [HttpPost("/certificate-review/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, string action, string? comment)
        {
            var certificate = await _context.Certificates.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (certificate == null)
            {
                return NotFound();
            }

            string actionText;
            switch (action)
            {
                case "approve":
                    certificate.Status = DocumentStatus.Reviewed;
                    actionText = "reviewed and approved";
                    break;
                case "sendback":
                    certificate.Status = DocumentStatus.SentBackToSubmitter;
                    actionText = "sent back for changes";
                    break;
                case "reject":
                    certificate.Status = DocumentStatus.Rejected;
                    actionText = "rejected";
                    break;
                default:
                    ModelState.AddModelError("", "Invalid action.");
                    return View(certificate);
            }

            certificate.ReviewComment = comment;
            await _context.SaveChangesAsync();

            var message = $"Your certificate submission \"{certificate.Title}\" has been {actionText}.";
            if (!string.IsNullOrWhiteSpace(comment))
            {
                message += $" Reviewer comment: {comment}";
            }

            await _notificationService.NotifyAsync(
                certificate.User,
                message,
                actionUrl: "/profile?tab=certificates",
                sendEmail: true,
                emailSubject: $"Update on your certificate submission: {certificate.Title}");

            TempData["FormSuccess"] = "Review submitted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
