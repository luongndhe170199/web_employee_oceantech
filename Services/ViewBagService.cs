using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OceanTechLevel1.Controllers;
using OceanTechLevel1.Models;
using System.Text.Json;


namespace OceanTechLevel1.Services
{
    public class ViewBagService
    {
        private readonly Oceantech2Context _context;

        public ViewBagService(Oceantech2Context context)
        {
            _context = context;
        }

        public void PopulateViewBags(Controller controller)
        {
            controller.ViewBag.Ethnicities = new SelectList(_context.Ethnicities, "EthnicityId", "EthnicityName");
            controller.ViewBag.Occupations = new SelectList(_context.Occupations, "OccupationId", "OccupationName");
            controller.ViewBag.Positions = new SelectList(_context.Positions, "PositionId", "PositionName");
            controller.ViewBag.Provinces = new SelectList(_context.Provinces, "ProvinceId", "ProvinceName");
            controller.ViewBag.Districts = _context.Districts.ToList();
            controller.ViewBag.Communes = _context.Communes.ToList();

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

            controller.ViewBag.DistrictsJson = districtsJson;
            controller.ViewBag.CommunesJson = communesJson;
        }

     
    }
}
