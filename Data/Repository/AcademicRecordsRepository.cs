using Data.Repository.IRepository;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        public decimal FindAvgGPAByDepartmentAndFilters(int departmentId, Level? level, Gender? gender)
        {
            var studentQuery = context.Students.AsQueryable();

            studentQuery = studentQuery.Where(s => s.DepartmentId == departmentId);

            if (gender.HasValue)
            {
                studentQuery = studentQuery.Where(s => s.Gender == gender.Value);
            }

            var studentIds = studentQuery.Select(s => s.Id).ToList();

            var recordsQuery = context.academicRecords.AsQueryable()
                .Where(ar => studentIds.Contains(ar.StudentId));

            if (level.HasValue)
            {
                recordsQuery = recordsQuery.Where(ar => ar.Level == level.Value);
            }

            var records = recordsQuery.ToList();

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

        public decimal FindAvgGPAByDepartment(int departmentId)
        {
            return FindAvgGPAByDepartmentAndFilters(departmentId, null, null);
        }
        public decimal FindAvgGPAByDepartmentAndLevel(int departmentId, Level level)
        {
            return FindAvgGPAByDepartmentAndFilters(departmentId, level, null);
        }
        public decimal FindAvgGPAByDepartmentLevelGender(int departmentId, Level level, Gender gender)
        {
            return FindAvgGPAByDepartmentAndFilters(departmentId, level, gender);
        }
        public List<(string SubjectName, double AvgGpa)> GetLowestAvgGpaSubjectsByDepartment(int departmentId, int top)
        {
            var students = studentRepository.GetStudents(departmentId).ToList();

            var studentIds = students.Select(s => s.Id).ToList();

            var studentSubjects = context.StudentSubjects
                .Where(ss => studentIds.Contains(ss.StudentId))
                .ToList();

            var subjects = subjectRepository.GetAll();

            var grouped = studentSubjects
                .GroupBy(ss => ss.SubjectId)
                .OrderBy(g => g.Average(x => x.DegreePoints))
                .Take(top);

            var result = new List<(string SubjectName, double AvgGpa)>();
            foreach (var group in grouped)
            {
                var subject = subjects.FirstOrDefault(s => s.Id == group.Key);
                string name = subject?.Name ?? "Unknown";
                double avg = (double)group.Average(x => x.DegreePoints);
                result.Add((name, avg));
            }

            return result;
        }
    }
}
