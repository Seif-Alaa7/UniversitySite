using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class SubjectController : Controller
    {
        private readonly ISubjectRepository subjectRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IDepartmentRepository departmentRepository;

        public SubjectController(ISubjectRepository subjectRepository,IDoctorRepository doctorRepository,
            IDepartmentRepository departmentRepository)
        {
            this.subjectRepository = subjectRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ResultsRegisteration(int id)
        {
            var Subjects = subjectRepository.GetSubjects(id);
            ViewBag.DoctorNames = doctorRepository.GetName(Subjects);
            var department = departmentRepository.DepartmentByStudent(id);
            ViewData["StudentId"] = id;
            ViewData["departmentName"] = department.Name;

            return View(Subjects);
        }
    }
}
