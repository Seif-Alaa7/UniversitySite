using Data.Repository;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelwanUniversity.Areas.Students.Controllers
{
    [Area("Students")]
    [Authorize(Roles = "Student")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IUniFileRepository uniFileRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IStudentRepository studentRepository;
        public DepartmentController(IDepartmentRepository departmentRepository, 
            IDepartmentSubjectsRepository departmentSubjectsRepository,
            IHighBoardRepository highBoardRepository,
            IDoctorRepository doctorRepository,
            IUniFileRepository uniFileRepository, IStudentRepository studentRepository)
        {
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.highBoardRepository = highBoardRepository;
            this.doctorRepository = doctorRepository;
            this.uniFileRepository = uniFileRepository;
            this.studentRepository = studentRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isAllowed = studentRepository.IsStudentInDepartment(userId, id);
            if (!isAllowed)
                return Forbid();

            var Department = departmentRepository.GetOne(id);
            if (Department == null)
                return NotFound();
            var Images = uniFileRepository.GetAllImages();

            ViewData["Head"] = highBoardRepository.GetOne(Department.HeadId);

            ViewBag.Subjects = departmentSubjectsRepository.subjectsByDepartment(id);
            ViewData["Students"] = departmentRepository.GetStudentCount(id);

            ViewBag.StudentsBySubject = departmentSubjectsRepository.StudentCounts(ViewBag.Subjects);
            ViewBag.DoctorNames = doctorRepository.GetName(ViewBag.Subjects);
            ViewData["LogoTitle"] = Images[0].File;

            return View(Department);
        }
    }
}
