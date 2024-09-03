using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System.Linq;

namespace OceanTechLevel1.Controllers
{
    public class ProvinceController : Controller
    {
        private readonly Oceantech2Context _context;

        public ProvinceController(Oceantech2Context context)
        {
            _context = context;
        }

        public ActionResult Index()
        {
            var provinces = _context.Provinces.ToList();
            return View(provinces);
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(Province p)
        {
            _context.Provinces.Add(p);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Edit(int id)
        {
            Province province = _context.Provinces.Where(row=>row.ProvinceId == id).FirstOrDefault();
            return View(province);
        }
        [HttpPost]
        public ActionResult Edit(Province pro)
        {
            Province province = _context.Provinces.Where(row => row.ProvinceId == pro.ProvinceId).FirstOrDefault();

            province.ProvinceName=pro.ProvinceName;
           _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            // Tìm tỉnh và bao gồm các huyện và xã liên quan
            var province = _context.Provinces
                                   .Include(p => p.Districts)
                                   .ThenInclude(d => d.Communes)
                                   .FirstOrDefault(p => p.ProvinceId == id);

            if (province == null)
            {
                return NotFound();
            }

            // Xóa các bản ghi Employee liên quan đến Commune, District và Province
            var employeesInCommunes = _context.Employees
                                              .Where(e => e.CommuneId.HasValue &&
                                                          _context.Communes
                                                                  .Any(c => c.CommuneId == e.CommuneId.Value &&
                                                                            _context.Districts
                                                                                    .Any(d => d.DistrictId == c.DistrictId &&
                                                                                              _context.Provinces
                                                                                                      .Any(p => p.ProvinceId == d.ProvinceId &&
                                                                                                                p.ProvinceId == id))))
                                              .ToList();
            _context.Employees.RemoveRange(employeesInCommunes);

            // Xóa tất cả các xã liên quan
            foreach (var district in province.Districts)
            {
                _context.Communes.RemoveRange(district.Communes);
            }

            // Xóa tất cả các huyện liên quan
            _context.Districts.RemoveRange(province.Districts);

            // Xóa tỉnh
            _context.Provinces.Remove(province);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
