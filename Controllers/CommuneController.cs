﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System.Linq;

namespace OceanTechLevel1.Controllers
{
    public class CommuneController : Controller
    {
        private readonly Oceantech2Context _context;

        public CommuneController(Oceantech2Context context)
        {
            _context = context;
        }

        public ActionResult Index(string searchTerm, int page = 1)
        {
            var communes = _context.Communes
                                   .Include(c => c.District)
                                   .ThenInclude(d => d.Province)
                                   .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Sử dụng Trim() để loại bỏ khoảng trắng ở đầu và cuối chuỗi
                searchTerm = searchTerm.Trim().ToLower();

                communes = communes.Where(c => c.CommuneName.Trim().ToLower().Contains(searchTerm) ||
                                                c.District.DistrictName.Trim().ToLower().Contains(searchTerm) ||
                                                c.District.Province.ProvinceName.Trim().ToLower().Contains(searchTerm));
            }

            var communeList= communes.ToList();
            // Paging 
            int NoOfRecordPerPage = 5;
            int NoOfPages = Convert.ToInt32(Math.Ceiling
                (Convert.ToDouble(communeList.Count) / Convert.ToDouble
                (NoOfRecordPerPage)));

            int NoOfRecordToSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPages = NoOfPages;
            communeList = communeList.Skip(NoOfRecordToSkip).Take(NoOfRecordPerPage).ToList();
            return View(communeList);
        }



        public ActionResult Create()
        {
            ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName");
            return View();
        }

        [HttpPost]
        public ActionResult Create(Commune commune)
        {
            // Kiểm tra nếu Model hợp lệ
            if (!ModelState.IsValid)
            {
                ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
                return View(commune);
            }

            // Kiểm tra trùng lặp tên xã trong cùng một huyện
            var existingCommune = _context.Communes
                .FirstOrDefault(c => c.CommuneName.ToLower().Trim() == commune.CommuneName.ToLower().Trim()
                                     && c.DistrictId == commune.DistrictId);

            if (existingCommune != null)
            {
                // Thêm thông báo lỗi nếu tên xã đã tồn tại
                ModelState.AddModelError("CommuneName", "Tên xã đã tồn tại trong huyện này.");
                ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
                return View(commune);
            }

            // Nếu hợp lệ, thêm xã mới vào database
            _context.Communes.Add(commune);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult Edit(int id)
        {
            var commune = _context.Communes.Include(c => c.District).ThenInclude(d => d.Province).FirstOrDefault(c => c.CommuneId == id);
            if (commune == null)
            {
                return NotFound();
            }
            ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
            return View(commune);
        }

        [HttpPost]
        public ActionResult Edit(Commune commune)
        {
            // Kiểm tra nếu Model hợp lệ
            if (!ModelState.IsValid)
            {
                ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
                return View(commune);
            }

            // Kiểm tra trùng lặp tên xã trong cùng một huyện (trừ xã hiện tại đang chỉnh sửa)
            var existingCommune = _context.Communes
                .FirstOrDefault(c => c.CommuneName.ToLower().Trim() == commune.CommuneName.ToLower().Trim()
                                     && c.DistrictId == commune.DistrictId
                                     && c.CommuneId != commune.CommuneId);

            if (existingCommune != null)
            {
                // Thêm thông báo lỗi nếu tên xã đã tồn tại
                ModelState.AddModelError("CommuneName", "Tên xã đã tồn tại trong huyện này.");
                ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
                return View(commune);
            }

            // Nếu hợp lệ, cập nhật xã trong database
            _context.Communes.Update(commune);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            var commune = _context.Communes.Include(c => c.Employees).FirstOrDefault(c => c.CommuneId == id);
            if (commune == null)
            {
                return NotFound();
            }

            // Xóa tất cả các nhân viên liên quan
            _context.Employees.RemoveRange(commune.Employees);

            // Xóa xã
            _context.Communes.Remove(commune);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
