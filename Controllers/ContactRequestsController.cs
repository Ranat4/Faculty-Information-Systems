using FacultyInformationSystem_FIS_.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyInformationSystem_FIS_.Controllers
{
    public class ContactRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.ContactMessages
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(requests);
        }
        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.ContactMessages
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string status)
        {
            var request = await _context.ContactMessages.FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.ContactMessages.FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            _context.ContactMessages.Remove(request);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
