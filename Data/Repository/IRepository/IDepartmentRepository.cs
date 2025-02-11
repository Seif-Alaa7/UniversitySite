using Microsoft.AspNetCore.Mvc.Rendering;
using Models;

namespace Data.Repository.IRepository
{
    public interface IDepartmentRepository
    {
        List<Department> GetAll();
        Department GetOne(int Id);
        void Delete(Department department);
        void Update(Department department);
        void Add(Department department);
        List<SelectListItem> Select();
        void Save();
        bool Exist(string Name);
        bool ExistHeadInDepartment(int headId);
        int GetStudentCount(int? studentDepartmentId);
        IEnumerable<Department> GetDepartmentsByCollegeId(int collegeId);
        Department? DepartmentByStudent(int StudentId);
        Dictionary<int, string> Dict();
        Dictionary<int, List<(int Id, string Name)>> GetDepartmentsByFaculty(List<Faculty> facultyList);
        Dictionary<int, string> GetDepartments(List<Department> departments);
        Department GetDepartbyHead(int id);
        public List<SelectListItem> SelectDepartsByFaculty(int id);
        public List<Doctor> GetDoctors(int departmentId);
        public List<dynamic> GetSubjectPassRates(int departmentId);
    }
}
