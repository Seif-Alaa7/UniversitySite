using Data.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace Data.Repository
{
    public class AcademicRecordsRepository : IAcademicRecordsRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IDepartmentRepository departmentRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IDoctorRepository doctorRepository;

        public AcademicRecordsRepository(ApplicationDbContext context,IDepartmentRepository departmentRepository
            ,ISubjectRepository subjectRepository,IStudentRepository studentRepository, IDoctorRepository doctorRepository)
        {
            this.context = context;
            this.departmentRepository = departmentRepository;
            this.studentRepository = studentRepository;
            this.doctorRepository = doctorRepository;
            this.subjectRepository = subjectRepository;
        }

        public void Update(AcademicRecords academicRecords)
        {
            context.academicRecords.Update(academicRecords);
        }

        public List<AcademicRecords> GetAll()
        {
            var AcademicRecords = context.academicRecords
                .ToList();
            return AcademicRecords;
        }

        public void Save()
        {
            context.SaveChanges();
        }
        public decimal CalculateGpaSemester(int studentId, Semester semester)
        {
            var academicRecord = context.academicRecords
                 .FirstOrDefault(ar => ar.StudentId == studentId && ar.Semester == semester);

            if (academicRecord == null || academicRecord.RecordedHours == 0)
                return 0;

            return academicRecord.SemesterPoints / academicRecord.RecordedHours;
        }
        public decimal CalculateGPATotal(int studentId)
        {
            var academicRecord = context.academicRecords
                .Where(ar => ar.StudentId == studentId)
                .GroupBy(ar => ar.StudentId)
                .Select(g => new
                {
                    TotalPoints = g.Sum(ar => ar.TotalPoints),
                    TotalHours = g.Sum(ar => ar.TotalHours)
                })
                .FirstOrDefault();

            if (academicRecord == null || academicRecord.TotalHours == 0)
                return 0;

            return academicRecord.TotalPoints / academicRecord.TotalHours;
        }
        public void DeleteByStudent(int studentId)
        {
           var link =  context.academicRecords.FirstOrDefault(x=>x.StudentId == studentId);
           context.academicRecords.Remove(link);
        }
        public AcademicRecords GetStudent(int id)
        {
            var Student = context.academicRecords.FirstOrDefault(x => x.StudentId == id);
            return Student;
        }
        public Dictionary<int, (Level Level, Semester Semester)> GetLevelANDSemester(List<Student> students)
        {
            var StudentsDictionary = context.academicRecords.ToList()
                  .ToDictionary(x => x.StudentId, x => new { x.Level, x.Semester });

            var records = new Dictionary<int, (Level Level, Semester Semester)>();
            foreach (var student in students)
            {
                if (StudentsDictionary.TryGetValue(student.Id, out var record))
                {
                    records[student.Id] = (record.Level, record.Semester);
                }
            }
            return records;
        }
        public List<object> GetChartData(int departmentId)
        {
            var chartData = new List<object>();

            var subjects = subjectRepository.GetSubjectsbyDepartment(departmentId).ToList();

            var doctors = subjects.Select(s => s.DoctorId)
                                  .Distinct()
                                  .Select(doctorId => doctorRepository.GetOne(doctorId))
                                  .Where(d => d != null)
                                  .ToList();

            foreach (var doctor in doctors)
            {
                var doctorSubjects = subjects.Where(s => s.DoctorId == doctor.Id).ToList();

                var doctorData = new
                {
                    Doctor = doctor.Name,
                    Subjects = doctorSubjects.Select(subject => new
                    {
                        Subject = subject.Name,
                        Average = studentRepository.StudentsBySubject(subject.Id)
                        .ToList()
                            .Select(student => subjectRepository.ReturnDegrees(new List<Subject> { subject }, student.Id))
                            .Where(subjectDegrees => subjectDegrees.ContainsKey(subject.Id))
                            .Select(subjectDegrees => subjectDegrees[subject.Id])
                            .DefaultIfEmpty(0)
                            .Average()
                    }).ToArray()
                };

                chartData.Add(doctorData);
            }

            return chartData; 
        }
        public decimal FindAvgGPAByDepartmentAndFilters(int departmentId, Level level, Gender gender)
        {
            var studentIds = context.Students
             .Where(s => s.DepartmentId == departmentId && s.Gender == gender)
             .Select(s => s.Id)
             .ToList();

            var records = context.academicRecords
                .Where(ar => studentIds.Contains(ar.StudentId) && ar.Level == level)
                .ToList();

            var groupedGpas = records
                .GroupBy(ar => ar.StudentId)
                .Select(g =>
                {
                    var totalPoints = g.Sum(ar => ar.TotalPoints);
                    var totalHours = g.Sum(ar => ar.TotalHours);
                    return totalHours > 0 ? (totalPoints / totalHours) : 0;
                })
                .Where(gpa => gpa > 0)
                .ToList();

            return groupedGpas.Any() ? groupedGpas.Average() : 0;
        }
    }
}
