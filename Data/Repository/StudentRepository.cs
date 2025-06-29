﻿using Data.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace Data.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IStudentSubjectsRepository studentSubjectsRepository;

        public StudentRepository(ApplicationDbContext context, IStudentSubjectsRepository studentSubjectsRepository)
        {
            this.context = context;
            this.studentSubjectsRepository = studentSubjectsRepository;
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
                .Include(s => s.Department)
                .ThenInclude(d => d.Faculty)
                .FirstOrDefault(s => s.Id == Id);
            return student;
        }
        public async Task<Student> GetByIdAsync(int id)
        {
            return await context.Students.FirstOrDefaultAsync(ar => ar.Id == id);
        }
        public string GetStudentName(int studentId)
        {
            return context.Students
                .Where(s => s.Id == studentId)
                .Select(s => s.Name)
                .FirstOrDefault() ?? "Unknown"; 
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
        public Dictionary<int, int> ReturnDegrees(IQueryable<Student> students, int Subjectid)
        {
            Dictionary<int, int> studentDegree = new Dictionary<int, int>();
            foreach (var student in students)
            {
                var studentSubjects = studentSubjectsRepository.GetOne(student.Id, Subjectid);
                if (studentSubjects != null)
                {
                    studentDegree[student.Id] = studentSubjects.Degree ?? 0;
                }
            }
            return studentDegree;
        }
        public Dictionary<int, Models.Enums.Grade> ReturnGrades(IQueryable<Student> students, int Subjectid)
        {
            Dictionary<int, Models.Enums.Grade> studentGrade = new Dictionary<int, Models.Enums.Grade>();
            foreach (var student in students)
            {
                var studentSubjects = studentSubjectsRepository.GetOne(student.Id, Subjectid);
                if (studentSubjects != null)
                {
                    studentGrade[student.Id] = studentSubjects.Grade ?? Models.Enums.Grade.F;
                }
            }
            return studentGrade;
        }
        public IEnumerable<StudentSubjects> GetStudentSubjects(int studentId)
        {
            return context.StudentSubjects
                          .Where(ss => ss.StudentId == studentId)
                          .Include(ss => ss.Subject)
                          .ToList();
        }
        public Student GetByUserId(string userId)
        {
            return context.Students.FirstOrDefault(s => s.ApplicationUserId == userId);
        }
        public Student GetAuthorizedStudent(string userId, int studentId)
        {
            var student = context.Students.FirstOrDefault(s => s.Id == studentId);

            if (student == null || student.ApplicationUserId != userId)
                return null;

            return student;
        }
        public bool IsStudentInDepartment(string userId, int departmentId)
        {
            return context.Students
                .Any(s => s.ApplicationUserId == userId && s.DepartmentId == departmentId);
        }
        public bool IsStudent(string userId)
        {
            return context.Students.Any(s => s.ApplicationUserId == userId);
        }
    }
}
