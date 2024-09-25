using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OceanTechLevel1.Services
{
    public class CommuneService
    {
        private readonly Oceantech2Context _context;
        private readonly PagingService _pagingService;
        public CommuneService(Oceantech2Context context, PagingService pagingService)
        {
            _context = context;
            _pagingService = pagingService;
        }

        public List<Commune> GetCommunes(string searchTerm, int page, int pageSize, out int totalPages)
        {
            var communes = _context.Communes
                                   .Include(c => c.District)
                                   .ThenInclude(d => d.Province)
                                   .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();

                communes = communes.Where(c => c.CommuneName.Trim().ToLower().Contains(searchTerm) ||
                                                c.District.DistrictName.Trim().ToLower().Contains(searchTerm) ||
                                                c.District.Province.ProvinceName.Trim().ToLower().Contains(searchTerm));
            }

            return _pagingService.GetPagedResult(communes, page, pageSize, out totalPages);
        }

        public Commune GetCommuneById(int id)
        {
            return _context.Communes.Include(c => c.District).ThenInclude(d => d.Province).FirstOrDefault(c => c.CommuneId == id);
        }

        public void CreateCommune(Commune commune)
        {
            _context.Communes.Add(commune);
            _context.SaveChanges();
        }

        public bool CommuneExists(string communeName, int districtId, int? communeId = null)
        {
            return _context.Communes.Any(c => c.CommuneName.ToLower().Trim() == communeName.ToLower().Trim() &&
                                              c.DistrictId == districtId &&
                                              (!communeId.HasValue || c.CommuneId != communeId.Value));
        }

        public void UpdateCommune(Commune commune)
        {
            _context.Communes.Update(commune);
            _context.SaveChanges();
        }

        public void DeleteCommune(Commune commune)
        {
            // Tải xã cùng với các nhân viên liên quan
            var communeToDelete = _context.Communes
                .Include(c => c.Employees)  // Tải các nhân viên liên quan đến xã
                .ThenInclude(e => e.EmployeeQualifications)  // Tải thêm các văn bằng của nhân viên
                .FirstOrDefault(c => c.CommuneId == commune.CommuneId);

            if (communeToDelete != null)
            {
                // Xóa các văn bằng của nhân viên trước
                foreach (var employee in communeToDelete.Employees)
                {
                    foreach (var qualification in employee.EmployeeQualifications)
                    {
                        _context.EmployeeQualifications.Remove(qualification);
                    }
                }

                // Xóa các nhân viên trong xã
                foreach (var employee in communeToDelete.Employees)
                {
                    _context.Employees.Remove(employee);
                }

                // Cuối cùng, xóa xã
                _context.Communes.Remove(communeToDelete);

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();
            }
        }

    }
}
