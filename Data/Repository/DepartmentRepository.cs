using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Data.Repository
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext context;

        public DepartmentRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public void Add(Department department)
        {
            context.Departments.Add(department);
        }

        public void Update(Department department)
        {
            context.Departments.Update(department);
        }

        public void Delete(Department department)
        {
            context.Departments.Remove(department);
        }

        public Department GetOne(int Id)
        {
            var department = context.Departments
                .Find(Id);
            return department;
        }

        public List<Department> GetAll()
        {
            var departments = context.Departments
                .ToList();
            return departments;
        }
        public List<SelectListItem> Select()
        {
            var options =  context.Departments.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name,

            }).ToList();
            return options;
        }
        public void Save()
        {
            context.SaveChanges();
        }
        public int GetStudentCount(int? studentDepartmentId)
        {
            if (studentDepartmentId == null)
            {
                throw new ArgumentNullException(nameof(studentDepartmentId), "Department ID cannot be null.");
            }

            var department = context.Departments
                                    .Include(d => d.Students)
                                    .SingleOrDefault(d => d.Id == studentDepartmentId);

            if (department == null)
            {
                throw new InvalidOperationException($"Department with ID {studentDepartmentId} not found.");
            }

            return department.Students.Count;
        }
        public bool Exist(string Name)
        {
            var existDepartment = context.Departments.Any(d => d.Name == Name);
            return existDepartment;
        }
        public bool ExistHeadInDepartment(int headId)
        {
            var existHeadInDepartment = context.Departments.Any(x => x.HeadId == headId);
            return existHeadInDepartment;
        }
        public IEnumerable<Department> GetDepartmentsByCollegeId(int collegeId)
        {
            return context.Departments.Where(d => d.FacultyId == collegeId).ToList();
        }
        public Department? DepartmentByStudent(int StudentId)
        {
            var student = context.Students
                .Include(s => s.Department)
                .FirstOrDefault(s => s.Id == StudentId);

            return student?.Department;
        }
        public Dictionary<int,string> Dict()
        {
           var Dict =  context.Departments
                .ToList().ToDictionary(d => d.Id, d => d.Name);
            return Dict;
        }
        public Dictionary<int, List<(int Id, string Name)>> GetDepartmentsByFaculty(List<Faculty> facultyList)
        {
            var departmentsByFaculty = new Dictionary<int, List<(int Id, string Name)>>();

            foreach (var faculty in facultyList)
            {
                var departments = context.Departments
                    .Where(d => d.FacultyId == faculty.Id)
                    .Select(d => new { d.Id, d.Name })
                    .ToList();

                if (departments.Any())
                {
                    departmentsByFaculty[faculty.Id] = departments
                        .Select(d => (d.Id, d.Name))
                        .ToList();
                }
                else
                {
                    departmentsByFaculty[faculty.Id] = new List<(int Id, string Name)>
            {
                (0, "N/A") 
            };
                }
            }

            return departmentsByFaculty;
        }
        public Dictionary<int, string> GetDepartments(List<Department> departments)
        {
            var DepartmentDictionary = context.Departments
               .ToList().ToDictionary(x => x.HeadId, x => x.Name);

            var DepartmentNames = new Dictionary<int, string>();
            foreach (var Depaerment in departments)
            {
                string DepartmentName;
                if (DepartmentDictionary.TryGetValue(Depaerment.HeadId, out DepartmentName))
                {
                    DepartmentNames[Depaerment.HeadId] = DepartmentName;
                }
            }
            return DepartmentNames;
        }
        public Department GetDepartbyHead(int id)
        {
            var department = context.Departments.FirstOrDefault(x => x.HeadId == id);
            return department;
        }
    }
}
