using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using FacultyInformationSystem_FIS_.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyInformationSystem_FIS_.Controllers
{
    [Authorize(Roles = "Admin,Dean")]
    public class ProfessionalDevelopmentReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ICollegeScopeService _collegeScope;
        private readonly IExportService _exportService;

        public ProfessionalDevelopmentReviewController(ApplicationDbContext context, INotificationService notificationService, ICollegeScopeService collegeScope, IExportService exportService)
        {
            _context = context;
            _notificationService = notificationService;
            _collegeScope = collegeScope;
            _exportService = exportService;
        }

        private async Task<List<ProfessionalDevelopment>> GetFilteredAsync(string? status, string? search)
        {
            var scopeCollegeId = await _collegeScope.GetScopeCollegeIdAsync(User);
            var query = _context.ProfessionalDevelopments.Include(p => p.User).ThenInclude(u => u.College).AsQueryable();

            if (scopeCollegeId.HasValue)
            {
                query = query.Where(p => p.User.CollegeId == scopeCollegeId);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DocumentStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(p => p.Status == parsedStatus);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    p.Title.Contains(term) ||
                    p.Organizer.Contains(term) ||
                    p.User.FullName.Contains(term));
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        [HttpGet("/professional-development-review")]
        public async Task<IActionResult> Index(string? status, string? search)
        {
            ViewData["Title"] = "Professional Development Submissions";
            ViewBag.StatusFilter = status;
            ViewBag.SearchFilter = search;

            var activities = await GetFilteredAsync(status, search);
            return View(activities);
        }

        [HttpGet("/professional-development-review/export/excel")]
        public async Task<IActionResult> ExportExcel(string? status, string? search)
        {
            var activities = await GetFilteredAsync(status, search);

            var headers = new[] { "Faculty", "College", "Title", "Type", "Organizer", "Location", "Role", "Start Date", "End Date", "Hours", "Certificate Received", "Status", "Submitted On" };
            var rows = activities.Select(p => (IReadOnlyList<string>)new[]
            {
                p.User.FullName,
                p.User.College?.Name ?? "—",
                p.Title,
                p.ActivityType.ToString(),
                p.Organizer,
                p.Location ?? "—",
                p.Role.ToString(),
                p.StartDate?.ToString("yyyy-MM-dd") ?? "—",
                p.EndDate?.ToString("yyyy-MM-dd") ?? "—",
                p.DurationHours?.ToString() ?? "—",
                p.CertificateReceived ? "Yes" : "No",
                p.Status.ToString(),
                p.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList();

            var bytes = _exportService.BuildExcel("Professional Development Submissions", headers, rows);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "professional-development-submissions.xlsx");
        }

        [HttpGet("/professional-development-review/export/pdf")]
        public async Task<IActionResult> ExportPdf(string? status, string? search)
        {
            var activities = await GetFilteredAsync(status, search);

            var headers = new[] { "Faculty", "College", "Title", "Type", "Organizer", "Location", "Role", "Start Date", "End Date", "Hours", "Cert. Received", "Status", "Submitted On" };
            var rows = activities.Select(p => (IReadOnlyList<string>)new[]
            {
                p.User.FullName,
                p.User.College?.Name ?? "—",
                p.Title,
                p.ActivityType.ToString(),
                p.Organizer,
                p.Location ?? "—",
                p.Role.ToString(),
                p.StartDate?.ToString("yyyy-MM-dd") ?? "—",
                p.EndDate?.ToString("yyyy-MM-dd") ?? "—",
                p.DurationHours?.ToString() ?? "—",
                p.CertificateReceived ? "Yes" : "No",
                p.Status.ToString(),
                p.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList();

            var bytes = _exportService.BuildPdf("Professional Development Submissions", headers, rows);
            return File(bytes, "application/pdf", "professional-development-submissions.pdf");
        }

        [HttpGet("/professional-development-review/{id}")]
        public async Task<IActionResult> Review(int id)
        {
            var activity = await _context.ProfessionalDevelopments.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
            if (activity == null)
            {
                return NotFound();
            }

            if (!await _collegeScope.CanAccessRecordAsync(User, activity.User.CollegeId))
            {
                return Forbid();
            }

            if (activity.Status == DocumentStatus.Submitted)
            {
                activity.Status = DocumentStatus.UnderReview;
                await _context.SaveChangesAsync();
            }

            ViewData["Title"] = "Review Professional Development";
            return View(activity);
        }

        [HttpPost("/professional-development-review/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, string action, string? comment)
        {
            var activity = await _context.ProfessionalDevelopments.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
            if (activity == null)
            {
                return NotFound();
            }

            if (!await _collegeScope.CanAccessRecordAsync(User, activity.User.CollegeId))
            {
                return Forbid();
            }

            string actionText;
            switch (action)
            {
                case "approve":
                    activity.Status = DocumentStatus.Approved;
                    actionText = "approved";
                    break;
                case "sendback":
                    activity.Status = DocumentStatus.Returned;
                    actionText = "returned for changes";
                    break;
                case "reject":
                    activity.Status = DocumentStatus.Rejected;
                    actionText = "rejected";
                    break;
                default:
                    ModelState.AddModelError("", "Invalid action.");
                    return View(activity);
            }

            activity.ReviewComment = comment;
            await _context.SaveChangesAsync();

            var message = $"Your professional development submission \"{activity.Title}\" has been {actionText}.";
            if (!string.IsNullOrWhiteSpace(comment))
            {
                message += $" Reviewer comment: {comment}";
            }

            await _notificationService.NotifyAsync(
                activity.User,
                message,
                actionUrl: "/profile?tab=professional-development",
                sendEmail: true,
                emailSubject: $"Update on your professional development submission: {activity.Title}");

            TempData["FormSuccess"] = "Review submitted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
