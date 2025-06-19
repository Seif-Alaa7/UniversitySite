using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string? ActionType { get; set; } 
        public string? TableName { get; set; }
        public int? RecordId { get; set; }
        public string? Description { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime ActionDate { get; set; } = DateTime.Now;
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }

    }
}
