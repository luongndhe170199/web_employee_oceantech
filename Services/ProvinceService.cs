using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;

        public ProvinceService(Oceantech2Context context, PagingService pagingService, IMemoryCache cache)
        {
            _context = context;
            _pagingService = pagingService;
            _cache = cache;
        }

        public List<Province> GetProvinces(string searchTerm, int page, int pageSize, out int totalPages)
        {
            var cacheKey = $"GetProvinces_{searchTerm}_{page}_{pageSize}";

            // Kiểm tra xem dữ liệu có trong cache hay không
            if (!_cache.TryGetValue(cacheKey, out List<Province> provinces))
            {
                var query = _context.Provinces.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower().Trim();
                    query = query.Where(p => p.ProvinceName.ToLower().Contains(searchTerm));
                }

                // Áp dụng phân trang
                provinces = _pagingService.GetPagedResult(query, page, pageSize, out totalPages);

                // Lưu dữ liệu vào cache với thời hạn là 5 phút
                _cache.Set(cacheKey, provinces, TimeSpan.FromMinutes(30));
            }
            else
            {
                // Tính toán lại số trang nếu dữ liệu lấy từ cache
                totalPages = (int)Math.Ceiling((double)_context.Provinces.Count() / pageSize);
            }

            return provinces;
        }

        public Province GetProvinceById(int id)
        {
            return _context.Provinces.Include(p => p.Districts).ThenInclude(d => d.Communes).FirstOrDefault(p => p.ProvinceId == id);
        }

        public void CreateProvince(Province province)
        {
            _context.Provinces.Add(province);
            _context.SaveChanges();
            _cache.Remove("GetProvinces_*");
        }

        public bool ProvinceExists(string provinceName)
        {
            return _context.Provinces.Any(p => p.ProvinceName.ToLower().Trim() == provinceName.ToLower().Trim());
        }

        public void UpdateProvince(Province province)
        {
            _context.SaveChanges();
            _cache.Remove("GetProvinces_*");
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

                _cache.Remove("GetProvinces_*");
            }
        }

    }
}