using System;
using System.Collections.Generic;

namespace OceanTechLevel1.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public int Age { get; set; }

    public int? EthnicityId { get; set; }

    public int? OccupationId { get; set; }

    public int? PositionId { get; set; }

    public string CitizenId { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public int? ProvinceId { get; set; }

    public int? DistrictId { get; set; }

    public int? CommuneId { get; set; }

    public string? MoreInfo { get; set; }

    public virtual Commune? Commune { get; set; }

    public virtual District? District { get; set; }

    public virtual Ethnicity? Ethnicity { get; set; }

    public virtual Occupation? Occupation { get; set; }

    public virtual Position? Position { get; set; }

    public virtual Province? Province { get; set; }
}
