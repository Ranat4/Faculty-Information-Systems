using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using FacultyInformationSystem_FIS_.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyInformationSystem_FIS_.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DegreeReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public DegreeReviewController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet("/degree-review")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Degree Submissions";
            var degrees = await _context.Degrees
                .Include(d => d.User)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            return View(degrees);
        }

        [HttpGet("/degree-review/{id}")]
        public async Task<IActionResult> Review(int id)
        {
            var degree = await _context.Degrees.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (degree == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Review Degree";
            return View(degree);
        }

        [HttpPost("/degree-review/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, string action, string? comment)
        {
            var degree = await _context.Degrees.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (degree == null)
            {
                return NotFound();
            }

            string actionText;
            switch (action)
            {
                case "approve":
                    degree.Status = DocumentStatus.Reviewed;
                    actionText = "reviewed and approved";
                    break;
                case "sendback":
                    degree.Status = DocumentStatus.SentBackToSubmitter;
                    actionText = "sent back for changes";
                    break;
                case "reject":
                    degree.Status = DocumentStatus.Rejected;
                    actionText = "rejected";
                    break;
                default:
                    ModelState.AddModelError("", "Invalid action.");
                    return View(degree);
            }

            degree.ReviewComment = comment;
            await _context.SaveChangesAsync();

            var message = $"Your degree submission \"{degree.Title}\" has been {actionText}.";
            if (!string.IsNullOrWhiteSpace(comment))
            {
                message += $" Reviewer comment: {comment}";
            }

            await _notificationService.NotifyAsync(
                degree.User,
                message,
                actionUrl: "/profile?tab=degrees",
                sendEmail: true,
                emailSubject: $"Update on your degree submission: {degree.Title}");

            TempData["FormSuccess"] = "Review submitted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
