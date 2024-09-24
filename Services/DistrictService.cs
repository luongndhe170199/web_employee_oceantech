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
            _context.Communes.RemoveRange(district.Communes);
            _context.Employees.RemoveRange(district.Employees);
            _context.Districts.Remove(district);
            _context.SaveChanges();
        }
    }
}
