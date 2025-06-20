using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Models;
using Models.Enums;

namespace HelwanUniversity.Services
{
    public class ActivityLogger
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivityLogger(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public void Log(string actionType, string tableName, int? recordId, string description, int? userId, string userName,UserRole userRole)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var agent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

            var log = new ActivityLog
            {
                ActionType = actionType,
                TableName = tableName,
                RecordId = recordId,
                Description = description,
                UserId = userId,
                UserName = userName,
                ActionDate = DateTime.Now,
                UserRole = userRole,
                IPAddress = ip,
                UserAgent = agent   
            };
            _context.ActivityLogs.Add(log);
            _context.SaveChanges();
        }
    }
}