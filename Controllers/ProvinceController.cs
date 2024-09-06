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

        public ActionResult Index(string searchTerm)
        {
            var provinces = _context.Provinces.AsQueryable();

            // Nếu từ khóa tìm kiếm không rỗng, lọc danh sách các tỉnh
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim(); // Chuyển từ khóa tìm kiếm thành chữ thường và loại bỏ khoảng trắng
                provinces = provinces.Where(p => p.ProvinceName.ToLower().Contains(searchTerm));
            }

            return View(provinces.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(Province p)
        {
            if (!ModelState.IsValid)
            {
                // Nếu Model không hợp lệ (ví dụ như bỏ trống tên tỉnh), trả về view với thông báo lỗi
                return View(p);
            }
            // Kiểm tra nếu tên tỉnh đã tồn tại
            var existingProvince = _context.Provinces
                                           .FirstOrDefault(province => province.ProvinceName.ToLower().Trim() == p.ProvinceName.ToLower().Trim());

            if (existingProvince != null)
            {
                // Nếu tên tỉnh đã tồn tại, thêm thông báo lỗi và trả về view
                ModelState.AddModelError("ProvinceName", "Tên tỉnh đã tồn tại.");
                return View(p);
            }

            // Nếu không có lỗi, thêm tỉnh mới vào database
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
            if (!ModelState.IsValid)
            {
                // Nếu Model không hợp lệ (ví dụ như bỏ trống tên tỉnh), trả về view với thông báo lỗi
                return View(pro);
            }
            Province province = _context.Provinces.Where(row => row.ProvinceId == pro.ProvinceId).FirstOrDefault();
            // Kiểm tra nếu tên tỉnh đã tồn tại
            var existingProvince = _context.Provinces
                                           .FirstOrDefault(province => province.ProvinceName.ToLower().Trim() == pro.ProvinceName.ToLower().Trim());

            if (existingProvince != null)
            {
                // Nếu tên tỉnh đã tồn tại, thêm thông báo lỗi và trả về view
                ModelState.AddModelError("ProvinceName", "Tên tỉnh đã tồn tại.");
                return View(pro);
            }

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

            // Xóa tất cả các văn bằng liên quan đến tỉnh
            var qualificationsInProvince = _context.EmployeeQualifications
                                                   .Where(eq => eq.ProvinceId == id)
                                                   .ToList();
            _context.EmployeeQualifications.RemoveRange(qualificationsInProvince);
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
