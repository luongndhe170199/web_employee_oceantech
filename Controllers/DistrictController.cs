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

        public ActionResult Index(string searchTerm, int page = 1)
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
            var districtList= districts.ToList();
            // Paging 
            int NoOfRecordPerPage = 5;
            int NoOfPages = Convert.ToInt32(Math.Ceiling
                (Convert.ToDouble(districtList.Count) / Convert.ToDouble
                (NoOfRecordPerPage)));

            int NoOfRecordToSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPages = NoOfPages;
            districtList = districtList.Skip(NoOfRecordToSkip).Take(NoOfRecordPerPage).ToList();
            return View(districtList);
        }



        public ActionResult Create()
        {
            ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName");
            return View();
        }

        [HttpPost]
        public ActionResult Create(District d)
        {
            // Kiểm tra nếu Model hợp lệ
            if (!ModelState.IsValid)
            {
                // Hiển thị lại form với dữ liệu đã nhập nếu Model không hợp lệ
                ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", d.ProvinceId);
                return View(d);
            }

            // Kiểm tra tên huyện có trùng lặp trong cùng một tỉnh không
            var existingDistrict = _context.Districts
                .FirstOrDefault(district => district.DistrictName.ToLower().Trim() == d.DistrictName.ToLower().Trim()
                                            && district.ProvinceId == d.ProvinceId);
            if (existingDistrict != null)
            {
                // Thêm thông báo lỗi nếu tên huyện đã tồn tại
                ModelState.AddModelError("DistrictName", "Tên huyện đã tồn tại trong tỉnh này.");
                ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", d.ProvinceId);
                return View(d);
            }

            // Thêm huyện mới vào database nếu không có lỗi
            _context.Districts.Add(d);
            _context.SaveChanges();
            return RedirectToAction("Index");
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
            // Kiểm tra nếu Model hợp lệ
            if (!ModelState.IsValid)
            {
                ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", d.ProvinceId);
                return View(d);
            }

            // Kiểm tra tên huyện có trùng lặp trong cùng một tỉnh không (trừ huyện hiện tại đang chỉnh sửa)
            var existingDistrict = _context.Districts
                .FirstOrDefault(district => district.DistrictName.ToLower().Trim() == d.DistrictName.ToLower().Trim()
                                            && district.ProvinceId == d.ProvinceId
                                            && district.DistrictId != d.DistrictId);
            if (existingDistrict != null)
            {
                // Thêm thông báo lỗi nếu tên huyện đã tồn tại
                ModelState.AddModelError("DistrictName", "Tên huyện đã tồn tại trong tỉnh này.");
                ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", d.ProvinceId);
                return View(d);
            }

            // Cập nhật huyện trong database nếu không có lỗi
            _context.Districts.Update(d);
            _context.SaveChanges();
            return RedirectToAction("Index");
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
