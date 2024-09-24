using OceanTechLevel1.Constants; // Thêm dòng này để import hằng số từ ValidationConstants
using OceanTechLevel1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OceanTechLevel1.Services
{
    public class ValidationService
    {
        private readonly Oceantech2Context _context;

        public ValidationService(Oceantech2Context context)
        {
            _context = context;
        }

        public bool ValidateFullName(string fullName)
        {
            return !string.IsNullOrEmpty(fullName);
        }

        public bool ValidateBirthDate(string birthDateString, out DateTime birthDate, out int age)
        {
            if (DateTime.TryParse(birthDateString, out birthDate))
            {
                var today = DateTime.Today;
                age = today.Year - birthDate.Year;
                if (birthDate.Date > today.AddYears(-age)) age--;
                return true;
            }
            age = 0;
            return false;
        }

        public bool ValidateEthnicity(string ethnicityName, out Ethnicity ethnicity)
        {
            ethnicity = _context.Ethnicities.FirstOrDefault(e => e.EthnicityName == ethnicityName);
            return ethnicity != null;
        }

        public bool ValidateOccupation(string occupationName, out Occupation occupation)
        {
            occupation = _context.Occupations.FirstOrDefault(o => o.OccupationName == occupationName);
            return occupation != null;
        }

        public bool ValidatePosition(string positionName, out Position position)
        {
            position = _context.Positions.FirstOrDefault(p => p.PositionName == positionName);
            return position != null;
        }

        public bool ValidateCitizenId(string citizenId)
        {
            return !string.IsNullOrEmpty(citizenId) && Regex.IsMatch(citizenId, $@"^\d{{{ValidationConstants.CitizenIdMinLength},{ValidationConstants.CitizenIdMaxLength}}}$");
        }

        public bool ValidatePhoneNumber(string phoneNumber)
        {
            return string.IsNullOrEmpty(phoneNumber) || Regex.IsMatch(phoneNumber, $@"^0\d{{{ValidationConstants.PhoneNumberMinLength },{ValidationConstants.PhoneNumberMaxLength }}}$");
        }

        public bool ValidateProvince(string provinceName, out Province province)
        {
            province = _context.Provinces.FirstOrDefault(p => p.ProvinceName == provinceName);
            return province != null;
        }

        public bool ValidateDistrict(string districtName, int provinceId, out District district)
        {
            district = _context.Districts.FirstOrDefault(d => d.DistrictName == districtName && d.ProvinceId == provinceId);
            return district != null;
        }

        public bool ValidateCommune(string communeName, int districtId, out Commune commune)
        {
            commune = _context.Communes.FirstOrDefault(c => c.CommuneName == communeName && c.DistrictId == districtId);
            return commune != null;
        }

        public bool IsCitizenIdUnique(string citizenId)
        {
            return !_context.Employees.Any(e => e.CitizenId == citizenId);
        }
    }
}
