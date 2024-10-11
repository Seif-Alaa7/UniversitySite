using HelwanUniversity.Vaildations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.FacultyVMs
{
    public class FacultyVm
    {
        public int Id { get; set; }
        public int DeanId { get; set; }
        [UniqueName]
        public string Name { get; set; } = null!;
        public string? Logo { get; set; }
        [NotMapped]
        [Display(Name = "If you want Change The Logo, Enter New Image here: ")]
        public IFormFile? LogoFile { get; set; }
        public string? Picture { get; set; }
        [NotMapped]
        [Display(Name = "If you want Change The Image, Enter New Image here: ")]
        public IFormFile? PictureFile { get; set; }
        public string? Description { get; set; }
        public int ViewCount { get; set; }
    }
}
