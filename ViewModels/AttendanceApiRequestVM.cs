using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class AttendanceApiRequestVM
    {
        [Required]
        public int SubjectId { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        public int? DoctorId { get; set; }

        [Required]
        public string AiSessionId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one attendee is required.")]
        public List<AttendeeModel> Attendees { get; set; }
    }

    public class AttendeeModel
    {
        [Required]
        public int StudentId { get; set; }
    }
}
