using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System.Linq;
using System.Text.Json;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using OceanTechLevel1.Services;
namespace OceanTechLevel1.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly Oceantech2Context _context;
        private readonly EmployeeService _employeeService;
        private readonly ExportImportService _exportImportService;
        private readonly QualificationService _qualificationService;
        private readonly ViewBagService _viewBagService;

        public EmployeeController(Oceantech2Context context, EmployeeService employeeService, ExportImportService exportImportService, QualificationService qualificationService, ViewBagService viewBagService)
        {
            _context = context;
            _employeeService = employeeService;
            _exportImportService = exportImportService;
            _qualificationService = qualificationService;
            _viewBagService = viewBagService;
        }

        private void PopulateViewBags()
        {
            _viewBagService.PopulateViewBags(this);
        }


        public ActionResult Create()
        {
            PopulateViewBags();
            return View();
        }

        [HttpPost]
        public ActionResult Create(Employee p)
        {
            if (Request.Form["NoPhoneNumber"] == "on")
            {
                p.PhoneNumber = null;
            }

            if (string.IsNullOrEmpty(p.CitizenId))
            {
                ModelState.AddModelError("CitizenId", "CCCD không được để trống.");
                return View(p);
            }

            var existingEmployee = _employeeService.GetEmployees(p.CitizenId.Trim()).FirstOrDefault();
            if (existingEmployee != null)
            {
                ModelState.AddModelError("CitizenId", "Số CCCD đã tồn tại.");
                PopulateViewBags();
                return View(p);
            }

            _employeeService.AddEmployee(p);
            return RedirectToAction("ListOfEmployee");
        }

        public ActionResult ListOfEmployee(string searchTerm, int page = 1)
        {
            // Tìm kiếm dựa trên từ khóa
            var employees = _employeeService.GetEmployees(searchTerm).ToList();

            // Paging 
            int NoOfRecordPerPage = 10;
            int NoOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(employees.Count) / Convert.ToDouble(NoOfRecordPerPage)));
            int NoOfRecordToSkip = (page - 1) * NoOfRecordPerPage;

            ViewBag.Page = page;
            ViewBag.NoOfPages = NoOfPages;
            ViewBag.SearchTerm = searchTerm; // Truyền searchTerm để sử dụng lại trong view

            // Lấy danh sách nhân viên cho trang hiện tại
            employees = employees.Skip(NoOfRecordToSkip).Take(NoOfRecordPerPage).ToList();

            return View(employees);
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

            // Kiểm tra nếu người dùng chọn checkbox "Không có SĐT"
            if (Request.Form["NoPhoneNumber"] == "on")
            {
                emp.PhoneNumber = null; // Đặt giá trị null nếu chọn "Không có SĐT"
            }
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
            _employeeService.UpdateEmployee(emp);

            _context.SaveChanges();
            return RedirectToAction("ListOfEmployee");
        }

        public ActionResult Delete(int id)
        {
            Employee employee = _context.Employees.Where(row => row.Id == id).FirstOrDefault();
            return View(employee);
        }
        [HttpPost]
        public ActionResult Delete(int id, Employee e)
        {
            _employeeService.DeleteEmployee(id);
            return RedirectToAction("ListOfEmployee");
        }

        public ActionResult EmployeeQualifications(int employeeId)
        {
            // Lấy thông tin nhân viên kèm theo danh sách văn bằng
            var employee = _qualificationService.GetEmployeeWithQualifications(employeeId);

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
            var employee = _qualificationService.GetEmployeeWithQualifications(EmployeeId);

            if (employee == null)
            {
                return NotFound();
            }

            if (!_qualificationService.ProvinceExists(ProvinceId))
            {
                TempData["ErrorMessage"] = "Tỉnh/thành phố không tồn tại.";
                return RedirectToAction("EmployeeQualifications", new { employeeId = EmployeeId });
            }

            var validQualificationsCount = _qualificationService.GetValidQualificationsCount(employee);

            if (validQualificationsCount >= 3)
            {
                TempData["ErrorMessage"] = "Nhân viên này đã có đủ 3 văn bằng chưa hết hạn. Không thể thêm văn bằng mới.";
                return RedirectToAction("EmployeeQualifications", new { employeeId = EmployeeId });
            }

            var newQualification = new EmployeeQualification
            {
                QualificationName = QualificationName,
                IssueDate = IssueDate,
                ExpirationDate = ExpirationDate,
                EmployeeId = EmployeeId,
                ProvinceId = ProvinceId
            };

            _qualificationService.AddQualification(newQualification);

            return RedirectToAction("EmployeeQualifications", new { employeeId = EmployeeId });
        }

        public ActionResult DeleteQualification(int qualificationId, int employeeId)
        {
            var qualification = _qualificationService.GetQualificationById(qualificationId);

            if (qualification == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy văn bằng để xóa.";
                return RedirectToAction("EmployeeQualifications", new { employeeId = employeeId });
            }

            _qualificationService.DeleteQualification(qualification);

            TempData["SuccessMessage"] = "Xóa văn bằng thành công.";
            return RedirectToAction("EmployeeQualifications", new { employeeId = employeeId });
        }

        public ActionResult EditQualification(int qualificationId, int employeeId)
        {
            var qualification = _qualificationService.GetQualificationById(qualificationId);

            if (qualification == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy văn bằng.";
                return RedirectToAction("EmployeeQualifications", new { employeeId = employeeId });
            }

            ViewBag.Provinces = _qualificationService.GetAllProvinces();

            return View(qualification);
        }

        [HttpPost]
        public ActionResult EditQualification(int Id, string QualificationName, DateTime IssueDate, DateTime? ExpirationDate, int ProvinceId, int EmployeeId)
        {
            var qualification = _qualificationService.GetQualificationById(Id);

            if (qualification == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy văn bằng.";
                return RedirectToAction("EmployeeQualifications", new { employeeId = EmployeeId });
            }

            qualification.QualificationName = QualificationName;
            qualification.IssueDate = IssueDate;
            qualification.ExpirationDate = ExpirationDate;
            qualification.ProvinceId = ProvinceId;

            _qualificationService.UpdateQualification(qualification);

            TempData["SuccessMessage"] = "Cập nhật văn bằng thành công.";
            return RedirectToAction("EmployeeQualifications", new { employeeId = EmployeeId });
        }

        [HttpPost]
        public ActionResult ExportSelectedEmployees(int[] selectedEmployees)
        {
            var employees = _exportImportService.GetSelectedEmployees(selectedEmployees);
            var stream = _exportImportService.ExportEmployeesToExcel(employees);

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "employees.xlsx");
        }

        public ActionResult ExportEmployees(string searchTerm)
        {
            var employees = _exportImportService.GetFilteredEmployees(searchTerm);
            var stream = _exportImportService.ExportEmployeesToExcel(employees);

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "filtered-employees.xlsx");
        }
        public ActionResult Import()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Import(IFormFile employeeFile)
        {
            if (employeeFile == null || employeeFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một file Excel hợp lệ.";
                return RedirectToAction("Import");
            }

            var errors = _exportImportService.ImportEmployeesFromExcel(employeeFile);

            if (errors.Any())
            {
                TempData["ErrorMessage"] = string.Join("<br/>", errors);
                return RedirectToAction("Import");
            }
            else
            {
                TempData["SuccessMessage"] = "Import thành công!";
            }

            return RedirectToAction("ListOfEmployee");
        }
    }
}
