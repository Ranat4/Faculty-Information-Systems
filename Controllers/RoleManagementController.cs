using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyInformationSystem_FIS_.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RoleManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoleManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles
                .Include(r => r.RoleAccesses)
                .OrderBy(r => r.Name)
                .ToListAsync();

            return View(roles);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (!ModelState.IsValid)
            {
                return View(role);
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Role role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(role);
            }

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Permissions(int id)
        {
            var role = await _context.Roles
                .Include(r => r.RoleAccesses)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Permissions(int id, Dictionary<string, string> permissions)
        {
            var role = await _context.Roles
                .Include(r => r.RoleAccesses)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound();
            }

            _context.RoleAccesses.RemoveRange(role.RoleAccesses);

            foreach (var module in Enum.GetValues<Modules>())
            {
                var key = $"access_{module}";

                if (permissions.TryGetValue(key, out var accessValue) &&
                    Enum.TryParse<AccessType>(accessValue, out var access))
                {
                    _context.RoleAccesses.Add(new RoleAccess
                    {
                        RoleId = role.Id,
                        Module = module,
                        Access = access
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }

}
