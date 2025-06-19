using Models;

namespace Data.Repository.IRepository
{
    public interface IStudentRepository
    {
        List<Student> GetAll();
        Student GetOne(int Id);
        Task<Student> GetByIdAsync(int id);
        string GetStudentName(int studentId);
        void Delete(int id);
        void Update(Student student);
        void Save();
        bool Exist(string Name);
        bool ExistPhone(string Phone);
        List<Student> GetStudents(int id);
        IQueryable<Student> StudentsBySubject(int id);
        public void DeleteUser(string id);
        List<Student> TrueFees();
        List<Student> FalseFees();
        Dictionary<int, int> ReturnDegrees(IQueryable<Student> students, int Subjectid);
        Dictionary<int, Models.Enums.Grade> ReturnGrades(IQueryable<Student> students, int Subjectid);
        IEnumerable<StudentSubjects> GetStudentSubjects(int studentId);
        Student GetByUserId(string userId);
        Student GetAuthorizedStudent(string userId, int studentId);
        bool IsStudentInDepartment(string userId, int departmentId);
    }
}
