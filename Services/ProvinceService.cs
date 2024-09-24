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
            var employeesInCommunes = _context.Employees
                                              .Where(e => e.CommuneId.HasValue &&
                                                          _context.Communes.Any(c => c.CommuneId == e.CommuneId.Value &&
                                                                                     _context.Districts.Any(d => d.DistrictId == c.DistrictId &&
                                                                                                                 _context.Provinces.Any(p => p.ProvinceId == d.ProvinceId &&
                                                                                                                                               p.ProvinceId == province.ProvinceId))))
                                              .ToList();
            _context.Employees.RemoveRange(employeesInCommunes);

            var qualificationsInProvince = _context.EmployeeQualifications.Where(eq => eq.ProvinceId == province.ProvinceId).ToList();
            _context.EmployeeQualifications.RemoveRange(qualificationsInProvince);

            foreach (var district in province.Districts)
            {
                _context.Communes.RemoveRange(district.Communes);
            }

            _context.Districts.RemoveRange(province.Districts);
            _context.Provinces.Remove(province);

            _context.SaveChanges();
        }
    }
}
