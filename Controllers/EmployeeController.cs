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
            ViewBag.Positions = new SelectList(_context.Positions, "PositionId", "PositionName");
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

        [HttpPost]
        public ActionResult Create(Employee p)
        {
            if (string.IsNullOrEmpty(p.CitizenId))
            {
                ModelState.AddModelError("CitizenId", "CCCD không được để trống.");
                return View(p);
            }

            _context.Employees.Add(p);
            _context.SaveChanges();
            return RedirectToAction("ListOfEmployee");
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

        public ActionResult Edit(int id)
        {
            Employee employee = _context.Employees.Where(row => row.Id == id).FirstOrDefault();

            ViewBag.Ethnicities = new SelectList(_context.Ethnicities, "EthnicityId", "EthnicityName", employee.EthnicityId);
            ViewBag.Occupations = new SelectList(_context.Occupations, "OccupationId", "OccupationName", employee.OccupationId);
            ViewBag.Positions = new SelectList(_context.Positions, "PositionId", "PositionName", employee.PositionId);
            ViewBag.Provinces = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName", employee.ProvinceId);
            ViewBag.Districts = _context.Districts.ToList();  // hoặc
            ViewBag.Communes = _context.Communes.ToList();

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

            return View(employee);
        }

        [HttpPost]
        public ActionResult Edit(Employee emp)
        {
            Employee employee = _context.Employees.Where(row => row.Id == emp.Id).FirstOrDefault();

            // update
            employee.FullName = emp.FullName;
            employee.BirthDate = emp.BirthDate;
            employee.Age = emp.Age;
            employee.EthnicityId = emp.EthnicityId;
            employee.CitizenId = emp.CitizenId;
            employee.OccupationId = emp.OccupationId;
            employee.PositionId = emp.PositionId;
            employee.PhoneNumber = emp.PhoneNumber;
            employee.ProvinceId = emp.ProvinceId;
            employee.DistrictId = emp.DistrictId;
            employee.CommuneId = emp.CommuneId;
            employee.MoreInfo = emp.MoreInfo;

            _context.SaveChanges();
            return RedirectToAction("ListOfEmployee");
        }

        public ActionResult Delete(int id)
        {
            Employee employee = _context.Employees.Where(row => row.Id == id).FirstOrDefault();
            return View(employee);
        }
        [HttpPost]
        public ActionResult Delete(int id,Employee e)
        {
            Employee employee = _context.Employees.Where(row => row.Id == id).FirstOrDefault();
            _context.Employees.Remove(employee);
            _context.SaveChanges();
            return RedirectToAction("ListOfEmployee");
        }

    }
}
