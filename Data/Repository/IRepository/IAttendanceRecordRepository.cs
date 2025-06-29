using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.IRepository
{
    public interface IAttendanceRecordRepository
    {
        Task<AttendanceRecord> GetByIdAsync(int id);
        Task<IEnumerable<AttendanceRecord>> GetAllAsync();
        Task AddAsync(AttendanceRecord entity);
        void Update(AttendanceRecord entity);
        void Delete(AttendanceRecord entity);
        Task SaveAsync();
        Task<IEnumerable<AttendanceRecord>> GetAttendanceByStudentIdAsync(int studentId);
        Task<IEnumerable<AttendanceRecord>> GetAttendanceBySubjectIdAsync(int subjectId);
        Task<IEnumerable<AttendanceRecord>> GetAttendanceForCourseOnDateAsync(int subjectId, DateTime date);
        Task<bool> HasStudentAttendedSubjectOnDateAsync(int studentId, int subjectId, DateTime date);
        Dictionary<DateTime, int> GetLectureNumbersBySubject(int subjectId);
        Dictionary<int, int> GetAttendanceCount(int subjectId);
        public (int recommendedAttendance, double correlation) RecommendAttendanceBasedOnAnalysis(int subjectId);
    }
}
