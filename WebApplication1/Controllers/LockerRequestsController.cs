using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    public class LockerRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LockerRequestsController> _logger;

        public LockerRequestsController(ApplicationDbContext context, IWebHostEnvironment env, ILogger<LockerRequestsController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        // GET: /LockerRequests/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /LockerRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LockerRequest model, IFormFile registrationFile)
        {
            _logger.LogInformation("Create action started with Name: {Name}, IdNumber: {IdNumber}", model.Name, model.IdNumber);

            // Log all form values for debugging
            foreach (var prop in model.GetType().GetProperties())
            {
                _logger.LogInformation("Property {Property}: {Value}", prop.Name, prop.GetValue(model));
            }

            // Only validate the essential fields
            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "Name is required");
            }
            if (string.IsNullOrEmpty(model.IdNumber))
            {
                ModelState.AddModelError(nameof(model.IdNumber), "Student ID is required");
            }
            if (!model.TermsAccepted)
            {
                ModelState.AddModelError(nameof(model.TermsAccepted), "You must accept the terms");
            }
            if (registrationFile == null || registrationFile.Length == 0)
            {
                ModelState.AddModelError("registrationFile", "Please upload your registration file");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // save file
            if (registrationFile != null && registrationFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid() + Path.GetExtension(registrationFile.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                    await registrationFile.CopyToAsync(fs);

                model.RegistrationFilePath = "/uploads/" + fileName;
                Console.WriteLine("Form submitted to Create action!");

            }

            model.CreatedAt = DateTime.UtcNow;
            model.Approver = null; // No approver for new requests
            try
            {
                // Check if this request already exists
                var existing = await _context.LockerRequests
                    .FirstOrDefaultAsync(r => r.Name == model.Name &&
                                            r.IdNumber == model.IdNumber &&
                                            r.CreatedAt > DateTime.UtcNow.AddMinutes(-1));

                if (existing != null)
                {
                    _logger.LogWarning("Duplicate submission detected for {Name}", model.Name);
                    return RedirectToAction("Submitted");
                }

                _context.Add(model);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully saved locker request for {Name}", model.Name);
                return RedirectToAction("Submitted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving locker request for {Name}", model.Name);
                ModelState.AddModelError("", "An error occurred while saving your request. Please try again.");
                return View(model);
            }
        }

        // simple submitted confirmation
        public IActionResult Submitted() => View();

        // Public list of locker requests (for users to view their requests/status)
        // Optional idNumber filter: ?idNumber=12345
    public IActionResult List(string? idNumber = null)
        {
            var query = _context.LockerRequests.AsQueryable();
            if (!string.IsNullOrEmpty(idNumber))
            {
                query = query.Where(r => r.IdNumber == idNumber);
            }

            var list = query.OrderByDescending(r => r.CreatedAt).ToList();
            return View(list);
        }

    // Admin list page with pagination
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public IActionResult Admin(int page = 1)
        {
            int pageSize = 5;
            var query = _context.LockerRequests.OrderByDescending(x => x.CreatedAt);
            var totalRecords = query.Count();
            var list = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Return partial for AJAX requests
            if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Admin", list);
            }

            return View(list);
        }

        // Approve/Reject action
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, bool approve)
        {
            var req = await _context.LockerRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Approved = approve;
            req.Approver = "Shewakram";
            req.ApprovalDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // If this was an AJAX request, return the admin partial so the panel can update in-place
            if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                int pageSize = 5;
                var query = _context.LockerRequests.OrderByDescending(x => x.CreatedAt);
                var list = query.Take(pageSize).ToList();
                return PartialView("Admin", list);
            }

            return RedirectToAction(nameof(Admin));
        }

        // Delete action
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.LockerRequests.FindAsync(id);
            if (request == null) return NotFound();

            // Delete the file if it exists
            if (!string.IsNullOrEmpty(request.RegistrationFilePath))
            {
                var filePath = Path.Combine(_env.WebRootPath, request.RegistrationFilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.LockerRequests.Remove(request);
            await _context.SaveChangesAsync();

            if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                int pageSize = 5;
                var query = _context.LockerRequests.OrderByDescending(x => x.CreatedAt);
                var list = query.Take(pageSize).ToList();
                return PartialView("Admin", list);
            }

            return RedirectToAction(nameof(Admin));
        }
    }
}
