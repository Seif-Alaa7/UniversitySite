using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Models.Enums;

namespace ViewModels
{
    public class SubjectVM
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
        public int departmentId { get; set; }
        public int OriginalDepartmentId { get; set; }
    }
}
