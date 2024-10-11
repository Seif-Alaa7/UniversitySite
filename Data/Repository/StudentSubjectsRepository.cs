using Data.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace Data.Repository
{
    public class StudentSubjectsRepository : IStudentSubjectsRepository
    {
        private readonly ApplicationDbContext context;
        public StudentSubjectsRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public List<StudentSubjects> StudentBySubject(int id)
        {
            var students = context.StudentSubjects
                          .Where(ds => ds.SubjectId == id)
                          .Include(ds => ds.Student)
                          .ToList();

            return students;
        }
        public void Add(StudentSubjects studentsubject)
        {
            context.StudentSubjects.Add(studentsubject);
        }
        public StudentSubjects GetOne(int studentID,int subjectID)
        {
            var students = context.StudentSubjects.FirstOrDefault(x=>x.StudentId == studentID && x.SubjectId == subjectID);
            return students;
        }
        public void Save()
        {
            context.SaveChanges();
        }
        public bool Exist(int studentid, int subjectid)
        {
            var exist = context.StudentSubjects.Any(x=>x.StudentId == studentid &&  x.SubjectId == subjectid);
            return exist;
        }
        public void Update(StudentSubjects model)
        {
            context.Update(model);  
        }
        public void Delete(StudentSubjects model) 
        { 
            context.Remove(model);
        }
        public IQueryable<StudentSubjects> FindStudent(int StudentID)
        {
            var studentSubjects = context.StudentSubjects.Where(x=>x.StudentId==StudentID);
            return studentSubjects;
        }

        public Grade CalculateGrade(int? degree)
        {
            if (degree <= 100 && degree >= 90)
            {
                return Grade.APlus;
            }
            else if (degree >= 85 && degree < 90)
            {
                return Grade.A;
            }
            else if (degree >= 80 && degree< 85)
            {
                return Grade.BPlus;
            }
            else if(degree >= 75 && degree<80)
            {
                return Grade.B;
            }
            else if(degree >= 70 && degree < 75)
            {
                return Grade.CPlus;
            }
            else if(degree >=65 && degree < 70)
            {
                return Grade.C;
            
            }else if(degree >= 60 && degree < 65)
            {
                return Grade.D;
            }
            else
            {
                return Grade.F;
            }
        }
        public decimal CalculateTotalPoints(int studentid)
        {
           var points = context.StudentSubjects
            .Where(ss => ss.StudentId == studentid)
            .Sum(ss => ss.DegreePoints * ss.Subject.SubjectHours);

            return points;
        }
        public decimal CalculateSemesterPoints(int studentid, Semester semester)
        {
            var SemesterPoints = context.StudentSubjects
                .Where(ss => ss.StudentId == studentid && ss.Subject.Semester == semester)
                .Sum(ss => ss.DegreePoints * ss.Subject.SubjectHours);

            return SemesterPoints;
        }
        public int CalculateCreditHours(int studentid)
        {
            var totalCreditHours = context.StudentSubjects
                .Where(ss => ss.StudentId == studentid && ss.Degree >= 60)
                .Sum(ss => ss.Subject.SubjectHours);

            return totalCreditHours;
        }
        public int CalculateRecordedHours(int studentid,Semester semester)
        {
            var Hours = context.StudentSubjects
                .Where(ss => ss.StudentId == studentid && ss.Subject.Semester == semester)
                .Sum(ss => ss.Subject.SubjectHours);

           return Hours;
        }
        public int CalculateTotalHours(int studentid)
        {
            var hours = context.StudentSubjects
            .Where(ss => ss.StudentId == studentid)
            .Sum(ss => ss.Subject.SubjectHours);

            return hours;
        }
        public Level CalculateLevel(int CreditHours)
        {
           if(CreditHours < 36)
           {
                return Level.First;
           }
            else if(CreditHours >= 36 &&  CreditHours < 72)
            {
                return Level.Second;  
            }
           else if(CreditHours >= 72 &&  CreditHours< 108)
           {
                return Level.Third;
           }
           else if(CreditHours >= 108 && CreditHours <= 144)
           {
                return Level.Fourth;
           }
            else
            {
                return Level.First;
            }
        }
        public Semester Calculatesemester(int creditHours)
        {
            if (creditHours < 18)
            {
                return Semester.First;
            }
            else if (creditHours >= 18 && creditHours < 36)
            {
                return Semester.Second;
            }
            else if (creditHours >= 36 && creditHours < 54)
            {
                return Semester.Third;
            }
            else if (creditHours >= 54 && creditHours < 72)
            {
                return Semester.Fourth;
            }
            else if (creditHours >= 72 && creditHours < 90)
            {
                return Semester.Fifth;
            }
            else if (creditHours >= 90 && creditHours < 108)
            {
                return Semester.Sixth;
            }
            else if (creditHours >= 108 && creditHours < 126)
            {
                return Semester.Seventh;
            }
            else if (creditHours >= 126 && creditHours <= 144)
            {
                return Semester.Eighth;
            }
            else
            {
                return Semester.First;
            }
        }
    }
}
