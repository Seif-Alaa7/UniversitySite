using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.Enums;
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
        private readonly ISubjectRepository subjectRepository;
        private readonly IActivityLogger _logger;

        public StudentController(IStudentRepository studentRepository , IDepartmentRepository departmentRepository, IFacultyRepository faculty,
            ICloudinaryService cloudinaryService, IUniversityRepository universityRepository,
            IAcademicRecordsRepository academicRecordsRepository,
            IFacultyRepository facultyRepository, IHighBoardRepository highBoardRepository, IDoctorRepository doctorRepository,
            ISubjectRepository subjectRepository, IActivityLogger logger)
        {
            this.studentRepository = studentRepository;
            this.departmentRepository = departmentRepository;
            this.cloudinaryService = cloudinaryService;
            this.universityRepository = universityRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.facultyRepository = facultyRepository;
            this.highBoardRepository = highBoardRepository;
            this.doctorRepository = doctorRepository;
            this.subjectRepository = subjectRepository;
            this._logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Details(int id)
        {
            var studentDatails = studentRepository.GetOne(id);
            if (studentDatails == null)
            {
                return NotFound();
            }
            var department = departmentRepository.DepartmentByStudent(id);

            if (department == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);

            if (entity is Doctor doctor)
            {
                var studentSubjects = studentRepository.GetStudentSubjects(id);
                var doctorSubjects = subjectRepository.SubjectsByDoctor(doctor.Id);
                var isTeachingStudent = studentSubjects.Any(s => doctorSubjects.Any(d => d.Id == s.SubjectId));
                if (!isTeachingStudent)
                    return Forbid();
            }
            else if (entity is HighBoard head && head.JobTitle == JobTitle.HeadOfDepartment)
            {
                var doctorDepartment = departmentRepository.GetDepartbyHead(head.Id);
                if (doctorDepartment == null || doctorDepartment.Id != department.Id)
                    return Forbid();
            }
            else if (entity is HighBoard dean && dean.JobTitle == JobTitle.DeanOfFaculty)
            {
                var faculty = facultyRepository.GetFacultybyDean(dean.Id);
                var facultyOfStudent = facultyRepository.FacultyByDepartment(department.Id);

                if (faculty == null || facultyOfStudent == null || faculty.Id != facultyOfStudent.Id)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }
            var facultyData = facultyRepository.FacultyByDepartment(department.Id);
            ViewData["Faculty"] = facultyData;

            var googleForm = universityRepository.Get().GoogleForm;
            if (googleForm != null)
            {
                ViewData["FormBifurcation"] = googleForm;
            }
            else
            {
                return NotFound();
            }

            if(entity is HighBoard HB)
            {
                string positionDetails = HB.JobTitle switch
                {
                    JobTitle.HeadOfDepartment => $" of {departmentRepository.GetDepartbyHead(HB.Id)?.Name}",
                    JobTitle.DeanOfFaculty => $" of {facultyRepository.GetFacultybyDean(HB.Id)?.Name}",
                    _ => ""
                };

                _logger.Log(
                         actionType:"Show Student Details",
                         tableName: "Students",
                         recordId: studentDatails.Id,
                         description: $"{HB.JobTitle}{positionDetails} viewed the profile of student '{studentDatails.Name}'.",
                         userId: HB.Id,
                         userName: HB.Name,
                         userRole: UserRole.HighBoard
                );
            }
            else if(entity is Doctor DOC)
            {
                _logger.Log(
                         actionType: "Show Student Details",
                         tableName: "Students",
                         recordId: studentDatails.Id,
                         description: $"Doctor viewed student '{studentDatails.Name}' details.",
                         userId: DOC.Id,
                         userName: DOC.Name,
                         userRole: UserRole.Doctor
                );
            }
            return View(studentDatails);
        }
        public async Task<IActionResult> StudentsByDepartment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await highBoardRepository.GetEntityByUserIdAsync(userId);

            if (entity is not HighBoard highBoard)
            {
                return Forbid();
            }

            if (highBoard.JobTitle == JobTitle.HeadOfDepartment)
            {
                if (highBoard.Department.Id != id)
                {
                    return Forbid();
                }
            }
            else if (highBoard.JobTitle == JobTitle.DeanOfFaculty)
            {
                var department = departmentRepository.GetOne(id);
                if (department == null || department.FacultyId != highBoard.Faculty.Id)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            else
            {
                ViewBag.SuccessMessage = TempData["Success"];
            }
            ViewData["Doctor"] = highBoard;

            var students = studentRepository.GetStudents(id).ToList();
            if (students == null)
            {
                return NotFound();
            }
            ViewBag.Students = students;    
            ViewBag.Records = academicRecordsRepository.GetLevelANDSemester(students);
            ViewData["DepartmentName"] = departmentRepository.GetOne(id)?.Name;
            ViewData["FacultyName"] = facultyRepository.FacultyByDepartment(id).Name;
            ViewBag.ID = id;

            return View(students);
        }
        public async Task<IActionResult> FeesStatus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await highBoardRepository.GetEntityByUserIdAsync(userId);
            if (entity is not HighBoard highBoard || highBoard.JobTitle != JobTitle.VP_For_Finance)
            {
                return Forbid();
            }

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
        [HttpPost]
        public IActionResult LogExportExcel()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var highBoard = highBoardRepository.GetByUserId(userId);
            if (highBoard == null)
                return Forbid();

            _logger.Log(
                actionType:"Export Excel",
                tableName: "Students",
                recordId: 0,
                description: $"{highBoard.JobTitle} exported a report showing students’ tuition fee status",
                userId: highBoard.Id,
                userName: highBoard.Name,
                userRole: UserRole.HighBoard 
            );
            return Ok();
        }
    }
}
