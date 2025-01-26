using Data.Repository;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public DepartmentSubjectsController(IDepartmentRepository departmentRepository, IDepartmentSubjectsRepository departmentSubjectsRepository
            , IAcademicRecordsRepository academicRecordsRepository, ISubjectRepository subjectRepository, IDoctorRepository doctorRepository)
        {
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.subjectRepository = subjectRepository;
            this.doctorRepository = doctorRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult DisplaySubjects(int Studentid)
        {

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            else
            {
                ViewBag.SuccessMessage = TempData["Success"];
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
                ViewBag.ID = department.Id;
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
