using Data.Repository.IRepository;
using Models;
using Models.Enums;

namespace Data.Repository
{
    public class AcademicRecordsRepository : IAcademicRecordsRepository
    {
        private readonly ApplicationDbContext context;

        public AcademicRecordsRepository(ApplicationDbContext context)
        {
            this.context = context;
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
    }
}
