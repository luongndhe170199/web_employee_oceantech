using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OceanTechLevel1.Models;

public partial class Province
{
    public int ProvinceId { get; set; }

    [Required(ErrorMessage = "Yêu cầu nhập tên tỉnh.")]
    public string ProvinceName { get; set; } = null!;

    public virtual ICollection<District> Districts { get; set; } = new List<District>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
