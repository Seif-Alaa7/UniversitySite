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

            var Student = studentRepository.GetAll().FirstOrDefault(h => h.ApplicationUserId == userId);
            if (Student != null)
            {
                Student.PaymentFees = true;
                Student.PaymentFeesDate = DateTime.Now;

                studentRepository.Update(Student);
                studentRepository.Save();
            };
            return View();
       }
       public IActionResult cancel()
       {
            return View();
       }
    }
}
