using Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class StudentSubjects
    {
        private decimal _DegreePoints;

        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int? Degree { get; set; }
        public Grade? Grade { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal DegreePoints
        {
            get => _DegreePoints;
            set => _DegreePoints = Math.Round(value, 3);
        }
        public string? StudentName { get; set; }
        public Student Student { get; set; } = null!;
        public Subject Subject { get; set; } = null!;

    }
}
