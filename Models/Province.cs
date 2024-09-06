using System;
using System.Collections.Generic;

namespace OceanTechLevel1.Models;

public partial class Province
{
    public int ProvinceId { get; set; }

    public string ProvinceName { get; set; } = null!;

    public virtual ICollection<District> Districts { get; set; } = new List<District>();

    public virtual ICollection<EmployeeQualification> EmployeeQualifications { get; set; } = new List<EmployeeQualification>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
