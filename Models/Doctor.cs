using Models.Enums;

namespace Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public Gender Gender { get; set; }
        public Religion Religion { get; set; }
        public string? Picture { get; set; }
        public string? Address { get; set; }
        public JobTitle JobTitle { get; set; }
        public List<Subject> Subjects { get; set; } = new List<Subject>();
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
