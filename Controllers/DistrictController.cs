using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System.Linq;

namespace OceanTechLevel1.Controllers
{
    public class DistrictController : Controller
    {
        private readonly Oceantech2Context _context;

        public DistrictController(Oceantech2Context context)
        {
            _context = context;
        }

        public ActionResult Index(string searchTerm)
        {
            var districts = _context.Districts.Include(d => d.Province).AsQueryable();

            // Nếu từ khóa tìm kiếm không rỗng, lọc danh sách các huyện
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower(); // Loại bỏ khoảng trắng ở đầu và cuối, và chuyển về chữ thường

                // Tìm kiếm theo tên huyện hoặc tên tỉnh
                districts = districts.Where(d => d.DistrictName.Trim().ToLower().Contains(searchTerm) ||
                                                 d.Province.ProvinceName.Trim().ToLower().Contains(searchTerm));
            }

            return View(districts.ToList());
        }



        public ActionResult Create()
        {
            ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName");
            return View();
        }

        [HttpPost]
        public ActionResult Create(District d)
        {
            if (ModelState.IsValid)
            {
                _context.Districts.Add(d);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", d.ProvinceId);
            return View(d);
        }

        public ActionResult Edit(int id)
        {
            var district = _context.Districts.Include(d => d.Province).FirstOrDefault(d => d.DistrictId == id);
            if (district == null)
            {
                return NotFound();
            }
            ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", district.ProvinceId);
            return View(district);
        }

        [HttpPost]
        public ActionResult Edit(District d)
        {
            if (ModelState.IsValid)
            {
                _context.Districts.Update(d);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", d.ProvinceId);
            return View(d);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var district = _context.Districts.Include(d => d.Communes).Include(d => d.Employees).FirstOrDefault(d => d.DistrictId == id);
            if (district == null)
            {
                return NotFound();
            }

            // Xóa tất cả các xã liên quan
            _context.Communes.RemoveRange(district.Communes);

            // Xóa tất cả các nhân viên liên quan
            _context.Employees.RemoveRange(district.Employees);

            // Xóa huyện
            _context.Districts.Remove(district);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
