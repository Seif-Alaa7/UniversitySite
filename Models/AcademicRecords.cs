using Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class AcademicRecords
    {
        private decimal _GPASemester;
        private decimal _GPATotal;
        private decimal _SemesterPoints;
        private decimal _TotalPoints;

        public int Id { get; set; }
        public int StudentId { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal GPASemester
        {
            get => _GPASemester;
            set => _GPASemester = Math.Round(value, 3);
        }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal GPATotal
        {
            get => _GPATotal;
            set => _GPATotal = Math.Round(value, 3);
        }
        public int CreditHours { get; set; }
        public int RecordedHours { get; set; }
        public int TotalHours { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal SemesterPoints
        {
            get => _SemesterPoints;
            set => _SemesterPoints = Math.Round(value, 3);
        }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal TotalPoints
        {
            get => _TotalPoints;
            set => _TotalPoints = Math.Round(value, 3);
        }
        public Level Level { get; set; }
        public Semester Semester { get; set; }
        public Student Student { get; set; } = null!;
    }
}
