using System;
using System.Collections.Generic;

namespace OceanTechLevel1.Models;

public partial class Ethnicity
{
    public int EthnicityId { get; set; }

    public string EthnicityName { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
