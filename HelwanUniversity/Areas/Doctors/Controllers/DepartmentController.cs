using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Data.Repository;
using System.Security.Claims;
using Data.Repository.IRepository;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IFacultyRepository facultyRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IAcademicRecordsRepository academicRecordsRepository;
        private readonly ISubjectRepository subjectRepository;

        public DepartmentController(IDepartmentRepository departmentRepository, IDepartmentSubjectsRepository departmentSubjectsRepository,
            IHighBoardRepository highBoardRepository, IDoctorRepository doctorRepository, IFacultyRepository facultyRepository,
            IStudentRepository studentRepository, IAcademicRecordsRepository academicRecordsRepository,
            ISubjectRepository subjectRepository)
        {
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.highBoardRepository = highBoardRepository;
            this.doctorRepository = doctorRepository;
            this.facultyRepository = facultyRepository;
            this.studentRepository = studentRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.subjectRepository = subjectRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            var Department = departmentRepository.GetOne(id);

            ViewData["Head"] = highBoardRepository.GetName(Department.HeadId);
            ViewBag.Subjects = departmentSubjectsRepository.subjectsByDepartment(id);
            ViewData["Students"] = departmentRepository.GetStudentCount(id);

            ViewBag.StudentsBySubject = departmentSubjectsRepository.StudentCounts(ViewBag.Subjects);
            ViewBag.DoctorNames = doctorRepository.GetName(ViewBag.Subjects);

            return View(Department);
        }
        public IActionResult Students(int id)
        {
            var Faculty = facultyRepository.GetFacultybyDean(id);
            if(Faculty == null)
            {
                return NotFound();
            };
            var Departments = departmentRepository.GetDepartmentsByCollegeId(Faculty.Id);
            ViewData["FacultyName"] = Faculty.Name;
            return View(Departments);
        }
        public IActionResult DepartmentInfo(int id)
        {
            ViewData["DepartmentName"] = departmentRepository.GetOne(id)?.Name;
            ViewData["FacultyId"] = facultyRepository.FacultyByDepartment(id).Id;
            ViewBag.ID = id;
            return View();
        }
        [HttpGet]
        public IActionResult GetdegreesForDepartment(int DepartmentId)
        {
            var gradesData = academicRecordsRepository.GetChartData(DepartmentId);
            return Json(gradesData);
        }
        [HttpGet]
        public IActionResult GetSubjectPassRates(int departmentId)
        {
            var subjectPassRates = departmentRepository.GetSubjectPassRates(departmentId);
            return Json(subjectPassRates);
        }
        public IActionResult ChartData()
        {
            return View();
        }
    }
}
