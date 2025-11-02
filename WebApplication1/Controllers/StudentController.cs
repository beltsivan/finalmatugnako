using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
namespace MyWebApplication.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _db;
        public StudentController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Student> objStudentList = _db.Students;
            //var objStudentList = _db.Students.ToList();
            return View(objStudentList);
        }
        //GET
        public IActionResult Create()
        {
            return View();
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Student obj)
        {
            if (ModelState.IsValid)
            {
                _db.Students.Add(obj);
                _db.SaveChanges();
                TempData["success"] = "Student created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //GET
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var student = _db.Students.Find(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Student student)
        {
            if (ModelState.IsValid)
            {
                _db.Students.Update(student);
                _db.SaveChanges();
                TempData["success"] = "Student updated successfully";
                return RedirectToAction("Index");
            }
            return View(student);
        }

        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var student = _db.Students.Find(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var student = await _db.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            try
            {
                _db.Students.Remove(student);
                await _db.SaveChangesAsync();
                
                // Resequence the IDs to maintain order
                await _db.ResequenceStudentIds();
                
                TempData["success"] = "Student deleted successfully";
            }
            catch (Exception)
            {
                TempData["error"] = "Error deleting student";
            }
            
            return RedirectToAction("Index");
        }
    }
}