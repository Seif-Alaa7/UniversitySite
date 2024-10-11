using Microsoft.AspNetCore.Http;
using Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViewModels
{
    public class StudentVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Nationality { get; set; } = null!;
        public Gender Gender { get; set; }
        public Religion Religion { get; set; }
        public DateOnly BirthDate { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public int DepartmentId { get; set; }
        public string? Picture { get; set; }
        [NotMapped]
        public IFormFile? FormFile { get; set; }
        public DateTime AdmissionDate { get; set; } = DateTime.Now;
        public bool PaymentFees { get; set; }
        public DateTime? PaymentFeesDate { get; set; }
    }
}
