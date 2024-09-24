using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;

namespace OceanTechLevel1.Services
{
    public class QualificationService
    {
        private readonly Oceantech2Context _context;

        public QualificationService(Oceantech2Context context)
        {
            _context = context;
        }

        public Employee GetEmployeeWithQualifications(int employeeId)
        {
            return _context.Employees
                .Include(e => e.EmployeeQualifications)
                .FirstOrDefault(e => e.Id == employeeId);
        }

        public List<Province> GetAllProvinces()
        {
            return _context.Provinces.ToList();
        }

        public bool ProvinceExists(int provinceId)
        {
            return _context.Provinces.Any(p => p.ProvinceId == provinceId);
        }

        public int GetValidQualificationsCount(Employee employee)
        {
            return employee.EmployeeQualifications
                .Count(q => q.ExpirationDate == null || q.ExpirationDate > DateTime.Now);
        }

        public void AddQualification(EmployeeQualification qualification)
        {
            _context.EmployeeQualifications.Add(qualification);
            _context.SaveChanges();
        }

        public void DeleteQualification(EmployeeQualification qualification)
        {
            _context.EmployeeQualifications.Remove(qualification);
            _context.SaveChanges();
        }

        public EmployeeQualification GetQualificationById(int qualificationId)
        {
            return _context.EmployeeQualifications.FirstOrDefault(q => q.Id == qualificationId);
        }

        public void UpdateQualification(EmployeeQualification qualification)
        {
            _context.SaveChanges();
        }
    }


}
