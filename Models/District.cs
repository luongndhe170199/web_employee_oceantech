using System;
using System.Collections.Generic;

namespace OceanTechLevel1.Models;

public partial class District
{
    public int DistrictId { get; set; }

    public string DistrictName { get; set; } = null!;

    public int? ProvinceId { get; set; }

    public virtual ICollection<Commune> Communes { get; set; } = new List<Commune>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual Province? Province { get; set; }
}
