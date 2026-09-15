using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using FacultyInformationSystem_FIS_.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyInformationSystem_FIS_.Controllers
{
    [Authorize(Roles = "Admin,Dean")]
    public class CvReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ICollegeScopeService _collegeScope;
        private readonly IExportService _exportService;

        public CvReviewController(ApplicationDbContext context, INotificationService notificationService, ICollegeScopeService collegeScope, IExportService exportService)
        {
            _context = context;
            _notificationService = notificationService;
            _collegeScope = collegeScope;
            _exportService = exportService;
        }

        private async Task<List<CvRecord>> GetFilteredAsync(string? status, string? search)
        {
            var scopeCollegeId = await _collegeScope.GetScopeCollegeIdAsync(User);
            var query = _context.CvRecords.Include(c => c.User).ThenInclude(u => u.College).AsQueryable();

            if (scopeCollegeId.HasValue)
            {
                query = query.Where(c => c.User.CollegeId == scopeCollegeId);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DocumentStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(c => c.Status == parsedStatus);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(c =>
                    c.Title.Contains(term) ||
                    c.User.FullName.Contains(term));
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        [HttpGet("/cv-review")]
        public async Task<IActionResult> Index(string? status, string? search)
        {
            ViewData["Title"] = "CV Submissions";
            ViewBag.StatusFilter = status;
            ViewBag.SearchFilter = search;

            var cvs = await GetFilteredAsync(status, search);
            return View(cvs);
        }

        [HttpGet("/cv-review/export/excel")]
        public async Task<IActionResult> ExportExcel(string? status, string? search)
        {
            var cvs = await GetFilteredAsync(status, search);

            var headers = new[] { "Faculty", "College", "Title", "Date", "Status", "Submitted On" };
            var rows = cvs.Select(c => (IReadOnlyList<string>)new[]
            {
                c.User.FullName,
                c.User.College?.Name ?? "—",
                c.Title,
                c.Date?.ToString("yyyy-MM-dd") ?? "—",
                c.Status.ToString(),
                c.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList();

            var bytes = _exportService.BuildExcel("CV Submissions", headers, rows);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "cv-submissions.xlsx");
        }

        [HttpGet("/cv-review/export/pdf")]
        public async Task<IActionResult> ExportPdf(string? status, string? search)
        {
            var cvs = await GetFilteredAsync(status, search);

            var headers = new[] { "Faculty", "College", "Title", "Date", "Status", "Submitted On" };
            var rows = cvs.Select(c => (IReadOnlyList<string>)new[]
            {
                c.User.FullName,
                c.User.College?.Name ?? "—",
                c.Title,
                c.Date?.ToString("yyyy-MM-dd") ?? "—",
                c.Status.ToString(),
                c.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList();

            var bytes = _exportService.BuildPdf("CV Submissions", headers, rows);
            return File(bytes, "application/pdf", "cv-submissions.pdf");
        }

        [HttpGet("/cv-review/{id}")]
        public async Task<IActionResult> Review(int id)
        {
            var cv = await _context.CvRecords.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (cv == null)
            {
                return NotFound();
            }

            if (!await _collegeScope.CanAccessRecordAsync(User, cv.User.CollegeId))
            {
                return Forbid();
            }

            if (cv.Status == DocumentStatus.Submitted)
            {
                cv.Status = DocumentStatus.UnderReview;
                await _context.SaveChangesAsync();
            }

            ViewData["Title"] = "Review CV";
            return View(cv);
        }

        [HttpPost("/cv-review/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, string action, string? comment)
        {
            var cv = await _context.CvRecords.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (cv == null)
            {
                return NotFound();
            }

            if (!await _collegeScope.CanAccessRecordAsync(User, cv.User.CollegeId))
            {
                return Forbid();
            }

            string actionText;
            switch (action)
            {
                case "approve":
                    cv.Status = DocumentStatus.Approved;
                    actionText = "approved";
                    break;
                case "sendback":
                    cv.Status = DocumentStatus.Returned;
                    actionText = "returned for changes";
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

            var message = $"Your CV submission \"{cv.Title}\" has been {actionText}.";
            if (!string.IsNullOrWhiteSpace(comment))
            {
                message += $" Reviewer comment: {comment}";
            }

            await _notificationService.NotifyAsync(
                cv.User,
                message,
                actionUrl: "/profile?tab=cv",
                sendEmail: true,
                emailSubject: $"Update on your CV submission: {cv.Title}");

            TempData["FormSuccess"] = "Review submitted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
