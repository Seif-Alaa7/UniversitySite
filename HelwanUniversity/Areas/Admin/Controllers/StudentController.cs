using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public StudentController(IStudentRepository studentRepository , IDepartmentRepository departmentRepository,IFacultyRepository facultyRepository,
            ICloudinaryService cloudinaryService,IUniversityRepository universityRepository,
            IAcademicRecordsRepository academicRecordsRepository)
        {
            this.studentRepository = studentRepository;
            this.departmentRepository = departmentRepository;
            this.facultyRepository = facultyRepository;
            this.cloudinaryService = cloudinaryService;
            this.universityRepository = universityRepository;
            this.academicRecordsRepository = academicRecordsRepository;
        }
        public IActionResult Index()
        {
            var Students = studentRepository.GetAll();
            ViewData["DepartmentNames"] = departmentRepository.Dict();

            ViewBag.FacultyNames = facultyRepository.GetNames(Students);
            ViewData["Records"] = academicRecordsRepository.GetLevelANDSemester(Students);

            return View(Students);
        }
        public IActionResult Details(int id)
        {

            var studentDatails = studentRepository.GetOne(id);
            var department = departmentRepository.DepartmentByStudent(id);

            if (department != null)
            {
                var facultyData = facultyRepository.FacultyByDepartment(department.Id);
                ViewData["Faculty"] = facultyData;
            }
            else
            {
                ViewData["Faculty"] = null;
            }
            ViewData["FormBifurcation"] = universityRepository.Get().GoogleForm;  

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
            var student = studentRepository.GetOne(studentVM.Id);

            if(studentVM.Name != student.Name)
            {
                var Exist = studentRepository.Exist(studentVM.Name);
                if (Exist)
                {
                    ModelState.AddModelError("Name", "This Name is Already Exist");
                    ViewBag.Departments = new SelectList(departmentRepository.GetAll(), "Id", "Name");
                    return View("Edit", studentVM);
                }
            }
            if(studentVM.PhoneNumber != student.PhoneNumber)
            {
                var Exist = studentRepository.ExistPhone(studentVM.PhoneNumber);
                if (Exist)
                {
                    ModelState.AddModelError("PhoneNumber", "This Phone is Already Exist");
                    ViewBag.Departments = new SelectList(departmentRepository.GetAll(), "Id", "Name");
                    return View("Edit", studentVM);
                }
            }
            try
            {
                studentVM.Picture = await cloudinaryService.UploadFile(studentVM.FormFile,student.Picture, "An error occurred while uploading the photo. Please try again.");

            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Departments = new SelectList(departmentRepository.GetAll(), "Id", "Name");
                return View("Edit",studentVM);
            }

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
            return RedirectToAction("Details", new { id = student.Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var student = studentRepository.GetOne(id);
            var UserId = student.ApplicationUserId;

            studentRepository.Delete(id);
            studentRepository.Save();

            studentRepository.DeleteUser(UserId);
            studentRepository.Save();

            TempData["SuccessMessage"] = "The Student has been successfully deleted";

            return RedirectToAction("Index");
        }
        public IActionResult StudentsByDepartment(int id)
        {
            var students = studentRepository.GetStudents(id).ToList();

            ViewBag.Records = academicRecordsRepository.GetLevelANDSemester(students);
            ViewData["DepartmentName"] = departmentRepository.GetOne(id)?.Name;

            return View(students);
        }
    }
}
