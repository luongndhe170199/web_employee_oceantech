using System;
using System.Collections.Generic;

namespace OceanTechLevel1.Models;

public partial class Occupation
{
    public int OccupationId { get; set; }

    public string OccupationName { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
