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
        public DepartmentSubjectsController(IDepartmentRepository departmentRepository, IDepartmentSubjectsRepository departmentSubjectsRepository
            , IAcademicRecordsRepository academicRecordsRepository)
        {
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult DisplaySubjects(int Studentid)
        {

            var department = departmentRepository.DepartmentByStudent(Studentid);
            var level = academicRecordsRepository.GetAll().FirstOrDefault(x => x.StudentId == Studentid).Level;
            var semester = academicRecordsRepository.GetAll().FirstOrDefault(x => x.StudentId == Studentid).Semester;

            ViewData["StudentId"] = Studentid;
            ViewData["departmentName"] = department.Name;
            var StudentSubjects = departmentSubjectsRepository.StudentSubjects(level, semester, department.Id);
            return View(StudentSubjects);
        }
    }
}
