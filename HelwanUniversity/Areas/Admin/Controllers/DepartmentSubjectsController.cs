using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Enums;
using System.Security.Claims;
using ViewModels;

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DepartmentSubjectsController : Controller
    {
        private readonly IDepartmentRepository departmentRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly IAcademicRecordsRepository academicRecordsRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IActivityLogger logger;    
        private readonly IHighBoardRepository highBoardRepository;

        public DepartmentSubjectsController(IDepartmentRepository departmentRepository,ISubjectRepository subjectRepository,
            IDepartmentSubjectsRepository departmentSubjectsRepository
            ,IAcademicRecordsRepository academicRecordsRepository,IDoctorRepository doctorRepository,IActivityLogger logger
            ,IHighBoardRepository highBoardRepository)
        {
            this.departmentRepository = departmentRepository;
            this.subjectRepository = subjectRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.doctorRepository = doctorRepository; 
            this.highBoardRepository = highBoardRepository;
            this.logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Add(int id) 
        {
            ViewData["DepartId"] = id;
            ViewData["Subjects"] = subjectRepository.Select();
            return View();
        }
        [HttpPost]
        public IActionResult SaveAdd(DepartmentSubjects model) 
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var subjectName = subjectRepository.GetName(model.SubjectId);
            var departmentName = departmentRepository.GetOne(model.DepartmentId)?.Name ?? "Unknown";

            var ExistDepartmentSubject = departmentSubjectsRepository.Exist(model);

            if (ExistDepartmentSubject)
            {
                ViewData["DepartId"] = model.DepartmentId;
                ModelState.AddModelError("SubjectId", "This Subject is Already Exist in this Department..");
                ViewData["Subjects"] = subjectRepository.Select();


                logger.Log(
                             actionType: "Add Subject From Department",
                             tableName: "DepartmentSubject",
                             recordId: model.DepartmentId,
                             description: $"{admin.JobTitle} attempted to add subject '{subjectName}' to department '{departmentName}', but it already exists",
                             userId: admin.Id,
                             userName: admin.Name,
                             userRole: UserRole.Admin
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

            logger.Log(
                       actionType: "Add Subject From Department",
                       tableName: "DepartmentSubject",
                       recordId: model.DepartmentId,
                       description: $"{admin.JobTitle} successfully added subject '{subjectName}' to department '{departmentName}'",
                       userId: admin.Id,
                       userName: admin.Name,
                       userRole: UserRole.Admin
            );

            return RedirectToAction("Details", "Department", new { area = "Admin", id = model.DepartmentId });
        }
        public IActionResult Delete(int subjectId, int departmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var subjectName = subjectRepository.GetName(subjectId);
            var departmentName = departmentRepository.GetOne(departmentId)?.Name ?? "Unknown";

            var link = departmentSubjectsRepository.DeleteRelation(subjectId, departmentId);

            if (link == null)
            {
                TempData["ErrorMessage"] = "The relationship between the subject and department could not be found.";


                logger.Log(
                             actionType: "Delete Subject From Department",
                             tableName: "DepartmentSubject",
                             recordId: departmentId,
                             description: $"{admin.JobTitle} attempted to delete subject '{subjectName}' from department '{departmentName}', but no such relation was found",
                             userId: admin.Id,
                             userName: admin.Name,
                             userRole: UserRole.Admin
                    );

                return RedirectToAction("Details", "Department", new { area = "Admin", id = departmentId });

            }

            departmentSubjectsRepository.Delete(link);
            departmentSubjectsRepository.Save();

            logger.Log(
                actionType: "Delete Subject From Department",
                tableName: "DepartmentSubject",
                recordId: departmentId,
                description: $"{admin.JobTitle} successfully deleted subject '{subjectName}' from department '{departmentName}'",
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.HighBoard
            );

            TempData["SuccessMessage"] = "The Subject has been successfully deleted From this department.";
            return RedirectToAction("Details", "Department", new { area = "Admin", id = departmentId });
        }
        public IActionResult DisplaySubjects(int Studentid)
        {
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            else
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            var department = departmentRepository.DepartmentByStudent(Studentid);
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
                ViewData["departmentName"] = department.Name;
                var Subjects = subjectRepository.GetSubjects(Studentid);
                var Department = departmentRepository.DepartmentByStudent(Studentid);
                ViewData["departmentName"] = department.Name;
                ViewBag.DoctorNames = doctorRepository.GetName(Subjects);
                ViewBag.Subjects = Subjects;
                return View(StudentSubjects);
            }
        }
    }
}
