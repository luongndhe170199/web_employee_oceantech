using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using OceanTechLevel1.Services;

namespace OceanTechLevel1.Controllers
{
    public class CommuneController : Controller
    {
        private readonly CommuneService _communeService;
        private readonly Oceantech2Context _context;

        public CommuneController(CommuneService communeService, Oceantech2Context context)
        {
            _communeService = communeService;
            _context = context;
        }

        public ActionResult Index(string searchTerm, int page = 1)
        {
            int NoOfRecordPerPage = 5;
            var communes = _communeService.GetCommunes(searchTerm, page, NoOfRecordPerPage, out int NoOfPages);

            ViewBag.Page = page;
            ViewBag.NoOfPages = NoOfPages;
            ViewBag.SearchTerm = searchTerm;
            return View(communes);
        }

        public ActionResult Create()
        {
            ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName");
            return View();
        }

        [HttpPost]
        public ActionResult Create(Commune commune)
        {
            if (!ModelState.IsValid)
            {
                ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
                return View(commune);
            }

            if (_communeService.CommuneExists(commune.CommuneName, commune.DistrictId ?? 0))
            {
                ModelState.AddModelError("CommuneName", "Tên xã đã tồn tại trong huyện này.");
                ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
                return View(commune);
            }

            _communeService.CreateCommune(commune);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var commune = _communeService.GetCommuneById(id);
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
            if (!ModelState.IsValid)
            {
                ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
                return View(commune);
            }

            if (_communeService.CommuneExists(commune.CommuneName, commune.DistrictId ?? 0, commune.CommuneId))
            {
                ModelState.AddModelError("CommuneName", "Tên xã đã tồn tại trong huyện này.");
                ViewData["DistrictId"] = new SelectList(_context.Districts.Include(d => d.Province), "DistrictId", "DistrictName", commune.DistrictId);
                return View(commune);
            }

            _communeService.UpdateCommune(commune);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var commune = _communeService.GetCommuneById(id);
            if (commune == null)
            {
                return NotFound();
            }

            _communeService.DeleteCommune(commune);
            return RedirectToAction("Index");
        }
    }
}
