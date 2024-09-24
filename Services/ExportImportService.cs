using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OceanTechLevel1.Services
{
    public class ExportImportService
    {
        private readonly Oceantech2Context _context;
        private readonly ValidationService _validationService;
        public ExportImportService(Oceantech2Context context,ValidationService validationService)
        {
            _context = context;
            _validationService = validationService;
        }

        public List<dynamic> GetSelectedEmployees(int[] selectedEmployees)
        {
            return _context.Employees
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
                    District = e.District != null ? e.District.DistrictName : "N/A",
                    Commune = e.Commune != null ? e.Commune.CommuneName : "N/A",
                    QualificationCount = e.EmployeeQualifications.Count(q => q.ExpirationDate == null || q.ExpirationDate > DateTime.Now)
                })
                .AsEnumerable()
                .Select(e => (dynamic)e)
                .ToList();
        }

        public MemoryStream ExportEmployeesToExcel(List<dynamic> employees)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Employees");

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
                    worksheet.Cell(i + 2, 9).Value = employee.PhoneNumber ?? "Chưa cung cấp";
                    worksheet.Cell(i + 2, 10).Value = employee.Province;
                    worksheet.Cell(i + 2, 11).Value = employee.District ?? "N/A";
                    worksheet.Cell(i + 2, 12).Value = employee.Commune ?? "N/A";
                    worksheet.Cell(i + 2, 13).Value = employee.QualificationCount;
                }

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream;
            }
        }

        public List<dynamic> GetFilteredEmployees(string searchTerm)
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
                    District = e.District != null ? e.District.DistrictName : "N/A",
                    Commune = e.Commune != null ? e.Commune.CommuneName : "N/A",
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

            return employees
                .AsEnumerable()
                .Select(e => (dynamic)e)
                .ToList();
        }

        public List<string> ImportEmployeesFromExcel(IFormFile employeeFile)
        {
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
                        if (worksheet.Cell(row, 1).IsEmpty() || worksheet.Cell(row, 7).IsEmpty())
                        {
                            throw new Exception($"Dòng {row}: Họ tên và CCCD không được để trống.");
                        }

                        employee.FullName = worksheet.Cell(row, 1).GetString();

                        if (DateTime.TryParse(worksheet.Cell(row, 2).GetString().Trim(), out DateTime birthDate))
                        {
                            employee.BirthDate = birthDate;
                            var today = DateTime.Today;
                            employee.Age = today.Year - employee.BirthDate.Year;
                            if (employee.BirthDate.Date > today.AddYears(-employee.Age)) employee.Age--;
                        }
                        else
                        {
                            throw new Exception($"Dòng {row}: Ngày sinh không hợp lệ.");
                        }

                        var ethnicityName = worksheet.Cell(row, 4).GetString();
                        var ethnicity = _context.Ethnicities.FirstOrDefault(e => e.EthnicityName == ethnicityName);
                        if (ethnicity == null) throw new Exception($"Dòng {row}: Dân tộc không hợp lệ.");
                        employee.EthnicityId = ethnicity.EthnicityId;

                        var occupationName = worksheet.Cell(row, 5).GetString();
                        var occupation = _context.Occupations.FirstOrDefault(o => o.OccupationName == occupationName);
                        if (occupation == null) throw new Exception($"Dòng {row}: Nghề nghiệp không hợp lệ.");
                        employee.OccupationId = occupation.OccupationId;

                        var positionName = worksheet.Cell(row, 6).GetString();
                        var position = _context.Positions.FirstOrDefault(p => p.PositionName == positionName);
                        if (position == null) throw new Exception($"Dòng {row}: Chức vụ không hợp lệ.");
                        employee.PositionId = position.PositionId;

                        employee.CitizenId = worksheet.Cell(row, 7).GetString();
                        if (string.IsNullOrEmpty(employee.CitizenId) || !Regex.IsMatch(employee.CitizenId, @"^\d{10,50}$"))
                        {
                            throw new Exception($"Dòng {row}: CCCD không hợp lệ.");
                        }

                        var existingEmployee = _context.Employees.FirstOrDefault(e => e.CitizenId == employee.CitizenId);
                        if (existingEmployee != null)
                        {
                            throw new Exception($"Dòng {row}: CCCD đã tồn tại.");
                        }

                        var phoneNumber = worksheet.Cell(row, 8).GetString();
                        if (string.IsNullOrEmpty(phoneNumber))
                        {
                            employee.PhoneNumber = null;
                        }
                        else if (!Regex.IsMatch(phoneNumber, @"^0\d{9,14}$"))
                        {
                            throw new Exception($"Dòng {row}: Số điện thoại không hợp lệ.");
                        }
                        else
                        {
                            employee.PhoneNumber = phoneNumber;
                        }

                        var provinceName = worksheet.Cell(row, 9).GetString();
                        var province = _context.Provinces.FirstOrDefault(p => p.ProvinceName == provinceName);
                        if (province == null) throw new Exception($"Dòng {row}: Tỉnh không hợp lệ.");
                        employee.ProvinceId = province.ProvinceId;

                        var districtName = worksheet.Cell(row, 10).GetString();
                        var district = _context.Districts.FirstOrDefault(d => d.DistrictName == districtName && d.ProvinceId == province.ProvinceId);
                        if (district == null) throw new Exception($"Dòng {row}: Quận/Huyện không hợp lệ.");
                        employee.DistrictId = district.DistrictId;

                        var communeName = worksheet.Cell(row, 11).GetString();
                        var commune = _context.Communes.FirstOrDefault(c => c.CommuneName == communeName && c.DistrictId == district.DistrictId);
                        if (commune == null) throw new Exception($"Dòng {row}: Xã/Phường không hợp lệ.");
                        employee.CommuneId = commune.CommuneId;

                        employeesToImport.Add(employee);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex.Message);
                    }

                    row++;
                }
            }

            if (!errors.Any())
            {
                _context.Employees.AddRange(employeesToImport);
                _context.SaveChanges();
            }

            return errors;
        }
    }
}
