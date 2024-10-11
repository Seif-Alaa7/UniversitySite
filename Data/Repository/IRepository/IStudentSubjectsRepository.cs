using Models;
using Models.Enums;

namespace Data.Repository.IRepository
{
    public interface IStudentSubjectsRepository
    {
        List<StudentSubjects> StudentBySubject(int id);
        void Add(StudentSubjects studentsubject);
        void Save();
        bool Exist(int studentid, int subjectid);
        Grade CalculateGrade(int? degree);
        void Update(StudentSubjects model);
        decimal CalculateTotalPoints(int studentid);
        decimal CalculateSemesterPoints(int studentid, Semester semester);
        int CalculateCreditHours(int studentid);

        int CalculateRecordedHours(int studentid, Semester semester);
        int CalculateTotalHours(int studentid);
        Level CalculateLevel(int CreditHours);
        Semester Calculatesemester(int creditHours);
        StudentSubjects GetOne(int studentID, int subjectID);
        IQueryable<StudentSubjects> FindStudent(int StudentID);
        void Delete(StudentSubjects model);
    }
}
