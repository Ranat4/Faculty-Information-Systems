using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using FacultyInformationSystem_FIS_.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FacultyInformationSystem_FIS_.Controllers
{
    [Authorize(Roles = "Faculty,Department Chair,Dean,Admin")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _env;

        public ProfileController(ApplicationDbContext context, INotificationService notificationService, IWebHostEnvironment env)
        {
            _context = context;
            _notificationService = notificationService;
            _env = env;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("/profile")]
        public async Task<IActionResult> Index(string tab = "degrees")
        {
            ViewData["Title"] = "My Profile";
            ViewData["ActiveTab"] = tab;

            ViewBag.Degrees = await _context.Degrees
                .Where(d => d.UserId == CurrentUserId)
                .OrderByDescending(d => d.YearObtained)
                .ToListAsync();

            // TODO (Rana): populate from a CvRecord table, same pattern.
            ViewBag.Cvs = new List<object>();

            return View();
        }

        [HttpGet("/profile/degrees/add")]
        public IActionResult AddDegree()
        {
            ViewData["Title"] = "Add Degree";
            return View(new Degree());
        }

        [HttpPost("/profile/degrees/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDegree(Degree model, IFormFile? file)
        {
            ViewData["Title"] = "Add Degree";

            var fileError = FileValidationHelper.Validate(file);
            if (fileError != null)
            {
                ModelState.AddModelError("file", fileError);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.UserId = CurrentUserId;
            model.CreatedAt = DateTime.UtcNow;
            model.Status = DocumentStatus.PendingReview;

            if (file != null && file.Length > 0)
            {
                model.FileName = file.FileName;
                model.FilePath = await FileValidationHelper.SaveAsync(file, "degrees", _env.WebRootPath);
            }

            _context.Degrees.Add(model);
            await _context.SaveChangesAsync();

            await NotifyAdminsOfSubmission(model);

            TempData["FormSuccess"] = "Degree submitted for review.";
            return RedirectToAction(nameof(Index), new { tab = "degrees" });
        }

        [HttpGet("/profile/degrees/{id}/edit")]
        public async Task<IActionResult> EditDegree(int id)
        {
            var degree = await _context.Degrees
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == CurrentUserId);

            if (degree == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Edit Degree";
            return View(degree);
        }

        [HttpPost("/profile/degrees/{id}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDegree(int id, Degree model, IFormFile? file)
        {
            ViewData["Title"] = "Edit Degree";

            var fileError = FileValidationHelper.Validate(file);
            if (fileError != null)
            {
                ModelState.AddModelError("file", fileError);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var degree = await _context.Degrees
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == CurrentUserId);

            if (degree == null)
            {
                return NotFound();
            }

            degree.Title = model.Title;
            degree.Institution = model.Institution;
            degree.FieldOfStudy = model.FieldOfStudy;
            degree.YearObtained = model.YearObtained;
            degree.Notes = model.Notes;

            if (file != null && file.Length > 0)
            {
                degree.FileName = file.FileName;
                degree.FilePath = await FileValidationHelper.SaveAsync(file, "degrees", _env.WebRootPath);
            }

            // Editing counts as resubmission — goes back to review.
            degree.Status = DocumentStatus.PendingReview;
            degree.ReviewComment = null;

            await _context.SaveChangesAsync();
            await NotifyAdminsOfSubmission(degree);

            TempData["FormSuccess"] = "Degree updated and resubmitted for review.";
            return RedirectToAction(nameof(Index), new { tab = "degrees" });
        }

        [HttpPost("/profile/degrees/{id}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDegree(int id)
        {
            var degree = await _context.Degrees
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == CurrentUserId);

            if (degree != null)
            {
                _context.Degrees.Remove(degree);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { tab = "degrees" });
        }

        private async Task NotifyAdminsOfSubmission(Degree degree)
        {
            var admins = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Admin"))
                .ToListAsync();

            foreach (var admin in admins)
            {
                await _notificationService.NotifyAsync(
                    admin,
                    "Faculty has submitted a document. Please review.",
                    actionUrl: $"/degree-review/{degree.Id}",
                    sendEmail: true,
                    emailSubject: "New document submitted for review");
            }
        }
    }
}
