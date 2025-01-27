using Data.Repository;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

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
        public DepartmentSubjectsController(IDepartmentRepository departmentRepository, IDepartmentSubjectsRepository departmentSubjectsRepository
            , IAcademicRecordsRepository academicRecordsRepository, ISubjectRepository subjectRepository, IDoctorRepository doctorRepository
            , IFacultyRepository facultyRepository)
        {
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.subjectRepository = subjectRepository;
            this.doctorRepository = doctorRepository;
            this.facultyRepository = facultyRepository;
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
            var ExistDepartmentSubject = departmentSubjectsRepository.Exist(model);

            if (ExistDepartmentSubject)
            {
                ViewData["DepartId"] = model.DepartmentId;
                ModelState.AddModelError("SubjectId", "This Subject is Already Exist in this Department..");
                ViewData["Subjects"] = subjectRepository.Select();
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
            return RedirectToAction("Details", "Department", new { area = "Doctors", id = model.DepartmentId });
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
        public IActionResult Delete(int subjectId, int departmentId)
        {
            var link = departmentSubjectsRepository.DeleteRelation(subjectId, departmentId);
            if (link == null)
            {
                TempData["ErrorMessage"] = "The relationship between the subject and department could not be found.";
                return RedirectToAction("Details", "Department", new { area = "Doctors", id = departmentId });
            }
            departmentSubjectsRepository.Delete(link);
            departmentSubjectsRepository.Save();
            return RedirectToAction("Details", "Department", new { area = "Doctors", id = departmentId });
        }
    }
}
