using Data.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace Data.Repository
{
    public class DepartmentSubjectsRepository : IDepartmentSubjectsRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IStudentSubjectsRepository studentSubjectsRepository;
        public DepartmentSubjectsRepository(ApplicationDbContext context, IStudentSubjectsRepository studentSubjectsRepository)
        {
            this.context = context;
            this.studentSubjectsRepository = studentSubjectsRepository;
        }
        public List<DepartmentSubjects> subjectsByDepartment(int id)
        {
            var subjects = context.DepartmentSubjects
                          .Where(ds => ds.DepartmentId == id)
                          .Include(ds => ds.Subject)
                          .ToList();

            return subjects;
        }

        public bool Exist(DepartmentSubjects model)
        {
            var ExistDepartmentSubject = context.DepartmentSubjects.Any(a => a.DepartmentId == model.DepartmentId && a.SubjectId == model.SubjectId);
            return ExistDepartmentSubject;
        }
        public void Add(DepartmentSubjects model)
        {
            context.DepartmentSubjects.Add(model);
        }
        public List<DepartmentSubjects> GetAll()
            {
                var list = context.DepartmentSubjects.ToList(); 
               return list;
            }
        public void Delete(DepartmentSubjects model)
        {
            context.DepartmentSubjects.Remove(model);
        }
        public void Save()
        {
            context.SaveChanges();
        }
        public List<DepartmentSubjects> SubjectDepartments(int subjectId)
        {
            var Departments = context.DepartmentSubjects.Where(x=>x.SubjectId == subjectId).ToList();
            return Departments;
        }
        public DepartmentSubjects? DeleteRelation(int subjectId, int DepartmentId)
        {
            var Relation = context.DepartmentSubjects.FirstOrDefault(x => x.SubjectId == subjectId && x.DepartmentId == DepartmentId);
            return Relation;
        }
        public List<DepartmentSubjects> StudentSubjects(Level level, Semester semester, int DepartmentId)
        {
            var Subjects = context.DepartmentSubjects.Include(x => x.Subject).Where(x => x.DepartmentId == DepartmentId);
            var Studentsubject = Subjects.Where(x => x.Subject.Level == level && x.Subject.Semester == semester).ToList();

            return Studentsubject;
        }
        public List<DepartmentSubjects> GetDepartmentSubjects(List<int> subjectIds)
        {
           var MOdels = context.DepartmentSubjects
                .Where(ds => subjectIds.Contains(ds.SubjectId))
                .ToList();

           return MOdels;
        }
        public Dictionary<int,List<int>> GetDepartmentsSubject(List<Subject> subjects,List<DepartmentSubjects> departmentSubjects)
        {
            var subjectDepartments = new Dictionary<int, List<int>>();

            foreach (var subject in subjects)
            {
                var departmentIds = departmentSubjects
                    .Where(ds => ds.SubjectId == subject.Id)
                    .Select(ds => ds.DepartmentId)
                    .ToList();

                subjectDepartments[subject.Id] = departmentIds;
            }
            return subjectDepartments;
        }
        public Dictionary<int,int> StudentCounts(List<DepartmentSubjects> subjects)
        {
            var studentsBySubject = new Dictionary<int, int>();
            foreach (var subject in subjects)
            {
                int studentCount = studentSubjectsRepository.StudentBySubject(subject.Subject.Id).Count;
                studentsBySubject[subject.Subject.Id] = studentCount;
            }
            return studentsBySubject;
        }
    }
}
