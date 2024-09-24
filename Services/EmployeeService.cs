using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System.Text.Json;
using System.Web.Mvc;

namespace OceanTechLevel1.Services
{
    public class EmployeeService
    {
        private readonly Oceantech2Context _context;

        public EmployeeService(Oceantech2Context context)
        {
            _context = context;
        }

        public Employee GetEmployeeById(int id)
        {
            return _context.Employees.FirstOrDefault(e => e.Id == id);
        }

        public IQueryable<EmployeeViewModel> GetEmployees(string searchTerm)
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
                    QualificationCount = e.EmployeeQualifications.Count(q => q.ExpirationDate == null || q.ExpirationDate > DateTime.Now)
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

            return employees;
        }

        public void AddEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            _context.SaveChanges();
        }

        public void UpdateEmployee(Employee employee)
        {
            var existingEmployee = GetEmployeeById(employee.Id);
            if (existingEmployee != null)
            {
                existingEmployee.FullName = employee.FullName;
                existingEmployee.BirthDate = employee.BirthDate;
                existingEmployee.Age = employee.Age;
                existingEmployee.EthnicityId = employee.EthnicityId;
                existingEmployee.CitizenId = employee.CitizenId;
                existingEmployee.OccupationId = employee.OccupationId;
                existingEmployee.PositionId = employee.PositionId;
                existingEmployee.PhoneNumber = employee.PhoneNumber;
                existingEmployee.ProvinceId = employee.ProvinceId;
                existingEmployee.DistrictId = employee.DistrictId;
                existingEmployee.CommuneId = employee.CommuneId;
                existingEmployee.MoreInfo = employee.MoreInfo;

                _context.SaveChanges();
            }
        }

        public void DeleteEmployee(int id)
        {
            var employee = _context.Employees
                .Include(e => e.EmployeeQualifications)
                .FirstOrDefault(e => e.Id == id);

            if (employee != null)
            {
                _context.EmployeeQualifications.RemoveRange(employee.EmployeeQualifications);
                _context.Employees.Remove(employee);
                _context.SaveChanges();
            }
        }
    }

}


