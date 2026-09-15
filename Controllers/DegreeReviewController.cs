using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using FacultyInformationSystem_FIS_.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyInformationSystem_FIS_.Controllers
{
    [Authorize(Roles = "Admin,Dean")]
    public class DegreeReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ICollegeScopeService _collegeScope;
        private readonly IExportService _exportService;

        public DegreeReviewController(ApplicationDbContext context, INotificationService notificationService, ICollegeScopeService collegeScope, IExportService exportService)
        {
            _context = context;
            _notificationService = notificationService;
            _collegeScope = collegeScope;
            _exportService = exportService;
        }

        private async Task<List<Degree>> GetFilteredAsync(string? status, string? search)
        {
            var scopeCollegeId = await _collegeScope.GetScopeCollegeIdAsync(User);
            var query = _context.Degrees.Include(d => d.User).ThenInclude(u => u.College).AsQueryable();

            if (scopeCollegeId.HasValue)
            {
                query = query.Where(d => d.User.CollegeId == scopeCollegeId);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DocumentStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(d => d.Status == parsedStatus);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(d =>
                    d.Title.Contains(term) ||
                    d.Institution.Contains(term) ||
                    d.User.FullName.Contains(term));
            }

            return await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
        }

        [HttpGet("/degree-review")]
        public async Task<IActionResult> Index(string? status, string? search)
        {
            ViewData["Title"] = "Degree Submissions";
            ViewBag.StatusFilter = status;
            ViewBag.SearchFilter = search;

            var degrees = await GetFilteredAsync(status, search);
            return View(degrees);
        }

        [HttpGet("/degree-review/export/excel")]
        public async Task<IActionResult> ExportExcel(string? status, string? search)
        {
            var degrees = await GetFilteredAsync(status, search);

            var headers = new[] { "Faculty", "College", "Title", "Institution", "Field of Study", "Year Obtained", "Status", "Submitted On" };
            var rows = degrees.Select(d => (IReadOnlyList<string>)new[]
            {
                d.User.FullName,
                d.User.College?.Name ?? "—",
                d.Title,
                d.Institution,
                d.FieldOfStudy ?? "—",
                d.YearObtained?.ToString() ?? "—",
                d.Status.ToString(),
                d.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList();

            var bytes = _exportService.BuildExcel("Degree Submissions", headers, rows);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "degree-submissions.xlsx");
        }

        [HttpGet("/degree-review/export/pdf")]
        public async Task<IActionResult> ExportPdf(string? status, string? search)
        {
            var degrees = await GetFilteredAsync(status, search);

            var headers = new[] { "Faculty", "College", "Title", "Institution", "Field of Study", "Year Obtained", "Status", "Submitted On" };
            var rows = degrees.Select(d => (IReadOnlyList<string>)new[]
            {
                d.User.FullName,
                d.User.College?.Name ?? "—",
                d.Title,
                d.Institution,
                d.FieldOfStudy ?? "—",
                d.YearObtained?.ToString() ?? "—",
                d.Status.ToString(),
                d.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList();

            var bytes = _exportService.BuildPdf("Degree Submissions", headers, rows);
            return File(bytes, "application/pdf", "degree-submissions.pdf");
        }

        [HttpGet("/degree-review/{id}")]
        public async Task<IActionResult> Review(int id)
        {
            var degree = await _context.Degrees.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (degree == null)
            {
                return NotFound();
            }

            if (!await _collegeScope.CanAccessRecordAsync(User, degree.User.CollegeId))
            {
                return Forbid();
            }

            // Opening the review page marks it as being looked at.
            if (degree.Status == DocumentStatus.Submitted)
            {
                degree.Status = DocumentStatus.UnderReview;
                await _context.SaveChangesAsync();
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

            if (!await _collegeScope.CanAccessRecordAsync(User, degree.User.CollegeId))
            {
                return Forbid();
            }

            string actionText;
            switch (action)
            {
                case "approve":
                    degree.Status = DocumentStatus.Approved;
                    actionText = "approved";
                    break;
                case "sendback":
                    degree.Status = DocumentStatus.Returned;
                    actionText = "returned for changes";
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
