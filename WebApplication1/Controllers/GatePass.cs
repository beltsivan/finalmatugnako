using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class GatePass : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GatePass(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult List()
        {
            IEnumerable<GatePassModel> gatepasses = _db.GatePasses.OrderByDescending(g => g.DateSubmitted).ToList();
            return View(gatepasses);
        }
    // Admin list with pagination
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public IActionResult AdminList(int page = 1)
        {
            int pageSize = 5;
            var query = _db.GatePasses.OrderByDescending(g => g.DateSubmitted);
            var total = query.Count();
            var list = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

            if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("AdminList", list);
            }

            return View("AdminList", list);
        }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public IActionResult Approve(int id, bool approve)
        {
            var gp = _db.GatePasses.Find(id);
            if (gp == null) return NotFound();
            gp.Status = approve ? "Approved" : "Rejected";
            _db.SaveChanges();
            if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                int pageSize = 5;
                var query = _db.GatePasses.OrderByDescending(g => g.DateSubmitted);
                var list = query.Take(pageSize).ToList();
                return PartialView("AdminList", list);
            }
            return RedirectToAction("AdminList");
        }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
        {
            var gp = _db.GatePasses.Find(id);
            if (gp == null) return NotFound();

            // remove uploaded files if present
            if (!string.IsNullOrEmpty(gp.OrDocumentPath))
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", gp.OrDocumentPath.TrimStart('/'));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
            if (!string.IsNullOrEmpty(gp.CrDocumentPath))
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", gp.CrDocumentPath.TrimStart('/'));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            _db.GatePasses.Remove(gp);
            _db.SaveChanges();
            if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                int pageSize = 5;
                var query = _db.GatePasses.OrderByDescending(g => g.DateSubmitted);
                var list = query.Take(pageSize).ToList();
                return PartialView("AdminList", list);
            }

            return RedirectToAction("AdminList");
        }
        public IActionResult Create()
        {
            return View("Index");
        }

        // POST: GatePass/Create


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GatePassModel model, IFormFile OrDocumentPath, IFormFile CrDocumentPath)
        {
            // Validate uploaded files
            var allowedExt = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
            const long maxFileSize = 5 * 1024 * 1024; // 5 MB

            if (OrDocumentPath == null || OrDocumentPath.Length == 0)
            {
                ModelState.AddModelError("OrDocumentPath", "Please upload the OR file.");
            }
            else
            {
                var ext = Path.GetExtension(OrDocumentPath.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !allowedExt.Contains(ext))
                    ModelState.AddModelError("OrDocumentPath", "OR file must be .pdf, .jpg, .jpeg or .png");

                if (!allowedTypes.Contains(OrDocumentPath.ContentType) && !OrDocumentPath.ContentType.StartsWith("image/"))
                    ModelState.AddModelError("OrDocumentPath", "Invalid OR file content type.");

                if (OrDocumentPath.Length > maxFileSize)
                    ModelState.AddModelError("OrDocumentPath", "OR file must be 5 MB or less.");
            }

            if (CrDocumentPath == null || CrDocumentPath.Length == 0)
            {
                ModelState.AddModelError("CrDocumentPath", "Please upload the CR file.");
            }
            else
            {
                var ext = Path.GetExtension(CrDocumentPath.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !allowedExt.Contains(ext))
                    ModelState.AddModelError("CrDocumentPath", "CR file must be .pdf, .jpg, .jpeg or .png");

                if (!allowedTypes.Contains(CrDocumentPath.ContentType) && !CrDocumentPath.ContentType.StartsWith("image/"))
                    ModelState.AddModelError("CrDocumentPath", "Invalid CR file content type.");

                if (CrDocumentPath.Length > maxFileSize)
                    ModelState.AddModelError("CrDocumentPath", "CR file must be 5 MB or less.");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            string webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string uploadFolder = Path.Combine(webRoot, "uploads");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // Upload OR file
            if (OrDocumentPath != null && OrDocumentPath.Length > 0)
            {
                string orFileName = Guid.NewGuid().ToString() + Path.GetExtension(OrDocumentPath.FileName);
                string orPath = Path.Combine(uploadFolder, orFileName);
                using (var stream = new FileStream(orPath, FileMode.Create))
                {
                    await OrDocumentPath.CopyToAsync(stream);
                }
                model.OrDocumentPath = "/uploads/" + orFileName;
            }

            // Upload CR file
            if (CrDocumentPath != null && CrDocumentPath.Length > 0)
            {
                string crFileName = Guid.NewGuid().ToString() + Path.GetExtension(CrDocumentPath.FileName);
                string crPath = Path.Combine(uploadFolder, crFileName);
                using (var stream = new FileStream(crPath, FileMode.Create))
                {
                    await CrDocumentPath.CopyToAsync(stream);
                }
                model.CrDocumentPath = "/uploads/" + crFileName;
            }

            model.DateSubmitted = DateTime.Now;
            model.Status ??= "Pending";

            _db.GatePasses.Add(model);
            _db.SaveChanges();
            return RedirectToAction("List");
        }
    }
}
