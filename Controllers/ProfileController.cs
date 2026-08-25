using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FacultyInformationSystem_FIS_.Controllers
{
    // Rana's CV tab actions go in this same controller — Index below
    // already builds ViewBag.Cvs for her to populate; her actions should
    // follow the same UserId == CurrentUserId scoping as Degree here.
    [Authorize(Roles = "Faculty,Department Chair,Dean,Admin")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> AddDegree(Degree model)
        {
            ViewData["Title"] = "Add Degree";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.UserId = CurrentUserId;
            model.CreatedAt = DateTime.UtcNow;

            _context.Degrees.Add(model);
            await _context.SaveChangesAsync();

            TempData["FormSuccess"] = "Degree added.";
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
        public async Task<IActionResult> EditDegree(int id, Degree model)
        {
            ViewData["Title"] = "Edit Degree";

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

            await _context.SaveChangesAsync();

            TempData["FormSuccess"] = "Degree updated.";
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
    }
}
