using Microsoft.AspNetCore.Mvc.Rendering;
using Models;

namespace Data.Repository.IRepository
{
    public interface IDoctorRepository
    {
        List<Doctor> GetAll();
        Doctor GetOne(int Id);
        void Delete(int id);
        void Update(Doctor doctor);
        List<SelectListItem> Select();
        void Save();
        bool ExistName(string Name);
        Dictionary<int, string> GetName(List<Subject> subjects);
        Dictionary<int, string> GetName(List<DepartmentSubjects> subjects);
        Dictionary<int, List<string>> GetSubjects(List<Doctor> Doctors);
        public Dictionary<int, List<string>> GetDepartments(List<Doctor> doctors);
        Dictionary<int, List<string>> GetColleges(List<Doctor> doctors);
        void DeleteUser(string id);
    }
}
