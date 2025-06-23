using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Enums;
using System.Security.Claims;
using ViewModels;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class DepartmentSubjectsController : Controller
    {
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly IAcademicRecordsRepository academicRecordsRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IFacultyRepository facultyRepository;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IActivityLogger _logger;

        public DepartmentSubjectsController(IDepartmentRepository departmentRepository, IDepartmentSubjectsRepository departmentSubjectsRepository
            , IAcademicRecordsRepository academicRecordsRepository, ISubjectRepository subjectRepository, IDoctorRepository doctorRepository
            , IFacultyRepository facultyRepository,IActivityLogger logger, IHighBoardRepository highBoardRepository)
        {
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.subjectRepository = subjectRepository;
            this.doctorRepository = doctorRepository;
            this.facultyRepository = facultyRepository;
            _logger = logger;
            this.highBoardRepository = highBoardRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Add(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);

            if (entity == null)
            {
                return NotFound();
            }
            var department = departmentRepository.GetOne(id);
            var facultyId = departmentRepository.GetFacultyIdByDepartmentId(id);
            if (entity is HighBoard highboard)
            {
                var headDepartment = await doctorRepository.GetDepartmentForHeadAsync(highboard.Id, id);
                var deanFaculty = await doctorRepository.GetDepartmentForDeanAsync(highboard.Id, facultyId);

                if (headDepartment == null && deanFaculty == null)
                {
                    return NotFound();
                }
            }
            else
            {
                return Forbid();
            }
            ViewData["DepartId"] = id;
            ViewData["Subjects"] = subjectRepository.Select();
            return View();
        }
        [HttpPost]
        public IActionResult SaveAdd(DepartmentSubjects model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var highBoard = highBoardRepository.GetByUserId(userId);
            if (highBoard == null)
                return Forbid();

            string positionDetails = highBoard.JobTitle switch
            {
                JobTitle.HeadOfDepartment => $" of {departmentRepository.GetDepartbyHead(highBoard.Id)?.Name}",
                JobTitle.DeanOfFaculty => $" of {facultyRepository.GetFacultybyDean(highBoard.Id)?.Name}",
                _ => ""
            };
            var subjectName = subjectRepository.GetName(model.SubjectId);
            var departmentName = departmentRepository.GetOne(model.DepartmentId)?.Name ?? "Unknown";

            var ExistDepartmentSubject = departmentSubjectsRepository.Exist(model);

            if (ExistDepartmentSubject)
            {
                ViewData["DepartId"] = model.DepartmentId;
                ModelState.AddModelError("SubjectId", "This Subject is Already Exist in this Department..");
                ViewData["Subjects"] = subjectRepository.Select();

                _logger.Log(
                             actionType:"Add Subject From Department",
                             tableName: "DepartmentSubject",
                             recordId: model.DepartmentId,
                             description: $"{highBoard.JobTitle}{positionDetails} attempted to add subject '{subjectName}' to department '{departmentName}', but it already exists",
                             userId: highBoard.Id,
                             userName: highBoard.Name,
                             userRole: UserRole.HighBoard
                );

                return View("Add");
            }
            else
            {
                var DepartmentSubject = new DepartmentSubjects()
                {
                    DepartmentId = model.DepartmentId,
                    SubjectId = model.SubjectId,
                };
                departmentSubjectsRepository.Add(DepartmentSubject);
                departmentSubjectsRepository.Save();

            }

            _logger.Log(
                           actionType: "Add Subject From Department",
                           tableName: "DepartmentSubject",
                           recordId: model.DepartmentId,
                           description: $"{highBoard.JobTitle}{positionDetails} successfully added subject '{subjectName}' to department '{departmentName}'",
                           userId: highBoard.Id,
                           userName: highBoard.Name,
                           userRole: UserRole.HighBoard
            );



            return RedirectToAction("Details", "Department", new { area = "Doctors", id = model.DepartmentId });
        }
        public async Task<IActionResult> DisplaySubjects(int Studentid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);

            if (entity is not HighBoard highboard)
            {
                return Forbid();
            }

            var department = departmentRepository.DepartmentByStudent(Studentid);
            if (department == null)
            {
                return NotFound();
            }

            var facultyId = departmentRepository.GetFacultyIdByDepartmentId(department.Id);

            var headDepartment = await doctorRepository.GetDepartmentForHeadAsync(highboard.Id, department.Id);
            var deanFaculty = await doctorRepository.GetDepartmentForDeanAsync(highboard.Id, facultyId);

            if (headDepartment == null && deanFaculty == null)
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

            var academicRecord = academicRecordsRepository.GetAll().FirstOrDefault(x => x.StudentId == Studentid);
            if (academicRecord == null || academicRecord.Level == null || academicRecord.Semester == null)
            {
                return NotFound();
            }
            else
            {
                var level = academicRecord.Level;
                var semester = academicRecord.Semester;
                var StudentSubjects = departmentSubjectsRepository.StudentSubjects(level, semester, department.Id);
                ViewData["StudentId"] = Studentid;
                ViewBag.ID = department.Id;
                var Subjects = subjectRepository.GetSubjects(Studentid);
                var Department = departmentRepository.DepartmentByStudent(Studentid);
                ViewData["departmentName"] = department.Name;
                ViewBag.DoctorNames = doctorRepository.GetName(Subjects);
                ViewBag.Subjects = Subjects;
                return View(StudentSubjects);
            }
        }
        public IActionResult Delete(int subjectId, int departmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var highBoard = highBoardRepository.GetByUserId(userId);
            if (highBoard == null)
                return Forbid();

            var subjectName = subjectRepository.GetName(subjectId); 
            var departmentName = departmentRepository.GetOne(departmentId)?.Name ?? "Unknown";
            string positionDetails = highBoard.JobTitle switch
            {
                JobTitle.HeadOfDepartment => $" of {departmentRepository.GetDepartbyHead(highBoard.Id)?.Name}",
                JobTitle.DeanOfFaculty => $" of {facultyRepository.GetFacultybyDean(highBoard.Id)?.Name}",
                _ => ""
            };

            var link = departmentSubjectsRepository.DeleteRelation(subjectId, departmentId);
            if (link == null)
            {
                TempData["ErrorMessage"] = "The relationship between the subject and department could not be found.";

                _logger.Log(
                                actionType: "Delete Subject From Department",
                                tableName: "DepartmentSubject",
                                recordId: departmentId,
                                description: $"{highBoard.JobTitle}{positionDetails} attempted to delete subject '{subjectName}' from department '{departmentName}', but no such relation was found",
                                userId: highBoard.Id,
                                userName: highBoard.Name,
                                userRole: UserRole.HighBoard
                       );

                return RedirectToAction("Details", "Department", new { area = "Doctors", id = departmentId });
            }
            departmentSubjectsRepository.Delete(link);
            departmentSubjectsRepository.Save();

            _logger.Log(
                     actionType: "Delete Subject From Department",
                     tableName: "DepartmentSubject",
                     recordId: departmentId,
                     description: $"{highBoard.JobTitle}{positionDetails} successfully deleted subject '{subjectName}' from department '{departmentName}'",
                     userId: highBoard.Id,
                     userName: highBoard.Name,
                     userRole: UserRole.HighBoard
            );

            return RedirectToAction("Details", "Department", new { area = "Doctors", id = departmentId });
        }
    }
}
