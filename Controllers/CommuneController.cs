using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System.Linq;

namespace OceanTechLevel1.Controllers
{
    public class CommuneController : Controller
    {
        private readonly Oceantech2Context _context;

        public CommuneController(Oceantech2Context context)
        {
            _context = context;
        }

        public ActionResult Index(string searchTerm)
        {
            var communes = _context.Communes
                                   .Include(c => c.District)
                                   .ThenInclude(d => d.Province)
                                   .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Sử dụng Trim() để loại bỏ khoảng trắng ở đầu và cuối chuỗi
                searchTerm = searchTerm.Trim().ToLower();

                communes = communes.Where(c => c.CommuneName.Trim().ToLower().Contains(searchTerm) ||
                                                c.District.DistrictName.Trim().ToLower().Contains(searchTerm) ||
                                                c.District.Province.ProvinceName.Trim().ToLower().Contains(searchTerm));
            }

            return View(communes.ToList());
        }



        public ActionResult Create()
        {
            ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName");
            return View();
        }

        [HttpPost]
        public ActionResult Create(Commune commune)
        {
            if (ModelState.IsValid)
            {
                _context.Communes.Add(commune);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
            return View(commune);
        }

        public ActionResult Edit(int id)
        {
            var commune = _context.Communes.Include(c => c.District).ThenInclude(d => d.Province).FirstOrDefault(c => c.CommuneId == id);
            if (commune == null)
            {
                return NotFound();
            }
            ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
            return View(commune);
        }

        [HttpPost]
        public ActionResult Edit(Commune commune)
        {
            if (ModelState.IsValid)
            {
                _context.Communes.Update(commune);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
            return View(commune);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var commune = _context.Communes.Include(c => c.Employees).FirstOrDefault(c => c.CommuneId == id);
            if (commune == null)
            {
                return NotFound();
            }

            // Xóa tất cả các nhân viên liên quan
            _context.Employees.RemoveRange(commune.Employees);

            // Xóa xã
            _context.Communes.Remove(commune);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
