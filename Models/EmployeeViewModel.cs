namespace OceanTechLevel1.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age { get; set; }
        public string CitizenId { get; set; }
        public string PhoneNumber { get; set; }
        public Ethnicity Ethnicity { get; set; }
        public Occupation Occupation { get; set; }
        public Position Position { get; set; }
        public Province Province { get; set; }
        public District District { get; set; }
        public Commune Commune { get; set; }
        public string MoreInfo { get; set; }
        public int QualificationCount { get; set; }  // Số lượng văn bằng
    }

}
