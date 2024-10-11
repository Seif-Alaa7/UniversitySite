using Models;

namespace Data.Repository.IRepository
{
    public interface IStudentRepository
    {
        List<Student> GetAll();
        Student GetOne(int Id);
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
    }
}
