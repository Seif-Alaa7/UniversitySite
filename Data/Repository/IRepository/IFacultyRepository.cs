using Microsoft.AspNetCore.Mvc.Rendering;
using Models;

namespace Data.Repository.IRepository
{
    public interface IFacultyRepository
    {
        List<Faculty> GetAll();
        Faculty GetOne(int Id);
        void Delete(Faculty faculty);
        void Update(Faculty faculty);
        void Add(Faculty faculty);
        List<SelectListItem> Select();
        void Save();
        bool ExistDeanInFaculty(int DeanId);
        Faculty? FacultyByDepartment(int DepartmentId);
        Dictionary<int, string> GetNames(List<Student> Students);
        Dictionary<int, string> GetFaculty(List<Faculty> faculties);
        Faculty GetFacultybyDean(int id);
        public List<Department> GetDepartments(int FacultyId);
        public List<Subject> GetSubjects(int FacultyId);
    }
}
