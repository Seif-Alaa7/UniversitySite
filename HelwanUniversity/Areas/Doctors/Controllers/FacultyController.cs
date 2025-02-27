using Data;
using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Security.Claims;
using ViewModels.FacultyVMs;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class FacultyController : Controller
    {
        private readonly IFacultyRepository facultyRepository;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IDepartmentRepository departmentRepository;

        public FacultyController(IFacultyRepository facultyRepository,
            IHighBoardRepository highBoardRepository , IDoctorRepository doctorRepository, IDepartmentRepository departmentRepository)
        {
            this.facultyRepository = facultyRepository;
            this.highBoardRepository = highBoardRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
        }
        public IActionResult Index()
        {
            var faculties = facultyRepository.GetAll().ToList();
            return View(faculties);
        }
        public IActionResult Details(int id)
        {
            var faculty = facultyRepository.GetOne(id);
            if (faculty == null)
            {
                return NotFound();
            }
            faculty.ViewCount++;
            facultyRepository.Save();

            ViewData["Dean"] = highBoardRepository.GetOne(faculty.DeanId)?.Name;

            return View(faculty);
        }
        public IActionResult FacultyInfo(int id)
        {
            var Faculty = facultyRepository.GetFacultybyDean(id);
            if (Faculty == null)
            {
                return NotFound();
            };
            ViewData["FacultyName"] = Faculty.Name;
            ViewData["FacultyId"] = Faculty.Id;
            ViewBag.ID = id;
            return View();
        }
        public async Task<IActionResult> DepartmentsOfFaculty(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);

            if (entity is not HighBoard highboard)
            {
                return Forbid();
            }
            var faculty = facultyRepository.GetFacultybyDean(id);
            if (faculty == null || faculty.DeanId != highboard.Id)
            {
                return Forbid();
            };
            var Departments = departmentRepository.GetDepartmentsByCollegeId(faculty.Id);
            ViewData["FacultyName"] = faculty.Name;
            return View(Departments);
        }
    }
}
