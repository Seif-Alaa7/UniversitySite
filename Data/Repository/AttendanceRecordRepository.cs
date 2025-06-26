using Models;
using Microsoft.EntityFrameworkCore;
using Data.Repository.IRepository;
using Microsoft.Identity.Client;
using System;

namespace Data.Repository
{
    public class AttendanceRecordRepository : IAttendanceRecordRepository
    {
        private readonly ApplicationDbContext context;
        public AttendanceRecordRepository(ApplicationDbContext db)
        {
            context = db;
        }

        // Implementations for CRUD Operations
        public async Task<AttendanceRecord> GetByIdAsync(int id)
        {
            return await context.attendanceRecords.FirstOrDefaultAsync(ar => ar.Id == id);
        }

        public async Task<IEnumerable<AttendanceRecord>> GetAllAsync()
        {
            return await context.attendanceRecords
                            .Include(ar => ar.Student)
                            .Include(ar => ar.Subject)
                            .Include(ar => ar.Doctor)
                            .ToListAsync();
        }

        public async Task AddAsync(AttendanceRecord entity)
        {
            await context.attendanceRecords.AddAsync(entity);
        }

        public void Update(AttendanceRecord entity)
        {
            context.attendanceRecords.Update(entity);
        }

        public void Delete(AttendanceRecord entity)
        {
            context.attendanceRecords.Remove(entity);
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }

        // Implementations for specific Attendance methods
        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceByStudentIdAsync(int studentId)
        {
            return await context.attendanceRecords
                            .Where(ar => ar.StudentId == studentId)
                            .Include(ar => ar.Student)
                            .Include(ar => ar.Subject)
                            .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceBySubjectIdAsync(int subjectId)
        {
            return await context.attendanceRecords
                            .Where(ar => ar.SubjectId == subjectId)
                            .Include(ar => ar.Student)
                            .Include(ar => ar.Subject)
                            .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceForCourseOnDateAsync(int subjectId, DateTime date)
        {
            // For DateTime comparison, use .Date to compare only the date part, ignoring time.
            return await context.attendanceRecords
                            .Where(ar => ar.SubjectId == subjectId && ar.AttendanceDate.Date == date.Date)
                            .Include(ar => ar.Student)
                            .Include(ar => ar.Subject)
                            .Include(ar => ar.Doctor)
                            .ToListAsync();
        }

        public async Task<bool> HasStudentAttendedSubjectOnDateAsync(int studentId, int subjectId, DateTime date)
        {
            return await context.attendanceRecords
                            .AnyAsync(ar => ar.StudentId == studentId &&
                                            ar.SubjectId == subjectId &&
                                            ar.AttendanceDate.Date == date.Date);
        }
        public Dictionary<DateTime, int> GetLectureNumbersBySubject(int subjectId)
        {
            var dates = context.attendanceRecords
                        .Where(ar => ar.SubjectId == subjectId)
                        .Select(ar => ar.AttendanceDate.Date)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();

            var dateToLectureMap = new Dictionary<DateTime, int>();
            int lectureCounter = 1;

            foreach (var date in dates)
            {
                dateToLectureMap[date] = lectureCounter;
                lectureCounter++;
            }

            return dateToLectureMap ?? new Dictionary<DateTime, int>();
        }
        public Dictionary<int, int> GetAttendanceCount(int subjectId)
        {
            var registeredStudents = context.StudentSubjects
                .Where(ss => ss.SubjectId == subjectId)
                .Select(ss => ss.StudentId)
                .Distinct()
                .ToList();

            var attendanceCounts = context.attendanceRecords
                .Where(ar => ar.SubjectId == subjectId)
                .GroupBy(ar => ar.StudentId)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new Dictionary<int, int>();

            foreach (var studentId in registeredStudents)
            {
                result[studentId] = attendanceCounts.ContainsKey(studentId) ? attendanceCounts[studentId] : 0;
            }

            return result;
        }
        public (int recommendedAttendance, double correlation) RecommendAttendanceBasedOnAnalysis(int subjectId)
        {
            var attendances = context.attendanceRecords
                .Where(a => a.SubjectId == subjectId)
                .GroupBy(a => a.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    AttendanceCount = g.Count()
                })
                .ToList();

            var grades = context.StudentSubjects
                .Where(s => s.SubjectId == subjectId)
                .Select(s => new
                {
                    s.StudentId,
                    s.Degree
                })
                .ToList();

            var combined = from a in attendances
                           join g in grades on a.StudentId equals g.StudentId
                           select new { a.AttendanceCount, g.Degree };

            if (combined.Count() < 3)
                return (0, 0); 

            var attendanceCounts = combined.Select(c => (double)c.AttendanceCount).ToList();
            var degrees = combined.Select(c => (double)c.Degree).ToList();

            double avgAttendance = attendanceCounts.Average();
            double avgDegree = degrees.Average();

            double numerator = 0;
            double denominatorLeft = 0;
            double denominatorRight = 0;

            for (int i = 0; i < combined.Count(); i++)
            {
                double attendanceDiff = attendanceCounts[i] - avgAttendance;
                double degreeDiff = degrees[i] - avgDegree;

                numerator += attendanceDiff * degreeDiff;
                denominatorLeft += Math.Pow(attendanceDiff, 2);
                denominatorRight += Math.Pow(degreeDiff, 2);
            }

            double correlation = numerator / Math.Sqrt(denominatorLeft * denominatorRight);

            int recommendedAttendance = 0;

            if (correlation > 0.4) 
            {
                var successful = combined.Where(c => c.Degree >= 60).ToList();
                recommendedAttendance = (int)Math.Ceiling(successful.Average(c => c.AttendanceCount));
            }

            return (recommendedAttendance, Math.Round(correlation, 2));
        }
    }
}
