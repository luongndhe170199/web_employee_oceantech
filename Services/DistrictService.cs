using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OceanTechLevel1.Services
{
    public class DistrictService
    {
        private readonly Oceantech2Context _context;
        private readonly PagingService _pagingService;
        public DistrictService(Oceantech2Context context, PagingService pagingService)
        {
            _context = context;
            _pagingService = pagingService;
        }

        public List<District> GetDistricts(string searchTerm, int page, int pageSize, out int totalPages)
        {
            var districts = _context.Districts.Include(d => d.Province).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                districts = districts.Where(d => d.DistrictName.Trim().ToLower().Contains(searchTerm) ||
                                                 d.Province.ProvinceName.Trim().ToLower().Contains(searchTerm));
            }

            return _pagingService.GetPagedResult(districts, page, pageSize, out totalPages);
        }

        public District GetDistrictById(int id)
        {
            return _context.Districts.Include(d => d.Province).Include(d => d.Communes).Include(d => d.Employees).FirstOrDefault(d => d.DistrictId == id);
        }

        public void CreateDistrict(District district)
        {
            _context.Districts.Add(district);
            _context.SaveChanges();
        }

        public bool DistrictExists(string districtName, int provinceId, int? districtId = null)
        {
            return _context.Districts.Any(d => d.DistrictName.ToLower().Trim() == districtName.ToLower().Trim() && d.ProvinceId == provinceId && (!districtId.HasValue || d.DistrictId != districtId.Value));
        }

        public void UpdateDistrict(District district)
        {
            _context.Districts.Update(district);
            _context.SaveChanges();
        }

        public void DeleteDistrict(District district)
        {
            // Tải huyện cùng với các xã và nhân viên liên quan
            var districtToDelete = _context.Districts
                .Include(d => d.Communes)
                .Include(d => d.Employees)
                .ThenInclude(e => e.EmployeeQualifications)  // Tải thêm văn bằng của nhân viên
                .FirstOrDefault(d => d.DistrictId == district.DistrictId);

            if (districtToDelete != null)
            {
                // Xóa các văn bằng của nhân viên trước
                foreach (var employee in districtToDelete.Employees)
                {
                    foreach (var qualification in employee.EmployeeQualifications)
                    {
                        _context.EmployeeQualifications.Remove(qualification);
                    }
                }

                // Xóa các nhân viên
                foreach (var employee in districtToDelete.Employees)
                {
                    _context.Employees.Remove(employee);
                }

                // Xóa các xã liên quan
                foreach (var commune in districtToDelete.Communes)
                {
                    _context.Communes.Remove(commune);
                }

                // Cuối cùng, xóa huyện
                _context.Districts.Remove(districtToDelete);

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();
            }
        }

    }
}
