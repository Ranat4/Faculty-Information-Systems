using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyInformationSystem_FIS_.Controllers
{
    // Login, Forgot Password, Reset Password actions belong here too —
    // whoever builds those should add them to this same controller.
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet("/register")]
        public async Task<IActionResult> Register()
        {
            ViewData["Title"] = "Register";
            ViewData["ActivePage"] = "Register";
            ViewBag.Roles = await _context.Roles.OrderBy(r => r.Name).ToListAsync();
            return View(new RegisterViewModel());
        }

        [HttpPost("/register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewData["Title"] = "Register";
            ViewData["ActivePage"] = "Register";

            var emailTaken = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailTaken)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.OrderBy(r => r.Name).ToListAsync();
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                RoleId = model.RoleId,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["FormSuccess"] = "Account created. Login isn't wired up yet — check back soon.";
            return RedirectToAction(nameof(Register));
        }
    }
}
