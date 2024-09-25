using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OceanTechLevel1.Services
{
    public class ProvinceService
    {
        private readonly Oceantech2Context _context;
        private readonly PagingService _pagingService;

        public ProvinceService(Oceantech2Context context, PagingService pagingService)
        {
            _context = context;
            _pagingService = pagingService;
        }

        public List<Province> GetProvinces(string searchTerm, int page, int pageSize, out int totalPages)
        {
            var provinces = _context.Provinces.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                provinces = provinces.Where(p => p.ProvinceName.ToLower().Contains(searchTerm));
            }

            return _pagingService.GetPagedResult(provinces, page, pageSize, out totalPages);
        }

        public Province GetProvinceById(int id)
        {
            return _context.Provinces.Include(p => p.Districts).ThenInclude(d => d.Communes).FirstOrDefault(p => p.ProvinceId == id);
        }

        public void CreateProvince(Province province)
        {
            _context.Provinces.Add(province);
            _context.SaveChanges();
        }

        public bool ProvinceExists(string provinceName)
        {
            return _context.Provinces.Any(p => p.ProvinceName.ToLower().Trim() == provinceName.ToLower().Trim());
        }

        public void UpdateProvince(Province province)
        {
            _context.SaveChanges();
        }

        public void DeleteProvince(Province province)
        {
            // Tải tỉnh cùng với tất cả các thực thể liên quan
            var provinceToDelete = _context.Provinces
                .Include(p => p.Districts)
                .ThenInclude(d => d.Communes)
                .Include(p => p.Employees)
                .ThenInclude(e => e.EmployeeQualifications)  // Tải thêm EmployeeQualifications
                .Include(p => p.EmployeeQualifications)
                .FirstOrDefault(p => p.ProvinceId == province.ProvinceId);

            if (provinceToDelete != null)
            {
                // Xóa các văn bằng của nhân viên trước
                foreach (var employee in provinceToDelete.Employees)
                {
                    foreach (var qualification in employee.EmployeeQualifications)
                    {
                        _context.EmployeeQualifications.Remove(qualification);
                    }
                }

                // Xóa các nhân viên
                foreach (var employee in provinceToDelete.Employees)
                {
                    _context.Employees.Remove(employee);
                }

                // Xóa các xã và huyện liên quan
                foreach (var district in provinceToDelete.Districts)
                {
                    foreach (var commune in district.Communes)
                    {
                        _context.Communes.Remove(commune);
                    }
                    _context.Districts.Remove(district);
                }

                // Xóa tỉnh
                _context.Provinces.Remove(provinceToDelete);

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();
            }
        }

    }
}
