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
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository departmentRepository;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IFacultyRepository facultyRepository;
        private readonly IActivityLogger logger;
        private readonly IStudentRepository studentRepository;

        public DepartmentController(IDepartmentRepository departmentRepository,IHighBoardRepository highBoardRepository,
            IDepartmentSubjectsRepository departmentSubjectsRepository,IDoctorRepository doctorRepository,IFacultyRepository facultyRepository,
            IActivityLogger logger, IStudentRepository studentRepository)
        {
            this.departmentRepository = departmentRepository;
            this.highBoardRepository = highBoardRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.doctorRepository = doctorRepository;
            this.facultyRepository = facultyRepository;
            this.studentRepository = studentRepository;
            this.logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            var department = departmentRepository.GetOne(id);

            if (department == null)
            {
                TempData["ErrorMessage"] = "The department could not be found.";
                return RedirectToAction("Index", "Faculty");
            }
            ViewData["Head"] = highBoardRepository.GetName(department.HeadId);

            ViewBag.Subjects = departmentSubjectsRepository.subjectsByDepartment(id);
            ViewData["Students"] = departmentRepository.GetStudentCount(id);

            ViewBag.StudentsBySubject = departmentSubjectsRepository.StudentCounts(ViewBag.Subjects);
            ViewBag.DoctorNames = doctorRepository.GetName(ViewBag.Subjects);

            return View(department);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var department = departmentRepository.GetOne(id);

            ViewData["Heads"] = highBoardRepository.selectHeads();
            ViewData["Faculities"] = facultyRepository.Select();

            //Mapping
            var departmentVM = new DepartmentVM()
            {
                Id = id,
                HeadId = department.HeadId,
                Name = department.Name,
                FacultyId = department.FacultyId,
                Allowed = department.Allowed,
            };

            return View(departmentVM);
        }
        [HttpPost]
        public IActionResult SaveEdit(DepartmentVM departmentVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var department = departmentRepository.GetOne(departmentVM.Id);

            if (departmentVM.Name != department.Name && departmentRepository.Exist(departmentVM.Name))
            {
                ModelState.AddModelError("Name", "Error, you are trying to change the department name to an existing name. Try another name.");

                ViewData["Heads"] = highBoardRepository.selectHeads();
                ViewData["Faculities"] = facultyRepository.Select();

                logger.Log(
                    actionType: "Update Department Name",
                    tableName: "Department",
                    recordId: department.Id,
                    description: $"{admin.JobTitle} attempted to change the department name from '{department.Name}' to '{departmentVM.Name}', but the name already exists.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("Edit", departmentVM);
            }

            if (department.HeadId != departmentVM.HeadId && departmentRepository.ExistHeadInDepartment(departmentVM.HeadId))
            {
                ModelState.AddModelError("HeadId", "This person is already the head of another department.");

                ViewData["Heads"] = highBoardRepository.selectHeads();
                ViewData["Faculities"] = facultyRepository.Select();

                var newHeadName = highBoardRepository.GetName(departmentVM.HeadId);

                logger.Log(
                    actionType: "Update Department Head",
                    tableName: "Department",
                    recordId: department.Id,
                    description: $"{admin.JobTitle} attempted to change the head of department '{department.Name}' to '{newHeadName}', but this person is already head of another department.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("Edit", departmentVM);
            }

            var changes = new List<string>();

            if (department.Name != departmentVM.Name)
                changes.Add($"Name changed to '{departmentVM.Name}'");

            if (department.HeadId != departmentVM.HeadId)
            {
                var oldHeadName = highBoardRepository.GetName(department.HeadId);
                var newHeadName = highBoardRepository.GetName(departmentVM.HeadId);
                changes.Add($"Head changed from '{oldHeadName}' to '{newHeadName}'");
            }

            if (department.FacultyId != departmentVM.FacultyId)
            {
                var oldFacultyName = facultyRepository.GetOne(department.FacultyId)?.Name ?? "Unknown";
                var newFacultyName = facultyRepository.GetOne(departmentVM.FacultyId)?.Name ?? "Unknown";
                changes.Add($"Faculty changed from '{oldFacultyName}' to '{newFacultyName}'");
            }

            if (department.Allowed != departmentVM.Allowed)
                changes.Add($"Allowed Students changed to '{departmentVM.Allowed}'");

            department.Name = departmentVM.Name;
            department.HeadId = departmentVM.HeadId;
            department.FacultyId = departmentVM.FacultyId;
            department.Allowed = departmentVM.Allowed;

            departmentRepository.Update(department);
            departmentRepository.Save();

            if (changes.Any())
            {
                logger.Log(
                    actionType: "Update Department Details",
                    tableName: "Department",
                    recordId: department.Id,
                    description: $"{admin.JobTitle} updated department '{department.Name}'. Changes applied: {string.Join(", ", changes)}.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );
            }

            if (departmentVM.FacultyId != department.FacultyId)
                return RedirectToAction("Details", "Faculty", new { id = department.FacultyId });

            return RedirectToAction("Details", new { id = department.Id });
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var department = departmentRepository.GetOne(id);
            if (department == null)
                return NotFound();

            var studentsCount = studentRepository.GetStudents(id).Count();
            if (studentsCount > 0)
            {
                TempData["ErrorMessage"] = "You cannot delete this department because it still has registered students.";

                logger.Log(
                    actionType: "Delete Department",
                    tableName: "Department",
                    recordId: id,
                    description: $"{admin.JobTitle} attempted to delete department '{department.Name}' but deletion was prevented because there are still {studentsCount} registered students.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return RedirectToAction("Details", "Faculty", new { id = department.FacultyId });
            }

            var departmentSubjects = departmentSubjectsRepository.GetAll().Where(ds => ds.DepartmentId == id).ToList();
            foreach (var depSubject in departmentSubjects)
            {
                departmentSubjectsRepository.Delete(depSubject);
            }

            departmentSubjectsRepository.Save();

            departmentRepository.Delete(department);
            departmentRepository.Save();

            logger.Log(
                actionType: "Delete Department",
                tableName: "Department",
                recordId: id,
                description: $"{admin.JobTitle} successfully deleted department '{department.Name}'.",
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.Admin
            );

            TempData["SuccessMessage"] = "The Department has been successfully deleted.";
            return RedirectToAction("Details", "Faculty", new { id = department.FacultyId });
        }

    }
}
