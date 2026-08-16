using FacultyInformationSystem_FIS_.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyInformationSystem_FIS_.Controllers
{
    [Route("admin/demo-requests")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var requests = await _context.DemoRequests
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
            return View(requests);
        }

        [HttpPost("{id}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name, string email, string institution, string message, string status)
        {
            var request = await _context.DemoRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            request.Name = name;
            request.Email = email;
            request.Institution = institution;
            request.Message = message;
            request.Status = status;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{id}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.DemoRequests.FindAsync(id);
            if (request != null)
            {
                _context.DemoRequests.Remove(request);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
