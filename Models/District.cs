using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OceanTechLevel1.Models;

public partial class District
{
    public int DistrictId { get; set; }
    [Required(ErrorMessage = "Yêu cầu nhập tên huyện.")]
    public string DistrictName { get; set; } = null!;

    public int? ProvinceId { get; set; }

    public virtual ICollection<Commune> Communes { get; set; } = new List<Commune>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual Province? Province { get; set; }
}
