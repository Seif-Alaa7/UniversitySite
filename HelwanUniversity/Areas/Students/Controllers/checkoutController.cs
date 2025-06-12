using Data.Repository;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelwanUniversity.Areas.Students.Controllers
{
    [Area("Students")]
    [Authorize(Roles = "Student")]
    public class checkoutController : Controller
    {
       private readonly IStudentRepository studentRepository;
        public checkoutController(IStudentRepository studentRepository)
        {
            this.studentRepository = studentRepository;
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

            return View();
        }
       public IActionResult cancel()
       {
            if (!User.Identity.IsAuthenticated)
                return Forbid();
            return View();
       }
    }
}
