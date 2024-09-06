using System;
using System.Collections.Generic;

namespace OceanTechLevel1.Models;

public partial class EmployeeQualification
{
    public int Id { get; set; }

    public string QualificationName { get; set; } = null!;

    public DateTime IssueDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public int ProvinceId { get; set; }

    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Province Province { get; set; } = null!;
}
