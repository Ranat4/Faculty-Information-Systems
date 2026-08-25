using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;
using FacultyInformationSystem_FIS_.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FacultyInformationSystem_FIS_.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IEmailService _emailService;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, IEmailService emailService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
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
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = model.RoleId
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            TempData["FormSuccess"] = "Account created — you can now log in.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet("/login")]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["Title"] = "Log in";
            ViewData["ActivePage"] = "Login";
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost("/login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["Title"] = "Log in";
            ViewData["ActivePage"] = "Login";
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Incorrect email or password.");
                return View(model);
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Incorrect email or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
            }

            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            var accesses = await _context.RoleAccesses
            .Where(ra => roleIds.Contains(ra.RoleId))
            .Select(ra => new { ra.Module, ra.Access })
            .Distinct()
            .ToListAsync();
            
            foreach (var access in accesses)
            {
            claims.Add(new Claim("Permission", $"{access.Module}:{access.Access}"));
            }
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = model.RememberMe });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            var panelRoles = new[] { "Admin", "Faculty", "Department Chair", "Dean" };
            if (user.UserRoles.Any(ur => panelRoles.Contains(ur.Role.Name)))
            {
            return RedirectToAction("Index", "Panel");
            }
            return RedirectToAction("Index", "Home");
        }


        [HttpPost("/logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("/access-denied")]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Access Denied";
            return View();
        }

        [Authorize(Policy = "ChangePassword")]
        [HttpGet("/change-password")]
        public IActionResult ChangePassword()
        {
            ViewData["Title"] = "Change Password";
            return View(new ChangePasswordViewModel());
        }

        [Authorize(Policy = "ChangePassword")]
        [HttpPost("/change-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ViewData["Title"] = "Change Password";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
                return View(model);
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["FormSuccess"] = "Your password has been changed.";
            return RedirectToAction(nameof(ChangePassword));
        }

        // ---------- Forgot Password → Verify Code → Reset Password ----------

        [HttpGet("/forgot-password")]
        public IActionResult ForgotPassword()
        {
            ViewData["Title"] = "Forgot Password";
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost("/forgot-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            ViewData["Title"] = "Forgot Password";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (!userExists)
            {
                ModelState.AddModelError(nameof(model.Email), "No account was found with that email address.");
                return View(model);
            }

            var code = Random.Shared.Next(100000, 999999).ToString();

            var resetCode = new PasswordResetCode
            {
                Email = model.Email,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.PasswordResetCodes.Add(resetCode);
            await _context.SaveChangesAsync();

            var fields = new (string Label, string Value)[]
            {
                ("Verification code", code),
                ("Expires", "10 minutes from now")
            };
            var html = EmailTemplateBuilder.Build(
                heading: "Reset your password",
                intro: "Use the code below to reset your password.",
                fields: fields);
            var plainText = $"Your password reset code is: {code}\nThis code expires in 10 minutes.";

            await _emailService.SendAsync(
                subject: "Your password reset code",
                plainTextBody: plainText,
                replyToEmail: model.Email,
                replyToName: model.Email,
                htmlBody: html,
                recipientEmail: model.Email);
                
    
            return RedirectToAction(nameof(VerifyCode), new { email = model.Email });
        }

        [HttpGet("/verify-code")]
        public IActionResult VerifyCode(string email)
        {
            ViewData["Title"] = "Verify Code";
            return View(new VerifyCodeViewModel { Email = email });
        }

        [HttpPost("/verify-code")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            ViewData["Title"] = "Verify Code";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resetCode = await _context.PasswordResetCodes
                .Where(r => r.Email == model.Email && r.Code == model.Code && !r.IsUsed)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (resetCode == null || resetCode.ExpiresAt < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "That code is invalid or has expired. Please request a new one.");
                return View(model);
            }

            return RedirectToAction(nameof(ResetPassword), new { email = model.Email, code = model.Code });
        }

        [HttpGet("/reset-password")]
        public IActionResult ResetPassword(string email, string code)
        {
            ViewData["Title"] = "Reset Password";
            return View(new ResetPasswordViewModel { Email = email, Code = code });
        }

        [HttpPost("/reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            ViewData["Title"] = "Reset Password";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resetCode = await _context.PasswordResetCodes
                .Where(r => r.Email == model.Email && r.Code == model.Code && !r.IsUsed)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (resetCode == null || resetCode.ExpiresAt < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "That code is invalid or has expired. Please start over.");
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(model);
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
            resetCode.IsUsed = true;

            await _context.SaveChangesAsync();

            TempData["FormSuccess"] = "Your password has been reset. You can now log in.";
            return RedirectToAction(nameof(Login));
        }
    }
}
