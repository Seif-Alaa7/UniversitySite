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

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SubjectController : Controller
    {
        private readonly ISubjectRepository subjectRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly IAcademicRecordsRepository academicRecordsRepository;
        private readonly IActivityLogger _logger;
        private readonly IHighBoardRepository _highBoardRepository; 

        public SubjectController(ISubjectRepository subjectRepository,IDoctorRepository doctorRepository,
            IDepartmentRepository departmentRepository,
            IDepartmentSubjectsRepository departmentSubjectsRepository,IAcademicRecordsRepository academicRecordsRepository,IHighBoardRepository highBoardRepository
            ,IActivityLogger logger)
        {
            this.subjectRepository = subjectRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this._highBoardRepository = highBoardRepository;
            this._logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Edit(int id, int departmentId)
        {
            var subject = subjectRepository.GetOne(id);


            var subjectVM = new SubjectVM()
            {
                Id = id,
                Name = subject.Name,
                Level = subject.Level,  
                Semester = subject.Semester,
                DoctorId = subject.DoctorId,
                SubjectHours = subject.SubjectHours,
                StudentsAllowed = subject.StudentsAllowed,
                summerStatus = subject.summerStatus,
                subjectType = subject.subjectType,  
                Salary = subject.Salary,
                departmentId = departmentId,
                OriginalDepartmentId = departmentId
            };
            var departments = departmentRepository.Select();

            foreach (var department in departments)
            {
                if (department.Value == departmentId.ToString())
                {
                    department.Selected = true;
                }
            }

            ViewBag.Departments = departments;

            ViewData["DoctorNames"] = doctorRepository.Select();

            return View(subjectVM);
        }
        [HttpPost]
        public IActionResult SaveEdit(SubjectVM model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = _highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var subject = subjectRepository.GetOne(model.Id);

            if (model.Name != subject.Name && subjectRepository.ExistSubject(model.Name))
            {
                ModelState.AddModelError("Name", "This name already exists.");

                _logger.Log(
                    actionType: "Update Subject Name",
                    tableName: "Subject",
                    recordId: subject.Id,
                    description: $"{admin.JobTitle} failed to update the name of subject '{subject.Name}' to '{model.Name}' as the name already exists.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("Edit", model);
            }

            var changes = new List<string>();

            if (subject.Name != model.Name)
                changes.Add($"Name changed to '{model.Name}'");

            if (subject.Level != model.Level)
                changes.Add($"Level changed to '{model.Level}'");

            if (subject.Semester != model.Semester)
                changes.Add($"Semester changed to '{model.Semester}'");

            if (subject.summerStatus != model.summerStatus)
                changes.Add($"Summer Status changed to '{model.summerStatus}'");

            if (subject.subjectType != model.subjectType)
                changes.Add($"Subject Type changed to '{model.subjectType}'");

            if (subject.Salary != model.Salary)
                changes.Add($"Salary changed to '{model.Salary}'");

            if (subject.DoctorId != model.DoctorId)
            {
                var oldDoctor = doctorRepository.GetDoctorOfSubject(subject.Id).Name;
                var newDoctor = doctorRepository.GetDoctorOfSubject(model.DoctorId).Name;
                changes.Add($"Doctor changed from '{oldDoctor}' to '{newDoctor}'");
            }

            subject.Name = model.Name;
            subject.Level = model.Level;
            subject.Semester = model.Semester;
            subject.StudentsAllowed = model.StudentsAllowed;
            subject.summerStatus = model.summerStatus;
            subject.subjectType = model.subjectType;
            subject.Salary = model.Salary;
            subject.SubjectHours = model.SubjectHours;
            subject.DoctorId = model.DoctorId;

            subjectRepository.Update(subject);
            subjectRepository.Save();

            var department = departmentRepository.GetOne(model.departmentId);
            var departmentOld = departmentRepository.GetOne(model.OriginalDepartmentId);

            var departmentSubjectOld = new DepartmentSubjects
            {
                SubjectId = subject.Id,
                DepartmentId = departmentOld.Id
            };

            var departmentSubject = new DepartmentSubjects
            {
                SubjectId = subject.Id,
                DepartmentId = department.Id
            };

            if (departmentSubjectsRepository.Exist(departmentSubject))
            {
                if (changes.Count > 0)
                {
                    _logger.Log(
                        actionType: "Update Subject Details",
                        tableName: "Subject",
                        recordId: subject.Id,
                        description: $"{admin.JobTitle} updated subject '{subject.Name}'. The subject was already linked to department '{department.Name}', so department association remained unchanged. Other updates applied: {string.Join(", ", changes)}.",
                        userId: admin.Id,
                        userName: admin.Name,
                        userRole: UserRole.Admin
                    );
                }

                return RedirectToAction("Details", "Department", new { id = departmentSubject.DepartmentId });
            }
            departmentSubjectsRepository.Delete(departmentSubjectOld);
            departmentSubjectsRepository.Add(departmentSubject);
            departmentRepository.Save();

            changes.Add($"Department changed from '{departmentOld.Name}' to '{department.Name}'");

            _logger.Log(
                actionType: "Update Subject Details",
                tableName: "Subject",
                recordId: subject.Id,
                description: $"{admin.JobTitle} updated subject '{subject.Name}': Department association changed from '{departmentOld.Name}' to '{department.Name}'. Other updates applied: {string.Join(", ", changes)}.",
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.Admin
            );

            return RedirectToAction("Details", "Department", new { id = departmentSubject.DepartmentId });
        }

        public IActionResult DeleteForever(int id, int Departmentid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = _highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var departments = departmentSubjectsRepository.SubjectDepartments(id).ToList();
            var deletedDepartmentNames = departments
                .Select(d => departmentRepository.GetOne(d.DepartmentId)?.Name ?? "Unknown")
                .ToList();

            var departmentsCount = deletedDepartmentNames.Count;
            string departmentLabel = departmentsCount == 1 ? "department" : "departments";

            foreach (var department in departments)
            {
                departmentSubjectsRepository.Delete(department);
            }
            departmentSubjectsRepository.Save();

            var subject = subjectRepository.GetOne(id);
            var subjectName = subject.Name;
            var subjectId = subject.Id;

            subjectRepository.Delete(subject);
            subjectRepository.Save();

            string departmentList = string.Join(", ", deletedDepartmentNames);

            TempData["SuccessMessage"] = "The Subject has been successfully deleted Forever.";

            _logger.Log(
                actionType: "Delete Subject",
                tableName: "Subject",
                recordId: subjectId,
                description: $"{admin.JobTitle} permanently deleted subject '{subjectName}' from {departmentsCount} {departmentLabel}: {departmentList}",
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.Admin
            );

            return RedirectToAction("Details", "Department", new { id = Departmentid });
        }
        public IActionResult ResultsRegisteration(int id)
        {
            var Subjects = subjectRepository.GetSubjects(id);
            ViewBag.DoctorNames = doctorRepository.GetName(Subjects);
            var department = departmentRepository.DepartmentByStudent(id);

            ViewData["StudentId"] = id;
            ViewData["departmentName"] = department.Name;

            var subjectDegree = subjectRepository.ReturnDegrees(Subjects,id);
            var subjectGrade = subjectRepository.ReturnGrades(Subjects,id);

            ViewData["subjectDegree"] = subjectDegree;
            ViewData["subjectGrade"] = subjectGrade;

            return View(Subjects);
        }
    }
}
