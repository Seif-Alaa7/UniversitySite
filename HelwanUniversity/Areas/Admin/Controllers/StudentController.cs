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

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly IStudentRepository studentRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IFacultyRepository facultyRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IUniversityRepository universityRepository;
        private readonly IAcademicRecordsRepository academicRecordsRepository;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IActivityLogger _logger;

        public StudentController(IStudentRepository studentRepository , IDepartmentRepository departmentRepository, IFacultyRepository facultyRepository,
            ICloudinaryService cloudinaryService, IUniversityRepository universityRepository,
            IAcademicRecordsRepository academicRecordsRepository, IActivityLogger logger, IHighBoardRepository highBoardRepository)
        {
            this.studentRepository = studentRepository;
            this.departmentRepository = departmentRepository;
            this.facultyRepository = facultyRepository;
            this.cloudinaryService = cloudinaryService;
            this.universityRepository = universityRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            _logger = logger;
            this.highBoardRepository = highBoardRepository;
        }
        public IActionResult Index()
        {
            var Students = studentRepository.GetAll();
            ViewData["DepartmentNames"] = departmentRepository.Dict();

            ViewBag.FacultyNames = facultyRepository.GetNames(Students);
            ViewData["Records"] = academicRecordsRepository.GetLevelANDSemester(Students);

            ViewBag.FacultyDepartments = departmentRepository.GetDepartmentsByFaculty();

            return View(Students);
        }
        public IActionResult Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var studentDatails = studentRepository.GetOne(id);
            if (studentDatails == null)
                return NotFound();

            var department = departmentRepository.DepartmentByStudent(id);
            var facultyData = department != null ? facultyRepository.FacultyByDepartment(department.Id) : null;

            ViewData["Faculty"] = facultyData;
            ViewData["Department"] = department;
            ViewData["FormBifurcation"] = universityRepository.Get()?.GoogleForm;

            string departmentName = department != null ? department.Name : "No Department";
            string facultyName = facultyData != null ? facultyData.Name : "No Faculty";

            string extraDetails = department != null
                ? $" from Department '{departmentName}' and Faculty '{facultyName}'"
                : " who is not assigned to any Department or Faculty";

            _logger.Log(
                actionType: "Show Student Details",
                tableName: "Student",
                recordId: studentDatails.Id,
                description: $"{admin.JobTitle} viewed the profile of student '{studentDatails.Name}'{extraDetails}.",
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.Admin
            );

            return View(studentDatails);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var student = studentRepository.GetOne(id);
            ViewBag.Departments = new SelectList(departmentRepository.GetAll(), "Id", "Name");
            var studentVM = new StudentVM
            {
                Id = id,
                Name = student.Name,
                DepartmentId = student.DepartmentId,
                Address = student.Address,
                AdmissionDate = student.AdmissionDate,
                BirthDate = student.BirthDate,
                Gender = student.Gender,
                Nationality = student.Nationality,
                PaymentFees = student.PaymentFees,
                PaymentFeesDate = student.PaymentFeesDate,
                PhoneNumber = student.PhoneNumber,
                Picture = student.Picture,
                Religion = student.Religion
            };
            return View(studentVM);
        }
        [HttpPost]
        public async Task<IActionResult> SaveEdit(StudentVM studentVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var student = studentRepository.GetOne(studentVM.Id);

            var oldDepartment = departmentRepository.GetOne(student.DepartmentId)?.Name ?? "Unknown";
            var oldFaculty = facultyRepository.FacultyByDepartment(student.DepartmentId)?.Name ?? "Unknown";

            if (studentVM.Name != student.Name)
            {
                var Exist = studentRepository.Exist(studentVM.Name);
                if (Exist)
                {
                    ModelState.AddModelError("Name", "This Name is Already Exist");
                    ViewBag.Departments = new SelectList(departmentRepository.GetAll(), "Id", "Name");

                    _logger.Log(
                        actionType: "Update Student Name",
                        tableName: "Student",
                        recordId: student.Id,
                        description: $"{admin.JobTitle} attempted to change student name to '{studentVM.Name}' but it already exists. Student belongs to Department '{oldDepartment}', Faculty '{oldFaculty}'.",
                        userId: admin.Id,
                        userName: admin.Name,
                        userRole: UserRole.Admin
                    );

                    return View("Edit", studentVM);
                }
            }

            if (studentVM.PhoneNumber != student.PhoneNumber)
            {
                var Exist = studentRepository.ExistPhone(studentVM.PhoneNumber);
                if (Exist)
                {
                    ModelState.AddModelError("PhoneNumber", "This Phone is Already Exist");
                    ViewBag.Departments = new SelectList(departmentRepository.GetAll(), "Id", "Name");

                    _logger.Log(
                        actionType: "Update Student Phone Number",
                        tableName: "Student",
                        recordId: student.Id,
                        description: $"{admin.JobTitle} attempted to change student phone number to '{studentVM.PhoneNumber}' but it already exists. Student belongs to Department '{oldDepartment}', Faculty '{oldFaculty}'.",
                        userId: admin.Id,
                        userName: admin.Name,
                        userRole: UserRole.Admin
                    );

                    return View("Edit", studentVM);
                }
            }

            try
            {
                studentVM.Picture = await cloudinaryService.UploadFile(studentVM.FormFile, student.Picture, "An error occurred while uploading the photo. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Departments = new SelectList(departmentRepository.GetAll(), "Id", "Name");

                _logger.Log(
                    actionType: "Update Student Picture",
                    tableName: "Student",
                    recordId: student.Id,
                    description: $"{admin.JobTitle} failed to update student '{student.Name}' photo due to error: {ex.Message}. Student belongs to Department '{oldDepartment}', Faculty '{oldFaculty}'.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("Edit", studentVM);
            }

            var sensitiveChanges = new List<string>();

            if (student.Name != studentVM.Name)
                sensitiveChanges.Add($"Name changed from '{student.Name}' to '{studentVM.Name}'");

            if (student.DepartmentId != studentVM.DepartmentId)
            {
                var newDepartment = departmentRepository.GetOne(studentVM.DepartmentId)?.Name ?? "Unknown";
                var newFaculty = facultyRepository.FacultyByDepartment(studentVM.DepartmentId)?.Name ?? "UnKnown";
                sensitiveChanges.Add($"Department changed from '{oldDepartment}' (Faculty '{oldFaculty}') to '{newDepartment}' (Faculty '{newFaculty}')");
            }

            if (student.PaymentFees != studentVM.PaymentFees)
                sensitiveChanges.Add($"Payment Fees changed from '{(student.PaymentFees ? "Paid" : "Not Paid")}' to '{(studentVM.PaymentFees ? "Paid" : "Not Paid")}'");

            if (student.Picture != studentVM.Picture)
                sensitiveChanges.Add("Profile picture has been updated");

            student.PhoneNumber = studentVM.PhoneNumber;
            student.Address = studentVM.Address;
            student.AdmissionDate = studentVM.AdmissionDate;
            student.BirthDate = studentVM.BirthDate;
            student.Name = studentVM.Name;
            student.Picture = studentVM.Picture;
            student.Gender = studentVM.Gender;
            student.Religion = studentVM.Religion;
            student.DepartmentId = studentVM.DepartmentId;
            student.Nationality = studentVM.Nationality;
            student.PaymentFees = studentVM.PaymentFees;
            student.PaymentFeesDate = studentVM.PaymentFeesDate;

            studentRepository.Update(student);
            studentRepository.Save();

            if (sensitiveChanges.Any())
            {
                _logger.Log(
                    actionType: "Update Student Details",
                    tableName: "Student",
                    recordId: student.Id,
                    description: $"{admin.JobTitle} updated student '{student.Name}' in department '{oldDepartment}'. Changes: {string.Join(", ", sensitiveChanges)}.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );
            }
            else
            {
                _logger.Log(
                    actionType: "Update Student Details",
                    tableName: "Student",
                    recordId: student.Id,
                    description: $"{admin.JobTitle} updated student '{student.Name}' in Department '{oldDepartment}', Faculty '{oldFaculty}' with no sensitive changes.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );
            }

            return RedirectToAction("Details", new { id = student.Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var student = studentRepository.GetOne(id);
            if (student == null)
                return NotFound();

            var department = departmentRepository.DepartmentByStudent(student.Id);
            var departmentName = department?.Name ?? "Unknown";

            var faculty = facultyRepository.FacultyByDepartment(department?.Id ?? 0);
            var facultyName = faculty?.Name ?? "Unknown";

            var applicationUserId = student.ApplicationUserId;

            studentRepository.Delete(id);
            studentRepository.Save();

            bool userDeleted = false;
            if (!string.IsNullOrEmpty(applicationUserId))
            {
                studentRepository.DeleteUser(applicationUserId);
                studentRepository.Save();
                userDeleted = true;
            }

            TempData["SuccessMessage"] = "The student and their user account have been successfully deleted.";

            _logger.Log(
                actionType: "Delete Student",
                tableName: "Student",
                recordId: student.Id,
                description: $"{admin.JobTitle} permanently deleted student '{student.Name}' from department '{departmentName}', faculty '{facultyName}'{(userDeleted ? " and their associated user account." : ". No user account was associated.")}",
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.Admin
            );

            return RedirectToAction("Index");
        }
        public IActionResult StudentsByDepartment(int id)
        {
            var students = studentRepository.GetStudents(id).ToList();

            ViewBag.Records = academicRecordsRepository.GetLevelANDSemester(students);
            ViewData["DepartmentName"] = departmentRepository.GetOne(id)?.Name;
            ViewBag.Students = students;    
            return View(students);
        }
    }
}
