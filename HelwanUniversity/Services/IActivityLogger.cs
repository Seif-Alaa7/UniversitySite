using Models;
using Models.Enums;

namespace HelwanUniversity.Services
{
    public interface IActivityLogger
    {
        void Log(string actionType, string tableName, int? recordId, string description, int? userId, string userName, UserRole userRole);

        List<ActivityLog> GetActivityLogs();

        Task<string> AnalyzeDescriptionAsync(string description);

        Task<Dictionary<string, int>> GetCategoryCountsAsync();
        Task<Dictionary<string, int>> GetCategoryCountsCachedAsync();
    }
}