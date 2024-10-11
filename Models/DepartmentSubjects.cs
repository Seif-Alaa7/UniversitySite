namespace Models
{
    public class DepartmentSubjects
    {
        public int DepartmentId { get; set; }
        public int SubjectId { get; set; }
        public Department Department { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
    }
}
