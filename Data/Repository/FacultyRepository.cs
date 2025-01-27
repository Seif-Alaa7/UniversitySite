using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Data.Repository
{
    public class FacultyRepository : IFacultyRepository
    {
        private readonly ApplicationDbContext context;

        public FacultyRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public void Add(Faculty faculty)
        {
            context.Faculties.Add(faculty);
        }

        public void Update(Faculty faculty)
        {
            context.Faculties.Update(faculty);
        }

        public void Delete(Faculty faculty)
        {
            context.Faculties.Remove(faculty);
        }

        public Faculty GetOne(int id)
        {
            var faculty = context.Faculties
                .Include(f => f.Departments)
                .FirstOrDefault(f => f.Id == id);
            return (faculty);
        }

        public List<Faculty> GetAll()
        {
            var faculties = context.Faculties
                .Include(f => f.Departments)
                .ToList();
            return faculties;
        }
        public List<SelectListItem> Select()
        {
            var options =  context.Faculties.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList();
            return options;
        }
        public bool ExistDeanInFaculty(int DeanId)
        {
            var existDeanInDepartment = context.Faculties.Any(x => x.DeanId == DeanId);
            return existDeanInDepartment;
        }
        public void Save()
        {
            context.SaveChanges();
        }
        public Faculty? FacultyByDepartment(int DepartmentId)
        {
            var department = context.Departments
                .Include(d => d.Faculty)
                .FirstOrDefault(d => d.Id == DepartmentId);

            return department?.Faculty;
        }
        public Dictionary<int, string> GetNames(List<Student> students)
        {
            var facultyNames = new Dictionary<int, string>();

            foreach (var student in students)
            {
                var department = context.Departments
                    .Include(d => d.Faculty)
                    .FirstOrDefault(d => d.Id == student.DepartmentId);

                if (department != null && department.Faculty != null)
                {
                    facultyNames[student.DepartmentId] = department.Faculty.Name;
                }
                else
                {
                    facultyNames[student.DepartmentId] = "N/A";
                }
            }

            return facultyNames;
        }
        public Dictionary<int,string> GetFaculty(List<Faculty> faculties)
        {
            var FacultyDictionary = context.Faculties
               .ToList().ToDictionary(x => x.DeanId, x => x.Name);

            var FacultyNames = new Dictionary<int, string>();
            foreach (var Faculty in faculties)
            {
                string FacultyName;
                if (FacultyDictionary.TryGetValue(Faculty.DeanId, out FacultyName))
                {
                    FacultyNames[Faculty.DeanId] = FacultyName;
                }
            }
            return FacultyNames;
        }
        public Faculty GetFacultybyDean(int id)
        {
            var Faculty = context.Faculties.FirstOrDefault(x=>x.DeanId == id);
            return Faculty;
        }
        public List<Department> GetDepartments(int FacultyId)
        {
            return context.Departments.Where(d => d.FacultyId == FacultyId).ToList();
        }
        public List<Subject> GetSubjects(int FacultyId)
        {
            var departmentIds = context.Departments
                .Where(d => d.FacultyId == FacultyId)
                .Select(d => d.Id)
                .ToList();
            return context.Subjects
                .Where(s => s.Departments.Any(d => departmentIds.Contains(d.Id)))
                .ToList();
        }
    }
}
