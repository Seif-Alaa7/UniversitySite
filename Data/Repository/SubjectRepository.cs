using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace Data.Repository
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IFacultyRepository facultyRepository;
        private readonly IStudentSubjectsRepository studentSubjectsRepository;

        public SubjectRepository(ApplicationDbContext context, IFacultyRepository facultyRepository, IStudentSubjectsRepository studentSubjectsRepository)
        {
            this.context = context;
            this.facultyRepository = facultyRepository;
            this.studentSubjectsRepository = studentSubjectsRepository;
        }
        public void Add(Subject subject)
        {
            context.Subjects.Add(subject);
        }

        public void Update(Subject subject)
        {
            context.Subjects.Update(subject);
        }

        public void Delete(Subject subject)
        {
            context.Subjects.Remove(subject);
        }

        public Subject GetOne(int Id)
        {
            var subject = context.Subjects
                .Find(Id);
            return subject;
        }
        public List<SelectListItem> Select()
        {
            var list = context.Subjects.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList();

            return list;
        }
        public List<SelectListItem> SelectSubjectsByFaculty(int id)
        {
            var subjects = facultyRepository.GetSubjects(id).Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList();
            return subjects;
        }
        public void Save()
        {
            context.SaveChanges();
        }
        public bool ExistSubject(string Subject)
        {
            var exist = context.Subjects.Any(x => x.Name == Subject);
            return exist;
        }
        public List<Subject> GetSubjects(int studentID)
        {
            try
            {
                var subjects = context.StudentSubjects
                                      .Where(x => x.StudentId == studentID)
                                      .Include(ds => ds.Subject)
                                      .AsNoTracking()
                                      .ToList();

                var list = subjects.Select(x => x.Subject).ToList();
                return list;

            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching subjects for student ID {studentID}", ex);
            }
        }
        public IQueryable<Subject> SubjectsByDoctor(int id)
        {
            var Subjects = context.Subjects.Where(x => x.DoctorId == id);
            return Subjects;
        }
        public string GetName(int id)
        {
            var name = context.Subjects.FirstOrDefault(x => x.Id == id)?.Name;
            return name;
        }
        public Level GetLevel(int id)
        {
            var level = context.Subjects.FirstOrDefault(x => x.Id == id).Level;
            return level;
        }
        public Semester GetSemester(int id)
        {
            var semester = context.Subjects.FirstOrDefault(x => x.Id == id).Semester;
            return semester;
        }
        public Dictionary<int , string> GetName(List<Subject> subjects)
        {
            var SubjectsDictionary = context.Subjects
                .ToList().ToDictionary(x => x.Id, x => x.Name);

            var SubjectNames = new Dictionary<int, string>();
            foreach (var subject in subjects)
            {
                string SubjectName;
                if (SubjectsDictionary.TryGetValue(subject.Id, out SubjectName))
                {
                    SubjectNames[subject.Id] = SubjectName;
                }
            }
            return SubjectNames;
        }
        public List<int> GetIds(List<Subject> subjects)
        {
            var Ids =  subjects.Select(s => s.Id).ToList();
            return Ids;
        }
        public List<StudentSubjects> GetStudentGrades(int subjectId)
        {
            var grades = context.StudentSubjects
                .Where(ss => ss.SubjectId == subjectId)
        .Include(ss => ss.Student)
        .Select(ss => new StudentSubjects
        {
            StudentId = ss.Student.Id,
            Degree = ss.Degree,
            Grade=ss.Grade
        })
        .ToList();
            return grades;
        }
        public Dictionary<int, int> ReturnDegrees(List<Subject> subjects, int studentid)
        {
            Dictionary<int, int> subjectDegree = new Dictionary<int, int>();
            foreach (var subject in subjects)
            {
                var studentSubjects = studentSubjectsRepository.GetOne(studentid,subject.Id);
                if (studentSubjects != null)
                {
                    subjectDegree[subject.Id] = studentSubjects.Degree ?? 0;
                }
            }
            return subjectDegree;
        }
        public Dictionary<int, Models.Enums.Grade> ReturnGrades(List<Subject> subjects, int studentid)
        {
            Dictionary<int, Models.Enums.Grade> studentGrade = new Dictionary<int, Models.Enums.Grade>();
            foreach (var subject in subjects)
            {
                var studentSubjects = studentSubjectsRepository.GetOne(studentid, subject.Id);
                if (studentSubjects != null)
                {
                    studentGrade[subject.Id] = studentSubjects.Grade ?? Models.Enums.Grade.F;
                }
            }
            return studentGrade;
        }
    }
}
