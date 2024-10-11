using Data.Repository.IRepository;
using Models;

namespace Data.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext context;

        public StudentRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public void Update(Student student)
        {
            context.Students.Update(student);
        }

        public void Delete(int id)
        {
            var student = GetOne(id);
            context.Students.Remove(student);
        }
        public void DeleteUser(string id)
        {
            var user = context.Users.SingleOrDefault(u => u.Id == id);
            if (user != null)
            {
                context.Users.Remove(user);
            }
        }
        public Student GetOne(int Id)
        {
            var student = context.Students
                .Find(Id);
            return student;
        }

        public List<Student> GetAll()
        {
            var students = context.Students
                .ToList();
            return students;
        }

        public void Save()
        {
            context.SaveChanges();
        } 
        public bool Exist(string Name)
        {
           var EXIST = context.Students.Any(x => x.Name == Name);
           return EXIST;
        }
        public bool ExistPhone (string Phone)
        {
            var EXIST = context.Students.Any(x => x.PhoneNumber == Phone);
            return EXIST;
        }
        public List<Student> GetStudents(int id)
        {
            var students = context.Students.Where(x=>x.DepartmentId == id).ToList();    
            return students;
        }
        public IQueryable<Student> StudentsBySubject(int id)
        {
            var list = context.StudentSubjects.Where(x => x.SubjectId == id).Select(x => x.Student);
            return list;
        }
        public List<Student> TrueFees()
        {
            var Students = context.Students.Where(x=>x.PaymentFees == true).ToList();
            return Students;
        }
        public List<Student> FalseFees()
        {
            var Students = context.Students.Where(x => x.PaymentFees == false).ToList();
            return Students;
        }
    }
}
