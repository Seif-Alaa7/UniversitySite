using Models.Enums;
using System.Text.Json.Serialization;

namespace Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public Level Level { get; set; }
        public Semester Semester { get; set; }
        public int DoctorId { get; set; }
        public int SubjectHours { get; set; }
        public int StudentsAllowed { get; set; }
        public SummerStatus summerStatus { get; set; }
        public SubjectType subjectType { get; set; }
        public int Salary { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Department> Departments { get; set; } = new List<Department>();
        public List<DepartmentSubjects> DepartmentSubjects { get; set; } = new List<DepartmentSubjects>();
        [JsonIgnore]
        public List<StudentSubjects> StudentSubjects { get; set; } = new List<StudentSubjects>();
        public Doctor Doctor { get; set; } = null!;

    }
}
