using Microsoft.AspNetCore.Http;
using Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViewModels
{
    public class HighBoardVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public JobTitle JobTitle { get; set; }
        public string? Picture { get; set; }
        [NotMapped]
        public IFormFile? FormFile { get; set; }
    }
}
