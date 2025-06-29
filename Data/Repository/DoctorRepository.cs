﻿using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;
using Stripe;
using System.Linq.Expressions;
using System.Numerics;

namespace Data.Repository
{
    public class DoctorRepository :IDoctorRepository
    {
        private readonly ApplicationDbContext context;
        private readonly ISubjectRepository subjectRepository;  

        public DoctorRepository(ApplicationDbContext context, ISubjectRepository subjectRepository)
        {
            this.context = context;
            this.subjectRepository = subjectRepository;
        }

        public void Update(Doctor doctor)
        {
            context.Doctors.Update(doctor);
        }

        public void Delete(int id)
        {
            var doctor = GetOne(id);
            context.Doctors.Remove(doctor);
        }
        public void DeleteUser(string id)
        {
            var user = context.Users.SingleOrDefault(u => u.Id == id);
            if (user != null)
            {
                context.Users.Remove(user);
            }
        }

        public Doctor GetOne(int Id)
        {
            var doctor = context.Doctors
                .Find(Id);
            return doctor;
        }
        public async Task<Doctor> GetByIdAsync(int id)
        {
            return await context.Doctors.FirstOrDefaultAsync(ar => ar.Id == id);
        }
        public List<SelectListItem> Select()
        {
            var options =  context.Doctors.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name

            }).ToList();
            return options;
        }

        public List<Doctor> GetAll()
        {
            var doctors = context.Doctors
                .ToList();
            return doctors;
        }

        public void Save()
        {
            context.SaveChanges();
        }
        public bool ExistName(string Name)
        {
            var exist = context.Doctors.Any(x => x.Name == Name);
            return exist;
        }
        public Dictionary<int , string> GetName(List<Subject> subjects)
        {
            var doctorDictionary = context.Doctors
                .ToList().ToDictionary(x => x.Id, x => x.Name);

            var doctorNames = new Dictionary<int, string>();
            foreach (var subject in subjects)
            {
                string doctorName;
                if (doctorDictionary.TryGetValue(subject.DoctorId, out doctorName))
                {
                    doctorNames[subject.DoctorId] = doctorName;
                }
            }
            return doctorNames;
        }
        public Dictionary<int, string> GetName(List<DepartmentSubjects> subjects)
        {
            var doctorDictionary = context.Doctors
                .ToList().ToDictionary(x => x.Id, x => x.Name);

            var doctorNames = new Dictionary<int, string>();
            foreach (var subject in subjects)
            {
                string doctorName;
                if (doctorDictionary.TryGetValue(subject.Subject.DoctorId, out doctorName))
                {
                    doctorNames[subject.Subject.DoctorId] = doctorName;
                }
            }
            return doctorNames;
        }
        public Dictionary<int,List<string>> GetSubjects(List<Doctor> Doctors)
        {
            var subjectsDoctor = new Dictionary<int, List<string>>();

            foreach (var Doctor in Doctors)
            {
                var SubjectNames = context.Subjects
                    .Where(ds => ds.DoctorId == Doctor.Id)
                    .Select(ds => ds.Name)
                    .ToList();

                subjectsDoctor[Doctor.Id] = SubjectNames;
            }
            return subjectsDoctor;
        }
        public Dictionary<int, List<string>> GetDepartments(List<Doctor> doctors)
        {
            var doctorDepartments = new Dictionary<int, List<string>>();

            foreach (var doctor in doctors)
            {
                var departmentIds = context.DepartmentSubjects
                    .Where(ds => ds.Subject.DoctorId == doctor.Id)
                    .Select(ds => ds.DepartmentId)
                    .Distinct()
                    .ToList();

                var departments = context.Departments
                    .Where(d => departmentIds.Contains(d.Id))
                    .Select(d => d.Name)
                    .ToList();

                doctorDepartments[doctor.Id] = departments;
            }
            return doctorDepartments;
        }
        public Dictionary<int, List<string>> GetColleges(List<Doctor> doctors)
        {
            var doctorColleges = new Dictionary<int, List<string>>();

            foreach (var doctor in doctors)
            {
                var departmentIds = context.DepartmentSubjects
                    .Where(ds => ds.Subject.DoctorId == doctor.Id)
                    .Select(ds => ds.DepartmentId)
                    .Distinct()
                    .ToList();

                var collegeIds = context.Departments
                    .Where(d => departmentIds.Contains(d.Id))
                    .Select(d => d.FacultyId)
                    .Distinct()
                    .ToList();

                var colleges = context.Faculties
                    .Where(c => collegeIds.Contains(c.Id))
                    .Select(c => c.Name)
                    .ToList();

                doctorColleges[doctor.Id] = colleges;
            }
            return doctorColleges;
        }
        public async Task<object?> GetEntityByUserIdAsync(string userId)
        {
            var doctor = await context.Doctors
                .FirstOrDefaultAsync(d => d.ApplicationUserId == userId);
            if (doctor != null && (doctor.JobTitle == JobTitle.Doctor || doctor.JobTitle == JobTitle.Prof_Doctor))
            {
                return doctor;
            }
            var highboard = await context.HighBoards
                .FirstOrDefaultAsync(h => h.ApplicationUserId == userId);
            if (highboard != null)
            {
                return highboard;
            }
            return null;
        }
        public async Task<T?> GetEntityForDoctorAsync<T>(int doctorId, int entityId, Expression<Func<T, bool>> condition) where T : class
        {
            return await context.Set<T>()
                .Where(condition)
                .FirstOrDefaultAsync();
        }

        public async Task<Subject?> GetCourseForDoctorAsync(int doctorId, int courseId)
        {
            return await context.Subjects
                .Where(c => c.Id == courseId && c.DoctorId == doctorId) 
                .FirstOrDefaultAsync();
        }
        public async Task<Faculty?> GetDepartmentForDeanAsync(int doctorId, int facultyId)
        {
            return await GetEntityForDoctorAsync<Faculty>(
                doctorId,
                facultyId,
                c => c.Id == facultyId && c.DeanId == doctorId
            );
        }
        public async Task<Department?> GetDepartmentForHeadAsync(int doctorId, int departmentId)
        {
            return await GetEntityForDoctorAsync<Department>(
                doctorId,
                departmentId,
                c => c.Id == departmentId && c.HeadId == doctorId
            );
        }
        public Doctor GetDoctorOfSubject(int DoctorId)
        {
            return subjectRepository.GetOne(DoctorId).Doctor;
        }
        public bool IsDoctor(string userId)
        {
            return context.Doctors.Any(s => s.ApplicationUserId == userId);
        }
    }
}
