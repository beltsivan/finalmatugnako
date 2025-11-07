using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebApplication1.Data.ApplicationDbContext _db;

        public AccountController(WebApplication1.Data.ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
    public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Admin (hardcoded for now)
                if (model.Username == "shewakram" && model.Password == "123")
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Admin", "LockerRequests");
                }

                // Student login: validate username + hashed password
                var student = _db.Students.FirstOrDefault(s => s.Username == model.Username);
                if (student != null && !string.IsNullOrEmpty(student.PasswordHash))
                {
                    var hasher = new PasswordHasher<Student>();
                    var verify = hasher.VerifyHashedPassword(student, student.PasswordHash, model.Password);
                    if (verify == PasswordVerificationResult.Success)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, student.Firstname + " " + student.Lastname),
                            new Claim(ClaimTypes.Role, "Student"),
                            new Claim("StudentId", student.Id.ToString())
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity));

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        // After successful login show welcome page
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt. If you don't have an account, please sign up.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(WebApplication1.Models.RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check uniqueness by username and email
            if (!string.IsNullOrEmpty(model.Username) && _db.Students.Any(s => s.Username == model.Username))
            {
                ModelState.AddModelError("Username", "An account with this username already exists.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.Email) && _db.Students.Any(s => s.Email == model.Email))
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            var student = new WebApplication1.Models.Student
            {
                Firstname = model.Firstname ?? string.Empty,
                Lastname = model.Lastname ?? string.Empty,
                Email = model.Email,
                Course = model.Course,
                DateCreated = DateTime.UtcNow,
                Username = model.Username ?? string.Empty,
                SchoolIdNumber = model.SchoolIdNumber,
                Age = model.Age
            };

            var hasher = new PasswordHasher<Student>();
            student.PasswordHash = hasher.HashPassword(student, model.Password ?? string.Empty);

            _db.Students.Add(student);
            await _db.SaveChangesAsync();

            // After signup, direct user to login page
            TempData["RegisterSuccess"] = "Account created successfully. Please log in.";
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}