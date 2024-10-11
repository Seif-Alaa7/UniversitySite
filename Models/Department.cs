namespace Models
{
    public class Department
    {
        public int Id { get; set; }
        public int HeadId { get; set; }
        public string Name { get; set; } = null!;
        public int FacultyId { get; set; }
        public int Allowed {  get; set; }
        public Faculty Faculty { get; set; } = null!;
        public HighBoard? HighBoard { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Subject> Subjects { get; set; } = new List<Subject>();
        public List<DepartmentSubjects> DepartmentSubjects { get; set; } = new List<DepartmentSubjects>();
    }
}
