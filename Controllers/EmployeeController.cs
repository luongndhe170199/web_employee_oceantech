using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OceanTechLevel1.Models;
using System.Linq;
using System.Text.Json;

namespace OceanTechLevel1.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly Oceantech2Context _context;

        public EmployeeController(Oceantech2Context context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            ViewBag.Ethnicities = new SelectList(_context.Ethnicities, "EthnicityId", "EthnicityName");
            ViewBag.Occupations = new SelectList(_context.Occupations, "OccupationId", "OccupationName");
            ViewBag.Provinces = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName");

            var districtsJson = JsonSerializer.Serialize(_context.Districts
                .Select(d => new { d.DistrictId, d.DistrictName, d.ProvinceId }).ToList(),
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

            var communesJson = JsonSerializer.Serialize(_context.Communes
                .Select(c => new { c.CommuneId, c.CommuneName, c.DistrictId }).ToList(),
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

            ViewBag.DistrictsJson = districtsJson;
            ViewBag.CommunesJson = communesJson;

            return View();
        }





        public IActionResult ListOfEmployee()
        {
            var employees = _context.Employees.Include(e => e.Ethnicity)
                .Include(e => e.Occupation)
                .Include(e => e.Position)
                .Include(e => e.Province)
                .Include(e => e.District)
                .Include(e => e.Commune)
                .ToList();
            return View(employees);
        }
    }
}
