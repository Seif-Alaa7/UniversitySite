using Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using HelwanUniversity.Vaildations;

namespace ViewModels.UniFileVMs
{
    public class UniFileVM
    {
        //Add
        public int Id { get; set; }
        [UniqueFile]
        public string File { get; set; } = null!;
        [NotMapped]
        [Display(Name = "Put the New Image Here: ")]
        public IFormFile? ImgPath { get; set; }
        public Filetype ContentType { get; set; }
    }
}
