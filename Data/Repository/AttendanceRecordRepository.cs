using Models;
using Microsoft.EntityFrameworkCore;
using Data.Repository.IRepository;

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
                            .Include(ar => ar.Doctor)
                            .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceBySubjectIdAsync(int subjectId)
        {
            return await context.attendanceRecords
                            .Where(ar => ar.SubjectId == subjectId)
                            .Include(ar => ar.Student)
                            .Include(ar => ar.Subject)
                            .Include(ar => ar.Doctor)
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
    }
}
