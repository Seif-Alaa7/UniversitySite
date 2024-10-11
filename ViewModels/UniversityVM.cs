using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViewModels
{
    public class UniversityVM
    {
        public int Id { get; set; }
        [Display(Name = "Name")]
        public string Name { get; set; } = null!;
        [Display(Name = "Description")]
        public string Description { get; set; } = null!;
        [Display(Name = "Current Logo")]
        public string Logo { get; set; } = null!;
        [NotMapped]
        [Display(Name = "If you want Change The Logo, Enter New Logo here: ")]
        public IFormFile? LogoFile { get; set; }
        public string MainPicture { get; set; } = null!;
        [NotMapped]
        [Display(Name = "If you want Change The Image, Enter New Image here: ")]
        public IFormFile? MainPictureFile { get; set; }

        [Display(Name = "Facebook Page")]
        public string FacebookPage { get; set; } = null!;
        [Display(Name = "LinkedIn Page")]
        public string LinkedInPage { get; set; } = null!;
        [Display(Name = "Main Page")]
        public string MainPage { get; set; } = null!;
        [Display(Name = "Contact Mail")]
        [DataType(DataType.EmailAddress)]
        public string ContactMail { get; set; } = null!;
        [Display(Name = "Historical Background")]
        public string HistoricalBackground { get; set; } = null!;
        public int ViewCount { get; set; }
        public string GoogleForm { get; set; } = null!;
    }
}
