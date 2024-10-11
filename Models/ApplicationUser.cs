using Microsoft.AspNetCore.Identity;
using Models.Enums;

namespace Models
{
    public class ApplicationUser : IdentityUser
    {
        public UserType UserType { get; set; }
        public Student? Student { get; set; }
        public Doctor? Doctor { get; set; }
        public HighBoard? HighBoard { get; set; }
    }
}
