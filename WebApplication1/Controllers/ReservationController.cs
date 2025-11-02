using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ReservationController : Controller
    {
        public readonly ApplicationDbContext _db;
        public ReservationController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult ResUI(int page = 1)
        {
            int pageSize = 5; // Limit to 5 rows per page
            var reservations = _db.Reservations
                                   .OrderBy(r => r.Id)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

            int totalRecords = _db.Reservations.Count();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return View(reservations);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(Reservation obj)
        {
            if (ModelState.IsValid)
            {
                _db.Reservations.Add(obj);
                _db.SaveChanges();
                return RedirectToAction("ResUI");
            }
            return View();
        }
        
    }
}
