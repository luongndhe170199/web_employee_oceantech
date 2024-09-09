using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System.Linq;
using System.Text.Json;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
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


        public ActionResult ListOfEmployee(string searchTerm, int page=1)
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
            // Paging 
            int NoOfRecordPerPage = 10;
            int NoOfPages = Convert.ToInt32(Math.Ceiling
                (Convert.ToDouble(employeesList.Count) / Convert.ToDouble
                (NoOfRecordPerPage)));

            int NoOfRecordToSkip = (page -1)*NoOfRecordPerPage ;
            ViewBag.Page = page;
            ViewBag.NoOfPages=NoOfPages;
            employeesList=employeesList.Skip(NoOfRecordToSkip).Take(NoOfRecordPerPage).ToList();

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

        [HttpPost]
        public ActionResult ExportSelectedEmployees(int[] selectedEmployees)
        {
            var employees = _context.Employees
                .Where(e => selectedEmployees.Contains(e.Id))
                .Select(e => new
                {
                    e.Id,
                    e.FullName,
                    e.CitizenId,
                    e.PhoneNumber,
                    e.BirthDate,
                    e.Age,
                    Ethnicity = e.Ethnicity.EthnicityName,
                    Occupation = e.Occupation.OccupationName,
                    Position = e.Position.PositionName,
                    Province = e.Province.ProvinceName,
                    District = e.District.DistrictName,
                    Commune = e.Commune.CommuneName,
                    QualificationCount = e.EmployeeQualifications.Count(q => q.ExpirationDate == null || q.ExpirationDate > DateTime.Now)
                }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Employees");

                // Thêm tiêu đề cho các cột
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Full Name";
                worksheet.Cell(1, 3).Value = "Birth Date";
                worksheet.Cell(1, 4).Value = "Age";
                worksheet.Cell(1, 5).Value = "Ethnicity";
                worksheet.Cell(1, 6).Value = "Occupation";
                worksheet.Cell(1, 7).Value = "Position";
                worksheet.Cell(1, 8).Value = "Citizen ID";
                worksheet.Cell(1, 9).Value = "Phone Number";
                worksheet.Cell(1, 10).Value = "Province";
                worksheet.Cell(1, 11).Value = "District";
                worksheet.Cell(1, 12).Value = "Commune";
                worksheet.Cell(1, 13).Value = "Qualification Count";

                // Thêm dữ liệu vào các dòng
                for (int i = 0; i < employees.Count; i++)
                {
                    var employee = employees[i];
                    worksheet.Cell(i + 2, 1).Value = employee.Id;
                    worksheet.Cell(i + 2, 2).Value = employee.FullName;
                    worksheet.Cell(i + 2, 3).Value = employee.BirthDate.ToString("dd/MM/yyyy");
                    worksheet.Cell(i + 2, 4).Value = employee.Age;
                    worksheet.Cell(i + 2, 5).Value = employee.Ethnicity;
                    worksheet.Cell(i + 2, 6).Value = employee.Occupation;
                    worksheet.Cell(i + 2, 7).Value = employee.Position;
                    worksheet.Cell(i + 2, 8).Value = employee.CitizenId;
                    worksheet.Cell(i + 2, 9).Value = employee.PhoneNumber;
                    worksheet.Cell(i + 2, 10).Value = employee.Province;
                    worksheet.Cell(i + 2, 11).Value = employee.District;
                    worksheet.Cell(i + 2, 12).Value = employee.Commune;
                    worksheet.Cell(i + 2, 13).Value = employee.QualificationCount;
                }

                // Trả về file Excel
                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "employees.xlsx");
                }
            }
        }

        public ActionResult ExportEmployees(string searchTerm)
        {
            var employees = _context.Employees
                .Include(e => e.Ethnicity)
                .Include(e => e.Occupation)
                .Include(e => e.Position)
                .Include(e => e.Province)
                .Include(e => e.District)
                .Include(e => e.Commune)
                .Select(e => new
                {
                    e.Id,
                    e.FullName,
                    e.BirthDate,
                    e.Age,
                    e.CitizenId,
                    e.PhoneNumber,
                    Ethnicity = e.Ethnicity.EthnicityName,
                    Occupation = e.Occupation.OccupationName,
                    Position = e.Position.PositionName,
                    Province = e.Province.ProvinceName,
                    District = e.District.DistrictName,
                    Commune = e.Commune.CommuneName,
                    QualificationCount = e.EmployeeQualifications.Count(q => q.ExpirationDate == null || q.ExpirationDate > DateTime.Now)
                });

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                employees = employees.Where(e => e.FullName.ToLower().Contains(searchTerm) ||
                                                 e.CitizenId.ToLower().Contains(searchTerm) ||
                                                 (e.PhoneNumber != null && e.PhoneNumber.ToLower().Contains(searchTerm)) ||
                                                 (e.Ethnicity != null && e.Ethnicity.ToLower().Contains(searchTerm)) ||
                                                 (e.Occupation != null && e.Occupation.ToLower().Contains(searchTerm)) ||
                                                 (e.Position != null && e.Position.ToLower().Contains(searchTerm)) ||
                                                 (e.Province != null && e.Province.ToLower().Contains(searchTerm)) ||
                                                 (e.District != null && e.District.ToLower().Contains(searchTerm)) ||
                                                 (e.Commune != null && e.Commune.ToLower().Contains(searchTerm)));
            }

            var employeesList = employees.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Employees");

                // Thêm tiêu đề cho các cột
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Full Name";
                worksheet.Cell(1, 3).Value = "Birth Date";
                worksheet.Cell(1, 4).Value = "Age";
                worksheet.Cell(1, 5).Value = "Ethnicity";
                worksheet.Cell(1, 6).Value = "Occupation";
                worksheet.Cell(1, 7).Value = "Position";
                worksheet.Cell(1, 8).Value = "Citizen ID";
                worksheet.Cell(1, 9).Value = "Phone Number";
                worksheet.Cell(1, 10).Value = "Province";
                worksheet.Cell(1, 11).Value = "District";
                worksheet.Cell(1, 12).Value = "Commune";
                worksheet.Cell(1, 13).Value = "Qualification Count";
                

                // Thêm dữ liệu vào các dòng
                for (int i = 0; i < employeesList.Count; i++)
                {
                    var employee = employeesList[i];
                    worksheet.Cell(i + 2, 1).Value = employee.Id;
                    worksheet.Cell(i + 2, 2).Value = employee.FullName;
                    worksheet.Cell(i + 2, 3).Value = employee.BirthDate.ToString("dd/MM/yyyy");
                    worksheet.Cell(i + 2, 4).Value = employee.Age;
                    worksheet.Cell(i + 2, 5).Value = employee.Ethnicity;
                    worksheet.Cell(i + 2, 6).Value = employee.Occupation;
                    worksheet.Cell(i + 2, 7).Value = employee.Position;
                    worksheet.Cell(i + 2, 8).Value = employee.CitizenId;
                    worksheet.Cell(i + 2, 9).Value = employee.PhoneNumber;
                    worksheet.Cell(i + 2, 10).Value = employee.Province;
                    worksheet.Cell(i + 2, 11).Value = employee.District;
                    worksheet.Cell(i + 2, 12).Value = employee.Commune;
                    worksheet.Cell(i + 2, 13).Value = employee.QualificationCount;
                }

                // Trả về file Excel
                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "filtered-employees.xlsx");
                }
            }
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

            List<string> errors = new List<string>();
            List<Employee> employeesToImport = new List<Employee>();

            using (var workbook = new XLWorkbook(employeeFile.OpenReadStream()))
            {
                var worksheet = workbook.Worksheet(1); // Giả định dữ liệu ở sheet đầu tiên
                int row = 2; // Bắt đầu từ dòng thứ 2, vì dòng đầu tiên là tiêu đề

                while (!worksheet.Row(row).IsEmpty())
                {
                    var employee = new Employee();
                    try
                    {
                        // Kiểm tra dữ liệu rỗng
                        if (worksheet.Cell(row, 1).IsEmpty() || worksheet.Cell(row, 7).IsEmpty()) // Full Name và CCCD
                        {
                            throw new Exception($"Dòng {row}: Họ tên và CCCD không được để trống.");
                        }

                        // Đọc thông tin từ file Excel
                        employee.FullName = worksheet.Cell(row, 1).GetString();

                        // Sử dụng TryParse để đảm bảo ngày hợp lệ
                        if (DateTime.TryParse(worksheet.Cell(row, 2).GetString(), out DateTime birthDate))
                        {
                            employee.BirthDate = birthDate;
                            // Tính tuổi từ ngày sinh
                            var today = DateTime.Today;
                            employee.Age = today.Year - employee.BirthDate.Year;
                            if (employee.BirthDate.Date > today.AddYears(-employee.Age)) employee.Age--;
                        }
                        else
                        {
                            throw new Exception($"Dòng {row}: Ngày sinh không hợp lệ.");
                        }

                        // Lấy thông tin Dân tộc
                        var ethnicityName = worksheet.Cell(row, 4).GetString();
                        var ethnicity = _context.Ethnicities.FirstOrDefault(e => e.EthnicityName == ethnicityName);
                        if (ethnicity == null) throw new Exception($"Dòng {row}: Dân tộc không hợp lệ.");
                        employee.EthnicityId = ethnicity.EthnicityId;

                        // Lấy thông tin Nghề nghiệp
                        var occupationName = worksheet.Cell(row, 5).GetString();
                        var occupation = _context.Occupations.FirstOrDefault(o => o.OccupationName == occupationName);
                        if (occupation == null) throw new Exception($"Dòng {row}: Nghề nghiệp không hợp lệ.");
                        employee.OccupationId = occupation.OccupationId;

                        // Lấy thông tin Chức vụ
                        var positionName = worksheet.Cell(row, 6).GetString();
                        var position = _context.Positions.FirstOrDefault(p => p.PositionName == positionName);
                        if (position == null) throw new Exception($"Dòng {row}: Chức vụ không hợp lệ.");
                        employee.PositionId = position.PositionId;

                        // Lấy CCCD
                        employee.CitizenId = worksheet.Cell(row, 7).GetString();
                        if (string.IsNullOrEmpty(employee.CitizenId) || !Regex.IsMatch(employee.CitizenId, @"^\d{10,50}$"))
                        {
                            throw new Exception($"Dòng {row}: CCCD không hợp lệ.");
                        }
                        // Check if CCCD already exists in the database
                        var existingEmployee = _context.Employees.FirstOrDefault(e => e.CitizenId == employee.CitizenId);
                        if (existingEmployee != null)
                        {
                            throw new Exception($"Dòng {row}: CCCD đã tồn tại,kiểm tra lại trong danh sách và chỉnh sửa trước khi thêm vào!!!");
                        }

                        // Lấy SDT (Số điện thoại)
                        employee.PhoneNumber = worksheet.Cell(row, 8).GetString();
                        if (!string.IsNullOrEmpty(employee.PhoneNumber) && !Regex.IsMatch(employee.PhoneNumber, @"^0\d{9,14}$"))
                        {
                            throw new Exception($"Dòng {row}: Số điện thoại không hợp lệ.");
                        }

                        // Lấy Tỉnh
                        var provinceName = worksheet.Cell(row, 9).GetString();
                        var province = _context.Provinces.FirstOrDefault(p => p.ProvinceName == provinceName);
                        if (province == null) throw new Exception($"Dòng {row}: Tỉnh không hợp lệ.");
                        employee.ProvinceId = province.ProvinceId;

                        // Lấy Quận/Huyện
                        var districtName = worksheet.Cell(row, 10).GetString();
                        var district = _context.Districts.FirstOrDefault(d => d.DistrictName == districtName && d.ProvinceId == province.ProvinceId);
                        if (district == null) throw new Exception($"Dòng {row}: Quận/Huyện không hợp lệ.");
                        employee.DistrictId = district.DistrictId;

                        // Lấy Xã/Phường
                        var communeName = worksheet.Cell(row, 11).GetString();
                        var commune = _context.Communes.FirstOrDefault(c => c.CommuneName == communeName && c.DistrictId == district.DistrictId);
                        if (commune == null) throw new Exception($"Dòng {row}: Xã/Phường không hợp lệ.");
                        employee.CommuneId = commune.CommuneId;

                        employeesToImport.Add(employee);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex.Message); // Ghi lại lỗi của từng dòng
                    }

                    row++;
                }
            }

            // Nếu có lỗi thì trả về thông báo lỗi, không lưu dữ liệu
            if (errors.Any())
            {
                TempData["ErrorMessage"] = string.Join("<br/>", errors);
                return RedirectToAction("Import");
            }

            // Lưu các nhân viên nếu không có lỗi
            _context.Employees.AddRange(employeesToImport);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Import thành công!";
            return RedirectToAction("ListOfEmployee");
        }


    }
}
