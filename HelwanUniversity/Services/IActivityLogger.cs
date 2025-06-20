using Models.Enums;

namespace HelwanUniversity.Services
{
    public interface IActivityLogger
    {
        void Log(string actionType, string tableName, int? recordId, string description, int? userId, string userName, UserRole userRole);
    }
}
