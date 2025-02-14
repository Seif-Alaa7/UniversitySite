using Data.Repository.IRepository;
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

            /*var doctors = departmentRepository.GetDoctors(departmentId);

            foreach (var doctor in doctors)
            {
                var subjects = subjectRepository.SubjectsByDoctor(doctor.Id).ToList();

                var doctorData = new
                {
                    Doctor = doctor.Name, 
                    Subjects = subjects.Select(subject => new
                    {
                        Subject = subject.Name, 
                        Average = studentRepository.StudentsBySubject(subject.Id)
                            .Select(student => subjectRepository.ReturnDegrees(new List<Subject> { subject }, student.Id))
                            .Where(subjectDegrees => subjectDegrees.ContainsKey(subject.Id))
                            .Select(subjectDegrees => subjectDegrees[subject.Id])
                            .DefaultIfEmpty(0)
                            .Average()
                    }).ToArray() 
                };*/
            // جلب المواد المرتبطة بالقسم عبر جدول DepartmentSubjects
            var subjects = subjectRepository.GetSubjectsbyDepartment(departmentId).ToList();

            // جلب الأطباء الذين يدرّسون هذه المواد (من خلال DoctorId في جدول Subjects)
            var doctors = subjects.Select(s => s.DoctorId)
                                  .Distinct()
                                  .Select(doctorId => doctorRepository.GetOne(doctorId))
                                  .Where(d => d != null)
                                  .ToList();

            foreach (var doctor in doctors)
            {
                // جلب المواد التي يدرسها الطبيب
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
    }
}
