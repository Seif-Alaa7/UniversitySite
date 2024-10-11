using Microsoft.AspNetCore.Http;
using Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViewModels
{
    public class DoctorVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public Gender Gender { get; set; }
        public Religion Religion { get; set; }
        public string? Picture { get; set; }
        [NotMapped]
        public IFormFile? FormFile { get; set; }
        public string? Address { get; set; }
        public JobTitle JobTitle { get; set; }
    }
}
