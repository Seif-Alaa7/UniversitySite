using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Models;
using Models.Enums;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using System.Buffers.Text;
using System.Diagnostics;
using System.Drawing;

namespace HelwanUniversity.Services
{
    public class ActivityLogger : IActivityLogger
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly string _cohereApiKey = "H19269cuq3GmYmO4BFNNSHloB1bc1k3lrzwNiw1P";
        private readonly IMemoryCache _cache;


        public ActivityLogger(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IMemoryCache cache)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _cache = cache;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cohereApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Log(string actionType, string tableName, int? recordId, string description, int? userId, string userName, UserRole userRole)
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


            _cache.Remove("CategoryStats");
        }

        public List<ActivityLog> GetActivityLogs()
        {
            return _context.ActivityLogs.ToList();
        }

        public async Task<string> AnalyzeDescriptionAsync(string description)
        {
            try
            {
                var prompt = $@"Based on the following activity description, classify it into one of these categories only:

                  - 'Security Threat' → For activities that indicate suspicious behavior, unauthorized access attempts, data breaches, or malicious intent.
                  - 'System Issue' → For failures caused by technical problems in the system such as server errors, database issues, system crashes, or timeouts.
                  - 'Sensitive Activity' → For activities involving modifications to critical system configurations, user permissions, or sensitive data, even if successful.
                  - 'Normal Activity' → For routine successful actions, or user mistakes such as invalid input, duplicate values, or failed attempts due to incorrect user data that are not caused by system failure.

                  Only return the category name without any extra text.

                  Description: {description}
                  Category:";

                var payload = new
                {
                    model = "command",
                    prompt = prompt,
                    max_tokens = 20
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("v1/generate", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return "Unknown";
                }

                using JsonDocument doc = JsonDocument.Parse(responseContent);
                var generatedText = doc.RootElement.GetProperty("generations")[0].GetProperty("text").GetString()?.Trim();

                return NormalizeCategory(generatedText);
            }
            catch
            {
                return "Unknown";
            }
        }

        private string NormalizeCategory(string? category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return "Unknown";

            category = category.ToLower();

            if (category.Contains("threat"))
                return "Security Threat";
            if (category.Contains("issue"))
                return "System Issue";
            if (category.Contains("sensitive"))
                return "Sensitive Activity";
            if (category.Contains("normal"))
                return "Normal Activity";

            return "Unknown";
        }

        public async Task<Dictionary<string, int>> GetCategoryCountsAsync()
        {
            var counts = new Dictionary<string, int>
            {
                { "Security Threat", 0 },
                { "System Issue", 0 },
                { "Sensitive Activity", 0 },
                { "Normal Activity", 0 }
            };

            var logs = _context.ActivityLogs.ToList();

            foreach (var log in logs)
            {
                var category = await AnalyzeDescriptionAsync(log.Description);

                if (counts.ContainsKey(category))
                {
                    counts[category]++;
                }
                else
                {
                    counts["Normal Activity"]++;
                }
            }

            return counts;
        }
        public async Task<Dictionary<string, int>> GetCategoryCountsCachedAsync()
        {
            if (_cache.TryGetValue("CategoryStats", out Dictionary<string, int> counts))
            {
                return counts;
            }

            counts = await GetCategoryCountsAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            _cache.Set("CategoryStats", counts, cacheOptions);

            return counts;
        }
    }
}