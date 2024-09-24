using Microsoft.AspNetCore.Mvc;
using OceanTechLevel1.Models;
using OceanTechLevel1.Services;

namespace OceanTechLevel1.Controllers
{
    public class ProvinceController : Controller
    {
        private readonly ProvinceService _provinceService;

        public ProvinceController(ProvinceService provinceService)
        {
            _provinceService = provinceService;
        }

        public ActionResult Index(string searchTerm, int page = 1)
        {
            int NoOfRecordPerPage = 5;
            var provinces = _provinceService.GetProvinces(searchTerm, page, NoOfRecordPerPage, out int NoOfPages);
            ViewBag.Page = page;
            ViewBag.NoOfPages = NoOfPages;
            ViewBag.SearchTerm = searchTerm;
            return View(provinces);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Province p)
        {
            if (!ModelState.IsValid)
            {
                return View(p);
            }

            if (_provinceService.ProvinceExists(p.ProvinceName))
            {
                ModelState.AddModelError("ProvinceName", "Tên tỉnh đã tồn tại.");
                return View(p);
            }

            _provinceService.CreateProvince(p);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var province = _provinceService.GetProvinceById(id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        [HttpPost]
        public ActionResult Edit(Province pro)
        {
            if (!ModelState.IsValid)
            {
                return View(pro);
            }

            if (_provinceService.ProvinceExists(pro.ProvinceName))
            {
                ModelState.AddModelError("ProvinceName", "Tên tỉnh đã tồn tại.");
                return View(pro);
            }

            var province = _provinceService.GetProvinceById(pro.ProvinceId);
            if (province == null)
            {
                return NotFound();
            }

            province.ProvinceName = pro.ProvinceName;
            _provinceService.UpdateProvince(province);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var province = _provinceService.GetProvinceById(id);
            if (province == null)
            {
                return NotFound();
            }

            _provinceService.DeleteProvince(province);

            return RedirectToAction("Index");
        }
    }
}
