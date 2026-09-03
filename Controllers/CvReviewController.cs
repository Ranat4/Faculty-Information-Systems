using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using FacultyInformationSystem_FIS_.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyInformationSystem_FIS_.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CvReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public CvReviewController(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet("/cv-review")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "CV Submissions";

            var cvs = await _context.CvRecords
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(cvs);
        }

        [HttpGet("/cv-review/{id}")]
        public async Task<IActionResult> Review(int id)
        {
            var cv = await _context.CvRecords
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cv == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Review CV";

            return View(cv);
        }

        [HttpPost("/cv-review/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(
            int id,
            string action,
            string? comment)
        {
            var cv = await _context.CvRecords
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cv == null)
            {
                return NotFound();
            }

            string actionText;

            switch (action)
            {
                case "approve":
                    cv.Status = DocumentStatus.Reviewed;
                    actionText = "reviewed and approved";
                    break;

                case "sendback":
                    cv.Status = DocumentStatus.SentBackToSubmitter;
                    actionText = "sent back for changes";
                    break;

                case "reject":
                    cv.Status = DocumentStatus.Rejected;
                    actionText = "rejected";
                    break;

                default:
                    ModelState.AddModelError("", "Invalid action.");
                    return View(cv);
            }

            cv.ReviewComment = comment;

            await _context.SaveChangesAsync();

            var message =
                $"Your CV submission \"{cv.Title}\" has been {actionText}.";

            if (!string.IsNullOrWhiteSpace(comment))
            {
                message +=
                    $" Reviewer comment: {comment}";
            }

            await _notificationService.NotifyAsync(
                cv.User,
                message,
                actionUrl: "/profile?tab=cv",
                sendEmail: true,
                emailSubject:
                    $"Update on your CV submission: {cv.Title}");

            TempData["FormSuccess"] = "Review submitted.";

            return RedirectToAction(nameof(Index));
        }
    }
}