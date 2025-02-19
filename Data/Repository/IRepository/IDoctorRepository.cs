using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using System.Linq.Expressions;

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
        Dictionary<int, List<string>> GetDepartments(List<Doctor> doctors);
        Dictionary<int, List<string>> GetColleges(List<Doctor> doctors);
        void DeleteUser(string id);
        Task<object?> GetEntityByUserIdAsync(string userId);
        Task<T?> GetEntityForDoctorAsync<T>(int doctorId, int entityId, Expression<Func<T, bool>> condition) where T : class;
        Task<Subject?> GetCourseForDoctorAsync(int doctorId, int courseId);
        Task<Faculty?> GetDepartmentForDeanAsync(int doctorId, int facultyId);
        Task<Department?> GetDepartmentForHeadAsync(int doctorId, int departmentId);
    }
}
