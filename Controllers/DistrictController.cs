using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OceanTechLevel1.Constants;
using OceanTechLevel1.Models;
using OceanTechLevel1.Services;

namespace OceanTechLevel1.Controllers
{
    public class DistrictController : Controller
    {
        private readonly DistrictService _districtService;
        private readonly Oceantech2Context _context;

        public DistrictController(DistrictService districtService, Oceantech2Context context)
        {
            _districtService = districtService;
            _context = context;
        }

        public ActionResult Index(string searchTerm, int page = ValidationConstants.DefaultPage)
        {
            int NoOfRecordPerPage = ValidationConstants.DefaultNoOfRecordsPerPage;
            var districts = _districtService.GetDistricts(searchTerm, page, NoOfRecordPerPage, out int NoOfPages);

            ViewBag.Page = page;
            ViewBag.NoOfPages = NoOfPages;
            ViewBag.SearchTerm = searchTerm;
            return View(districts);
        }

        public ActionResult Create()
        {
            ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName");
            return View();
        }

        [HttpPost]
        public ActionResult Create(District d)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", d.ProvinceId);
                return View(d);
            }

            if (_districtService.DistrictExists(d.DistrictName, d.ProvinceId??0))
            {
                ModelState.AddModelError("DistrictName", "Tên huyện đã tồn tại trong tỉnh này.");
                ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", d.ProvinceId);
                return View(d);
            }

            _districtService.CreateDistrict(d);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var district = _districtService.GetDistrictById(id);
            if (district == null)
            {
                return NotFound();
            }
            ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", district.ProvinceId);
            return View(district);
        }

        [HttpPost]
        public ActionResult Edit(District d)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", d.ProvinceId);
                return View(d);
            }

            if (_districtService.DistrictExists(d.DistrictName, d.ProvinceId ?? 0, d.DistrictId))
            {
                ModelState.AddModelError("DistrictName", "Tên huyện đã tồn tại trong tỉnh này.");
                ViewBag.ProvinceId = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", d.ProvinceId);
                return View(d);
            }

            _districtService.UpdateDistrict(d);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var district = _districtService.GetDistrictById(id);
            if (district == null)
            {
                return NotFound();
            }

            _districtService.DeleteDistrict(district);
            return RedirectToAction("Index");
        }
    }
}
