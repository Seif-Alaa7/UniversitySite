using Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViewModels
{
    public class StudentSubjectsVM
    {
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int? Degree { get; set; }
        public Models.Enums.Grade? Grade { get; set; }
    
    }
}
