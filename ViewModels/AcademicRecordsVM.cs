using Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class AcademicRecordsVM
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
        public int CreditHours { get; private set; }
        public int RecordedHours { get; private set; }
        public int TotalHours { get; private set; }
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
        public Level Level { get; private set; }
        public Semester Semester { get; private set; }
    }
}
