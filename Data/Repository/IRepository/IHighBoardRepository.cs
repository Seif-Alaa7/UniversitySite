using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.Enums;

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
        HighBoard GetDeanByFaculty(int facultyid);
        HighBoard? GetHeadByDepartment(int departmentid);
        bool IsHighboard(string userId);
    }
}
