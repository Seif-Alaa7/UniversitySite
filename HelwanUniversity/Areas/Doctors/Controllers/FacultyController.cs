﻿using Data;
using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Enums;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
using ViewModels;
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
        private readonly ISubjectRepository subjectRepository;

        public FacultyController(IFacultyRepository facultyRepository,
            IHighBoardRepository highBoardRepository, IDoctorRepository doctorRepository, IDepartmentRepository departmentRepository
            , IAcademicRecordsRepository academicRecordsRepository
            , ISubjectRepository subjectRepository)
        {
            this.facultyRepository = facultyRepository;
            this.highBoardRepository = highBoardRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.subjectRepository = subjectRepository;
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
        [HttpGet]
        public IActionResult GetDepartments(int facultyId)
        {
            var departments = departmentRepository.GetDepartmentsByCollegeId(facultyId)
                .Select(d => new { d.Id, d.Name });
            return Ok(departments);
        }
        [HttpGet]
        public IActionResult GetAvgGpaByDepartment(int facultyId)
        {
            var departments = departmentRepository.GetDepartmentsByCollegeId(facultyId);

            var result = departments.Select(dep => new
            {
                departmentId = dep.Id,
                departmentName = dep.Name,
                avgGpa = academicRecordsRepository.FindAvgGPAByDepartment(dep.Id)
            });

            return Ok(result);
        }
        [HttpGet]
        public IActionResult GetAvgGpaByLevel(int departmentId)
        {
            var levels = Enum.GetValues(typeof(Level)).Cast<Level>();
            var result = levels.Select(level => new
            {
                level = level.ToString(),
                avgGpa = academicRecordsRepository.FindAvgGPAByDepartmentAndLevel(departmentId, level)
            });

            return Ok(result);
        }
        [HttpGet]
        public IActionResult GetAvgGpaByGender(int departmentId, Level level)
        {
            var genders = Enum.GetValues(typeof(Gender)).Cast<Gender>();
            var result = genders.Select(gender => new
            {
                gender = gender.ToString(),
                avgGpa = academicRecordsRepository.FindAvgGPAByDepartmentLevelGender(departmentId, level, gender)
            });

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
            if (entity is not HighBoard highboard)
            {
                {
                    return Forbid();
                }
            }
            var department = await highBoardRepository.GetDepartmentForDeanAsync(highboard.Id, facultyId);
            if (department == null)
            {
                return Forbid();
            }
            ViewBag.departments = departmentRepository.GetDepartmentsByCollegeId(facultyId);
            return View();
        }
        [HttpGet]
        public IActionResult GetSubjectFullStatsByFaculty(int facultyId)
        {
            var departments = departmentRepository.GetDepartmentsByCollegeId(facultyId);
            var allStats = new List<object>();

            foreach (var dept in departments)
            {
                var gpaList = academicRecordsRepository.GetLowestAvgGpaSubjectsByDepartment(dept.Id, int.MaxValue);
                var rateList = academicRecordsRepository.GetSubjectRateByDepartment(dept.Id, int.MaxValue, true);

                var deptStats = gpaList.Select(g =>
                {
                    double? rateValue = rateList
                      .Where(r => r.SubjectId == g.SubjectId)
                      .Select(r => (double?)r.Rate)
                      .FirstOrDefault();

                    double passRate = (rateValue ?? 0) / 100.0;
                    double failRate = 1 - passRate;

                    return new
                    {
                        subjectId = g.SubjectId,
                        subjectName = g.SubjectName,
                        avgGpa = Math.Round(g.AvgGpa, 2),
                        passRate = Math.Round(passRate, 2),
                        failRate = Math.Round(failRate, 2),
                        departmentName = dept.Name
                    };
                });

                allStats.AddRange(deptStats);
            }

            return Ok(allStats);
        }
    }
}
