using Models;
using Models.Enums;

namespace Data.Repository.IRepository
{
    public interface IAcademicRecordsRepository
    {
        List<AcademicRecords> GetAll();
        void Update(AcademicRecords academicRecords);
        void Save();
        decimal CalculateGpaSemester(int studentId, Semester semester);
        decimal CalculateGPATotal(int studentId);
        void DeleteByStudent(int studentId);
        AcademicRecords GetStudent(int id);
        Dictionary<int, (Level Level, Semester Semester)> GetLevelANDSemester(List<Student> students);
        public List<object> GetChartData(int departmentId);
    }
}
