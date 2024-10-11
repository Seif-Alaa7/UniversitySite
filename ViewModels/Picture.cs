using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class Picture
    {
        public int Id { get; set; } 
        public string MainPicture { get; set; } = null!;
        [NotMapped]
        [Display(Name = "If you want Change The Image, Enter New Image here: ")]
        public IFormFile? MainPictureFile { get; set; }
    }
}
