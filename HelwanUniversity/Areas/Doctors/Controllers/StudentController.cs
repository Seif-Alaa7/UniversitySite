using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using ViewModels;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class StudentController : Controller
    {
        private readonly IStudentRepository studentRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IUniversityRepository universityRepository;
        private readonly IAcademicRecordsRepository academicRecordsRepository;  
        private readonly IFacultyRepository facultyRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IHighBoardRepository highBoardRepository;
        public StudentController(IStudentRepository studentRepository , IDepartmentRepository departmentRepository,IFacultyRepository faculty,
            ICloudinaryService cloudinaryService,IUniversityRepository universityRepository,
            IAcademicRecordsRepository academicRecordsRepository,
            IFacultyRepository facultyRepository,IHighBoardRepository highBoardRepository,IDoctorRepository doctorRepository)
        {
            this.studentRepository = studentRepository;
            this.departmentRepository = departmentRepository;
            this.cloudinaryService = cloudinaryService;
            this.universityRepository = universityRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.facultyRepository = facultyRepository;
            this.highBoardRepository = highBoardRepository;
            this.doctorRepository = doctorRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            var studentDatails = studentRepository.GetOne(id);
            var department = departmentRepository.DepartmentByStudent(id);

            if (department != null)
            {
                var facultyData = facultyRepository.FacultyByDepartment(department.Id);
                ViewData["Faculty"] = facultyData;
            }
            else
            {
                ViewData["Faculty"] = null;
            }
            if(universityRepository.Get().GoogleForm != null)
            {
                ViewData["FormBifurcation"] = universityRepository.Get().GoogleForm;
            }
            else
            {
               return NotFound();   
            }
            return View(studentDatails);
        }
        public IActionResult StudentsByDepartment(int id)
        {
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            else
            {
                ViewBag.SuccessMessage = TempData["Success"];
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var Highboard = highBoardRepository.GetAll().FirstOrDefault(h => h.ApplicationUserId == userId);
            var Doctor = doctorRepository.GetAll().FirstOrDefault(h => h.ApplicationUserId == userId);
            if (Doctor != null)
            {
                ViewData["Doctor"] = Doctor;
            }
            else
            {
                ViewData["Doctor"] = Highboard;
            }
            var students = studentRepository.GetStudents(id).ToList();
            if(students == null)
            {
                return NotFound();
            }
            ViewBag.Records = academicRecordsRepository.GetLevelANDSemester(students);
            ViewData["DepartmentName"] = departmentRepository.GetOne(id)?.Name;
            ViewData["FacultyName"] = facultyRepository.FacultyByDepartment(id).Name;
            ViewBag.ID = id;

            return View(students);
        }
        public IActionResult FeesStatus()
        {
            var students = studentRepository.GetAll();

            ViewData["DepartmentNames"] = departmentRepository.Dict();

            ViewBag.FacultyNames = facultyRepository.GetNames(students);
            ViewData["Records"] = academicRecordsRepository.GetLevelANDSemester(students);

            ViewBag.TotalCount = students.Count();
            return View(students);
        }

        public IActionResult FeesPaid()
        {
            var students = studentRepository.TrueFees();

            ViewData["DepartmentNames"] = departmentRepository.Dict();

            ViewBag.FacultyNames = facultyRepository.GetNames(students);
            ViewData["Records"] = academicRecordsRepository.GetLevelANDSemester(students);

            ViewBag.TotalCount = students.Count();
            return View("FeesStatus", students);
        }

        public IActionResult FeesUnpaid()
        {
            var students = studentRepository.FalseFees();

            ViewData["DepartmentNames"] = departmentRepository.Dict();

            ViewBag.FacultyNames = facultyRepository.GetNames(students);
            ViewData["Records"] = academicRecordsRepository.GetLevelANDSemester(students);

            ViewBag.TotalCount = students.Count();
            return View("FeesStatus", students);
        }
    }
}
