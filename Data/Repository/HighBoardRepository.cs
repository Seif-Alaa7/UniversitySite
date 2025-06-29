﻿using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;
using System.Linq.Expressions;

namespace Data.Repository
{
    public class HighBoardRepository : IHighBoardRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IFacultyRepository facultyRepository;
        private readonly IDepartmentRepository departmentRepository;

        public HighBoardRepository(ApplicationDbContext context,IFacultyRepository facultyRepository,
            IDepartmentRepository departmentRepository)
        {
            this.context = context;
            this.facultyRepository = facultyRepository; 
            this.departmentRepository = departmentRepository;   
        }
        public void Update(HighBoard highBoard)
        {
            context.HighBoards.Update(highBoard);
        }

        public void Delete(int id)
        {
            var highBoard = GetOne(id);
            context.HighBoards.Remove(highBoard);
        }
        public void DeleteUser(string id)
        {
            var user = context.Users.SingleOrDefault(u => u.Id == id);
            if (user != null)
            {
                context.Users.Remove(user);
            }
        }

        public HighBoard GetOne(int Id)
        {
            var highboard = context.HighBoards
                .Find(Id);
            return highboard;
        }

        public List<HighBoard> GetAll()
        {
            var highboards = context.HighBoards.ToList();
            return highboards;
        }
        public List<SelectListItem> selectDeans()
        {
            var options = context.HighBoards.Where(x => x.JobTitle == Models.Enums.JobTitle.DeanOfFaculty).Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList();
            return options;
        }
        public List<SelectListItem> selectHeads()
        {
            var options = context.HighBoards.Where(x => x.JobTitle == Models.Enums.JobTitle.HeadOfDepartment).Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList();
            return options;
        }
        public void Save()
        {
            context.SaveChanges();
        }
        public bool ExistName(string name)
        {
            var exist = context.HighBoards.Any(x => x.Name == name);
            return exist;
        }
        public bool ExistJop(JobTitle JobTitle)
        {
            var exist = context.HighBoards.Any(x=>x.JobTitle == JobTitle);
            return exist;
        }
        public string GetName(int id)
        {
            var name = context.HighBoards.FirstOrDefault(x=>x.Id == id)?.Name;
            return name;
        }
        public HighBoard GetPresident()
        {
            var president = context.HighBoards.FirstOrDefault(x=>x.JobTitle == JobTitle.President);
            return president;
        }
        public IQueryable<HighBoard> GetHeads()
        {
            var Heads = context.HighBoards.Where(x => x.JobTitle == JobTitle.HeadOfDepartment);
            return Heads;
        }
        public IQueryable<HighBoard> GetDeans()
        {
            var Deans = context.HighBoards.Where(x => x.JobTitle == JobTitle.DeanOfFaculty);
            return Deans;
        }

        public async Task<object?> GetEntityByUserIdAsync(string userId)
        {
            return await context.HighBoards
            .Include(hb => hb.Faculty)
            .Include(hb => hb.Department)
            .FirstOrDefaultAsync(h => h.ApplicationUserId == userId);
        }
        public HighBoard GetByUserId(string userId)
        {
            return context.HighBoards
                .Include(h => h.Faculty)
                .Include(h => h.Department)
                .FirstOrDefault(h => h.ApplicationUserId == userId);
        }
        public async Task<T?> GetEntityForHighboardAsync<T>(int doctorId, int entityId, Expression<Func<T, bool>> condition) where T : class
        {
            return await context.Set<T>()
                .Where(condition)
                .FirstOrDefaultAsync();
        }
        public async Task<Faculty?> GetDepartmentForDeanAsync(int doctorId, int facultyId)
        {
            return await GetEntityForHighboardAsync<Faculty>(
                doctorId,
                facultyId,
                c => c.Id == facultyId && c.DeanId == doctorId
            );
        }
        public async Task<Department?> GetDepartmentForHeadAsync(int doctorId, int departmentId)
        {
            return await GetEntityForHighboardAsync<Department>(
                doctorId,
                departmentId,
                c => c.Id == departmentId && c.HeadId == doctorId
            );
        }
        public HighBoard GetDeanByFaculty(int facultyid)
        {
            return facultyRepository.GetOne(facultyid).HighBoard;
        }
        public HighBoard? GetHeadByDepartment(int departmentid)
        {
            return departmentRepository.GetOne(departmentid).HighBoard;
        }
        public bool IsHighboard(string userId)
        {
            return context.HighBoards.Any(s => s.ApplicationUserId == userId);
        }
}
}
