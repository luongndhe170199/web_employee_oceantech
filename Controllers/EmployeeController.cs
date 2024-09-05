using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System.Linq;
using System.Text.Json;

namespace OceanTechLevel1.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly Oceantech2Context _context;

        public EmployeeController(Oceantech2Context context)
        {
            _context = context;
        }

        private void PopulateViewBags()
        {
            ViewBag.Ethnicities = new SelectList(_context.Ethnicities, "EthnicityId", "EthnicityName");
            ViewBag.Occupations = new SelectList(_context.Occupations, "OccupationId", "OccupationName");
            ViewBag.Positions = new SelectList(_context.Positions, "PositionId", "PositionName");
            ViewBag.Provinces = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName");
            ViewBag.Districts = _context.Districts.ToList();  // hoặc
            ViewBag.Communes = _context.Communes.ToList();

            var districtsJson = JsonSerializer.Serialize(_context.Districts
                .Select(d => new { d.DistrictId, d.DistrictName, d.ProvinceId }).ToList(),
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

            var communesJson = JsonSerializer.Serialize(_context.Communes
                .Select(c => new { c.CommuneId, c.CommuneName, c.DistrictId }).ToList(),
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

            ViewBag.DistrictsJson = districtsJson;
            ViewBag.CommunesJson = communesJson;
        }


        public ActionResult Create()
        {
           PopulateViewBags();

            return View();
        }

        [HttpPost]
        public ActionResult Create(Employee p)
        {
            if (string.IsNullOrEmpty(p.CitizenId))
            {
                ModelState.AddModelError("CitizenId", "CCCD không được để trống.");
                return View(p);
            }

            // Kiểm tra nếu CCCD đã tồn tại
            var existingEmployee = _context.Employees
                                           .FirstOrDefault(e => e.CitizenId == p.CitizenId.Trim());
            if (existingEmployee != null)
            {
                ModelState.AddModelError("CitizenId", "Số CCCD đã tồn tại.");
                PopulateViewBags(); // Gọi hàm để thiết lập lại ViewBag
                return View(p); // Trả về view với thông báo lỗi
            }

            _context.Employees.Add(p);
            _context.SaveChanges();
            return RedirectToAction("ListOfEmployee");
        }


        public ActionResult ListOfEmployee(string searchTerm)
        {
            var employees = _context.Employees
                .Include(e => e.Ethnicity)
                .Include(e => e.Occupation)
                .Include(e => e.Position)
                .Include(e => e.Province)
                .Include(e => e.District)
                .Include(e => e.Commune)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Loại bỏ khoảng trắng ở đầu và cuối chuỗi tìm kiếm
                searchTerm = searchTerm.Trim().ToLower();

                employees = employees.Where(e => e.FullName.Trim().ToLower().Contains(searchTerm) ||
                                                 e.CitizenId.Trim().ToLower().Contains(searchTerm) ||
                                                 (e.PhoneNumber != null && e.PhoneNumber.Trim().ToLower().Contains(searchTerm)) ||
                                                 (e.Ethnicity != null && e.Ethnicity.EthnicityName.Trim().ToLower().Contains(searchTerm)) ||
                                                 (e.Occupation != null && e.Occupation.OccupationName.Trim().ToLower().Contains(searchTerm)) ||
                                                 (e.Position != null && e.Position.PositionName.Trim().ToLower().Contains(searchTerm)) ||
                                                 (e.Province != null && e.Province.ProvinceName.Trim().ToLower().Contains(searchTerm)) ||
                                                 (e.District != null && e.District.DistrictName.Trim().ToLower().Contains(searchTerm)) ||
                                                 (e.Commune != null && e.Commune.CommuneName.Trim().ToLower().Contains(searchTerm)));
            }

            return View(employees.ToList());
        }

        public ActionResult Edit(int id)
        {
            Employee employee = _context.Employees.Where(row => row.Id == id).FirstOrDefault();

            PopulateViewBags();

            return View(employee);
        }

        [HttpPost]
        public ActionResult Edit(Employee emp)
        {
            Employee employee = _context.Employees.Where(row => row.Id == emp.Id).FirstOrDefault();

            // Kiểm tra nếu CCCD đã tồn tại trong hệ thống và không phải của nhân viên hiện tại
            var existingEmployee = _context.Employees
                                           .FirstOrDefault(e => e.CitizenId == emp.CitizenId && e.Id != emp.Id);

            if (existingEmployee != null)
            {
                ModelState.AddModelError("CitizenId", "Số CCCD đã tồn tại.");

                // Thiết lập lại ViewBag để các dropdown tiếp tục hiển thị đúng
                PopulateViewBags();

                // Trả về view với thông báo lỗi
                return View(emp);
            }
            // update
            employee.FullName = emp.FullName;
            employee.BirthDate = emp.BirthDate;
            employee.Age = emp.Age;
            employee.EthnicityId = emp.EthnicityId;
            employee.CitizenId = emp.CitizenId;
            employee.OccupationId = emp.OccupationId;
            employee.PositionId = emp.PositionId;
            employee.PhoneNumber = emp.PhoneNumber;
            employee.ProvinceId = emp.ProvinceId;
            employee.DistrictId = emp.DistrictId;
            employee.CommuneId = emp.CommuneId;
            employee.MoreInfo = emp.MoreInfo;

            _context.SaveChanges();
            return RedirectToAction("ListOfEmployee");
        }

        public ActionResult Delete(int id)
        {
            Employee employee = _context.Employees.Where(row => row.Id == id).FirstOrDefault();
            return View(employee);
        }
        [HttpPost]
        public ActionResult Delete(int id,Employee e)
        {
            Employee employee = _context.Employees.Where(row => row.Id == id).FirstOrDefault();
            _context.Employees.Remove(employee);
            _context.SaveChanges();
            return RedirectToAction("ListOfEmployee");
        }

    }
}
