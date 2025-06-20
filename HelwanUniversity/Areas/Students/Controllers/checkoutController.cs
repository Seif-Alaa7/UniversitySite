using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Enums;
using System.Security.Claims;

namespace HelwanUniversity.Areas.Students.Controllers
{
    [Area("Students")]
    [Authorize(Roles = "Student")]
    public class checkoutController : Controller
    {
       private readonly IStudentRepository studentRepository;
       private readonly IActivityLogger _logger;

        public checkoutController(IStudentRepository studentRepository,
            IActivityLogger logger)
        {
            this.studentRepository = studentRepository;
            this._logger = logger;
        }

        public IActionResult success()
       {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var student = studentRepository.GetByUserId(userId);
            if (student == null)
                return Forbid();

            student.PaymentFees = true;
            student.PaymentFeesDate = DateTime.Now;

            studentRepository.Update(student);
            studentRepository.Save();

            _logger.Log(
               actionType: "Payment Success",
               tableName: "Students",
               recordId: student.Id,
               description: $"Student '{student.Name}' completed payment successfully. PaymentFees set to true.",
               userId: student.Id,
               userName: student.Name,
               userRole: UserRole.Student
             );

            return View();
        }
       public IActionResult cancel()
       {
            if (!User.Identity.IsAuthenticated)
                return Forbid();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = studentRepository.GetByUserId(userId);

            _logger.Log(
                 actionType: "Cancel Payment",
                 tableName: "Students",
                 recordId: student.Id,
                 description: $"Student '{student.Name}' canceled the payment process before completion. PaymentFees remains false.",
                 userId: student.Id,
                 userName: student.Name,
                 userRole: UserRole.Student
            );

            return View();
       }
    }
}
