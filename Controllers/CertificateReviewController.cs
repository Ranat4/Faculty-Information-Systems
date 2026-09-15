using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using FacultyInformationSystem_FIS_.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyInformationSystem_FIS_.Controllers
{
    [Authorize(Roles = "Admin,Dean")]
    public class CertificateReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ICollegeScopeService _collegeScope;
        private readonly IExportService _exportService;

        public CertificateReviewController(ApplicationDbContext context, INotificationService notificationService, ICollegeScopeService collegeScope, IExportService exportService)
        {
            _context = context;
            _notificationService = notificationService;
            _collegeScope = collegeScope;
            _exportService = exportService;
        }

        private async Task<List<Certificate>> GetFilteredAsync(string? status, string? search)
        {
            var scopeCollegeId = await _collegeScope.GetScopeCollegeIdAsync(User);
            var query = _context.Certificates.Include(c => c.User).ThenInclude(u => u.College).AsQueryable();

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
                    c.IssuingOrganization.Contains(term) ||
                    c.User.FullName.Contains(term));
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        [HttpGet("/certificate-review")]
        public async Task<IActionResult> Index(string? status, string? search)
        {
            ViewData["Title"] = "Certificate Submissions";
            ViewBag.StatusFilter = status;
            ViewBag.SearchFilter = search;

            var certificates = await GetFilteredAsync(status, search);
            return View(certificates);
        }

        [HttpGet("/certificate-review/export/excel")]
        public async Task<IActionResult> ExportExcel(string? status, string? search)
        {
            var certificates = await GetFilteredAsync(status, search);

            var headers = new[] { "Faculty", "College", "Title", "Issuing Organization", "Certificate Number", "Start Date", "End Date", "Status", "Submitted On" };
            var rows = certificates.Select(c => (IReadOnlyList<string>)new[]
            {
                c.User.FullName,
                c.User.College?.Name ?? "—",
                c.Title,
                c.IssuingOrganization,
                c.CertificateNumber ?? "—",
                c.StartDate?.ToString("yyyy-MM-dd") ?? "—",
                c.EndDate?.ToString("yyyy-MM-dd") ?? "—",
                c.Status.ToString(),
                c.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList();

            var bytes = _exportService.BuildExcel("Certificate Submissions", headers, rows);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "certificate-submissions.xlsx");
        }

        [HttpGet("/certificate-review/export/pdf")]
        public async Task<IActionResult> ExportPdf(string? status, string? search)
        {
            var certificates = await GetFilteredAsync(status, search);

            var headers = new[] { "Faculty", "College", "Title", "Issuing Organization", "Certificate Number", "Start Date", "End Date", "Status", "Submitted On" };
            var rows = certificates.Select(c => (IReadOnlyList<string>)new[]
            {
                c.User.FullName,
                c.User.College?.Name ?? "—",
                c.Title,
                c.IssuingOrganization,
                c.CertificateNumber ?? "—",
                c.StartDate?.ToString("yyyy-MM-dd") ?? "—",
                c.EndDate?.ToString("yyyy-MM-dd") ?? "—",
                c.Status.ToString(),
                c.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList();

            var bytes = _exportService.BuildPdf("Certificate Submissions", headers, rows);
            return File(bytes, "application/pdf", "certificate-submissions.pdf");
        }

        [HttpGet("/certificate-review/{id}")]
        public async Task<IActionResult> Review(int id)
        {
            var certificate = await _context.Certificates.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (certificate == null)
            {
                return NotFound();
            }

            if (!await _collegeScope.CanAccessRecordAsync(User, certificate.User.CollegeId))
            {
                return Forbid();
            }

            if (certificate.Status == DocumentStatus.Submitted)
            {
                certificate.Status = DocumentStatus.UnderReview;
                await _context.SaveChangesAsync();
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

            if (!await _collegeScope.CanAccessRecordAsync(User, certificate.User.CollegeId))
            {
                return Forbid();
            }

            string actionText;
            switch (action)
            {
                case "approve":
                    certificate.Status = DocumentStatus.Approved;
                    actionText = "approved";
                    break;
                case "sendback":
                    certificate.Status = DocumentStatus.Returned;
                    actionText = "returned for changes";
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
