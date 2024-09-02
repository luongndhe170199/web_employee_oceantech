using System;
using System.Collections.Generic;

namespace OceanTechLevel1.Models;

public partial class Commune
{
    public int CommuneId { get; set; }

    public string CommuneName { get; set; } = null!;

    public int? DistrictId { get; set; }

    public virtual District? District { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
