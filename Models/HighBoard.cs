using Models.Enums;

namespace Models
{
    public class HighBoard
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public JobTitle JobTitle { get; set; }
        public string? Picture { get; set; }
        public Faculty Faculty { get; set; } = null!;
        public Department Department { get; set; } = null!;
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
