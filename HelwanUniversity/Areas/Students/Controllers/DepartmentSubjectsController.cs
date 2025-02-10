using Data.Repository;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels;

namespace HelwanUniversity.Areas.Students.Controllers
{
    [Area("Students")]
    [Authorize(Roles = "Student")]
    public class DepartmentSubjectsController : Controller
    {
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly IAcademicRecordsRepository academicRecordsRepository;
        private readonly IUniFileRepository uniFileRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly IDoctorRepository doctorRepository;    


        public DepartmentSubjectsController(IDepartmentRepository departmentRepository,
            IDepartmentSubjectsRepository departmentSubjectsRepository
            , IAcademicRecordsRepository academicRecordsRepository,
              IUniFileRepository uniFileRepository,
              ISubjectRepository subjectRepository,
              IDoctorRepository doctorRepository)
        {
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.uniFileRepository = uniFileRepository;
            this.subjectRepository = subjectRepository;
            this.doctorRepository = doctorRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult DisplaySubjects(int id)
        {
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            else
            {
                ViewBag.SuccessMessage = TempData["Success"];
            }

            var department = departmentRepository.DepartmentByStudent(id);
            var academicRecord = academicRecordsRepository.GetAll().FirstOrDefault(x => x.StudentId == id);
            if (academicRecord == null || academicRecord.Level == null || academicRecord.Semester == null)
            {
                return NotFound();
            }
            else
            {
                var level = academicRecord.Level;
                var semester = academicRecord.Semester;
                var StudentSubjects = departmentSubjectsRepository.StudentSubjects(level, semester, department.Id);
                ViewData["StudentId"] = id;
                var Subjects = subjectRepository.GetSubjects(id);
                ViewBag.DoctorNames = doctorRepository.GetName(Subjects);
                ViewBag.Subjects = Subjects;
                var Images = uniFileRepository.GetAllImages();
                ViewData["LogoTitle"] = Images[0].File;


                return View(StudentSubjects);
            }
        }
    }
}
