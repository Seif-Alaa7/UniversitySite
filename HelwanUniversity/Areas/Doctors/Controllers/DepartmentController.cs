using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Data.Repository;
using System.Security.Claims;
using Data.Repository.IRepository;
using Models;
using ViewModels;

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
        private readonly IHttpContextAccessor _httpContextAccessor;


        public DepartmentController(IDepartmentRepository departmentRepository, IDepartmentSubjectsRepository departmentSubjectsRepository,
            IHighBoardRepository highBoardRepository, IDoctorRepository doctorRepository, IFacultyRepository facultyRepository,
            IStudentRepository studentRepository, IHttpContextAccessor httpContextAccessor, IAcademicRecordsRepository academicRecordsRepository)
        {
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.highBoardRepository = highBoardRepository;
            this.doctorRepository = doctorRepository;
            this.facultyRepository = facultyRepository;
            this.studentRepository = studentRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Details(int id)
        {
            var Department = departmentRepository.GetOne(id);
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);
            if (entity == null)
            {
                return Forbid();
            }
            if (entity is Doctor doctor)
            {
                int doctorId = doctor.Id;
                var departments = doctorRepository.GetDepartments(new List<Doctor> { doctor });
                if (!departments.ContainsKey(doctorId) || !departments[doctorId].Contains(Department.Name))
                {
                    return Forbid();
                }
            }
            else if (entity is HighBoard highboard)
            {
                int highboardId = highboard.Id;
                var headDepartment = await doctorRepository.GetDepartmentForHeadAsync(highboardId, id);
                var deanFaculty = await doctorRepository.GetDepartmentForDeanAsync(highboardId, id);

                if (headDepartment == null && deanFaculty == null)
                {
                    return Forbid();
                }
            }
            ViewData["Head"] = highBoardRepository.GetName(Department.HeadId);
            ViewBag.Subjects = departmentSubjectsRepository.subjectsByDepartment(id);
            ViewData["Students"] = departmentRepository.GetStudentCount(id);

            ViewBag.StudentsBySubject = departmentSubjectsRepository.StudentCounts(ViewBag.Subjects);
            ViewBag.DoctorNames = doctorRepository.GetName(ViewBag.Subjects);

            return View(Department);
        }
        public async Task<IActionResult> Students(int id)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);

            if (entity is not HighBoard highboard)
            {
                return Forbid();
            }
            var faculty = facultyRepository.GetFacultybyDean(id);
            if(faculty == null || faculty.DeanId != highboard.Id)
            {
                return Forbid();
            };
            var Departments = departmentRepository.GetDepartmentsByCollegeId(faculty.Id);
            ViewData["FacultyName"] = faculty.Name;
            return View(Departments);
        }
        public async Task<IActionResult> DepartmentInfo(int id)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);
            if (entity == null)
            {
                return Forbid();
            }
            if (entity is HighBoard highboard)
            {
                int highboardId = highboard.Id;
                var course = await doctorRepository.GetDepartmentForHeadAsync(highboardId, id);
                if (course == null)
                {
                    return Forbid();
                }
            }
            ViewData["DepartmentName"] = departmentRepository.GetOne(id)?.Name;
            ViewData["FacultyId"] = facultyRepository.FacultyByDepartment(id).Id;
            ViewBag.ID = id;
            return View();
        }
    }
}
