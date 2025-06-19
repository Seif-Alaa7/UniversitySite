using Data;
using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Enums;
using NuGet.Protocol.Plugins;
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
        private readonly IAcademicRecordsRepository academicRecordsRepository;

        public FacultyController(IFacultyRepository facultyRepository,
            IHighBoardRepository highBoardRepository , IDoctorRepository doctorRepository, IDepartmentRepository departmentRepository
            , IAcademicRecordsRepository academicRecordsRepository)
        {
            this.facultyRepository = facultyRepository;
            this.highBoardRepository = highBoardRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
            this.academicRecordsRepository = academicRecordsRepository;
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
        public async Task<IActionResult> FacultyInfo(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);

            if (entity is not HighBoard highboard)
            {
                return Forbid();
            }

            var faculty = facultyRepository.GetFacultybyDean(id);
            if (faculty == null)
            {
                return NotFound();
            };

            if (faculty.DeanId != highboard.Id)
            {
                return Forbid();
            }
            ViewData["FacultyName"] = faculty.Name;
            ViewData["FacultyId"] = faculty.Id;
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
        public IActionResult getAvgGpa(int facultyId)
        {
            var levels = Enum.GetValues(typeof(Level)).Cast<Level>();
            var genders = Enum.GetValues(typeof(Gender)).Cast<Gender>();
            var departments = departmentRepository.GetDepsWithStudents(facultyId);
            var result = new List<object>();

            foreach (var department in departments)
            {
                var groups = new List<object>();
                foreach (var level in levels)
                {
                    foreach (var gender in genders)
                    {
                        var avgGpa = academicRecordsRepository.FindAvgGPAByDepartmentAndFilters(department.Id, level, gender);
                        groups.Add(new
                        {
                            level = level.ToString(),
                            gender = gender.ToString(),
                            avgGpa
                        });
                    }
                }
                result.Add(new
                {
                    departmentName = department.Name,
                    groups
                });
            }
            return Ok(result);
        }
        public async Task<IActionResult> ChartDataFaculty(int facultyId)
        {
            ViewBag.facultyId = facultyId;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);
            if (entity == null)
            {
                return NotFound();
            }
            if (entity is Doctor doctor)
            {
                /*{
                    return Forbid();
                }*/
            }
            if (entity is HighBoard highboard)
            {
                /*int highboardId = highboard.Id;
                var course = await doctorRepository.GetDepartmentForHeadAsync(highboardId, id);
                if (course == null)
                {
                    return NotFound();
                }*/
            }
            return View();
        }
    }
}
