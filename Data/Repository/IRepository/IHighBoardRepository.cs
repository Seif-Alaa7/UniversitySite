using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.Enums;
using System.Linq.Expressions;

namespace Data.Repository.IRepository
{
    public interface IHighBoardRepository
    {
        List<HighBoard> GetAll();
        HighBoard GetOne(int Id);
        void Delete(int id);
        void Update(HighBoard highBoard);
        List<SelectListItem> selectDeans();
        List<SelectListItem> selectHeads();
        void Save();
        bool ExistJop(JobTitle JobTitle);
        bool ExistName(string name);
        string GetName(int id);
        HighBoard GetPresident();
        IQueryable<HighBoard> GetHeads();
        IQueryable<HighBoard> GetDeans();
        void DeleteUser(string id);
        Task<object?> GetEntityByUserIdAsync(string userId);
        HighBoard GetByUserId(string userId);
        Task<T?> GetEntityForHighboardAsync<T>(int doctorId, int entityId, Expression<Func<T, bool>> condition) where T : class;
        Task<Faculty?> GetDepartmentForDeanAsync(int doctorId, int facultyId);
        Task<Department?> GetDepartmentForHeadAsync(int doctorId, int departmentId);
        HighBoard GetDeanByFaculty(int facultyid);
        HighBoard? GetHeadByDepartment(int departmentid);
        bool IsHighboard(string userId);
    }
}
