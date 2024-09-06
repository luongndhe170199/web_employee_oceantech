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
                .Select(e => new EmployeeViewModel
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    BirthDate = e.BirthDate,
                    Age = e.Age,
                    CitizenId = e.CitizenId,
                    PhoneNumber = e.PhoneNumber,
                    Ethnicity = e.Ethnicity,
                    Occupation = e.Occupation,
                    Position = e.Position,
                    Province = e.Province,
                    District = e.District,
                    Commune = e.Commune,
                    MoreInfo = e.MoreInfo,
                    QualificationCount = e.EmployeeQualifications.Count(q => q.ExpirationDate == null || q.ExpirationDate > DateTime.Now) // Đếm số lượng văn bằng chưa hết hạn
                });

            if (!string.IsNullOrEmpty(searchTerm))
            {
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

            var employeesList = employees.ToList();
            return View(employeesList);
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
            Employee employee = _context.Employees.Include(e => e.EmployeeQualifications).Where(row => row.Id == id).FirstOrDefault();
            // Xóa các văn bằng liên quan đến nhân viên
            _context.EmployeeQualifications.RemoveRange(employee.EmployeeQualifications);
            _context.Employees.Remove(employee);
            _context.SaveChanges();
            return RedirectToAction("ListOfEmployee");
        }

        public ActionResult EmployeeQualifications(int employeeId)
        {
            // Lấy thông tin nhân viên kèm theo danh sách văn bằng
            var employee = _context.Employees
                .Include(e => e.EmployeeQualifications)
                .FirstOrDefault(e => e.Id == employeeId);

            if (employee == null)
            {
                return NotFound(); // Trường hợp không tìm thấy nhân viên
            }
            // Lấy danh sách các tỉnh/thành phố để hiển thị trong dropdown
            ViewBag.Provinces = _context.Provinces.ToList();
            // Trả về view cùng với dữ liệu của nhân viên
            return View(employee);
        }
        
        [HttpPost]
        public ActionResult AddQualification(string QualificationName, DateTime IssueDate, DateTime? ExpirationDate, int EmployeeId, int ProvinceId)
        {
            // Kiểm tra nhân viên có tồn tại không
            var employee = _context.Employees
                .Include(e => e.EmployeeQualifications)
                .FirstOrDefault(e => e.Id == EmployeeId);

            if (employee == null)
            {
                return NotFound(); // Nếu không tìm thấy nhân viên
            }

            // Kiểm tra xem tỉnh/thành phố có hợp lệ không
            var provinceExists = _context.Provinces.Any(p => p.ProvinceId == ProvinceId);
            if (!provinceExists)
            {
                TempData["ErrorMessage"] = "Tỉnh/thành phố không tồn tại.";
                return RedirectToAction("EmployeeQualifications", new { employeeId = EmployeeId });
            }

            // Kiểm tra số lượng văn bằng chưa hết hạn
            var validQualificationsCount = employee.EmployeeQualifications
                .Count(q => q.ExpirationDate == null || q.ExpirationDate > DateTime.Now);

            if (validQualificationsCount >= 3)
            {
                TempData["ErrorMessage"] = "Nhân viên này đã có đủ 3 văn bằng chưa hết hạn. Không thể thêm văn bằng mới.";
                return RedirectToAction("EmployeeQualifications", new { employeeId = EmployeeId });
            }

            // Thêm văn bằng mới
            var newQualification = new EmployeeQualification
            {
                QualificationName = QualificationName,
                IssueDate = IssueDate,
                ExpirationDate = ExpirationDate,
                EmployeeId = EmployeeId,
                ProvinceId = ProvinceId // Đảm bảo rằng ProvinceId đã được kiểm tra là hợp lệ
            };

            _context.EmployeeQualifications.Add(newQualification);
            _context.SaveChanges();

            return RedirectToAction("EmployeeQualifications", new { employeeId = EmployeeId });
        }

        public ActionResult DeleteQualification(int qualificationId, int employeeId)
        {
            var qualification = _context.EmployeeQualifications
                .FirstOrDefault(q => q.Id == qualificationId);

            if (qualification == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy văn bằng để xóa.";
                return RedirectToAction("EmployeeQualifications", new { employeeId = employeeId });
            }

            _context.EmployeeQualifications.Remove(qualification);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Xóa văn bằng thành công.";
            return RedirectToAction("EmployeeQualifications", new { employeeId = employeeId });
        }

        public ActionResult EditQualification(int qualificationId, int employeeId)
        {
            var qualification = _context.EmployeeQualifications
                .FirstOrDefault(q => q.Id == qualificationId);

            if (qualification == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy văn bằng.";
                return RedirectToAction("EmployeeQualifications", new { employeeId = employeeId });
            }

            // Truyền danh sách các tỉnh/thành phố để hiển thị trong dropdown
            ViewBag.Provinces = _context.Provinces.ToList();

            return View(qualification);
        }
        [HttpPost]
        public ActionResult EditQualification(int Id, string QualificationName, DateTime IssueDate, DateTime? ExpirationDate, int ProvinceId, int EmployeeId)
        {
            var qualification = _context.EmployeeQualifications
                .FirstOrDefault(q => q.Id == Id);

            if (qualification == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy văn bằng.";
                return RedirectToAction("EmployeeQualifications", new { employeeId = EmployeeId });
            }

            // Cập nhật thông tin văn bằng
            qualification.QualificationName = QualificationName;
            qualification.IssueDate = IssueDate;
            qualification.ExpirationDate = ExpirationDate;
            qualification.ProvinceId = ProvinceId;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật văn bằng thành công.";
            return RedirectToAction("EmployeeQualifications", new { employeeId = EmployeeId });
        }

    }
}
