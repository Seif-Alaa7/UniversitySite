using Data.Repository;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

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

        public DepartmentSubjectsController(IDepartmentRepository departmentRepository,
            IDepartmentSubjectsRepository departmentSubjectsRepository
            , IAcademicRecordsRepository academicRecordsRepository,
              IUniFileRepository uniFileRepository)
        {
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.uniFileRepository = uniFileRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult DisplaySubjects(int id)
        {

            var department = departmentRepository.DepartmentByStudent(id);
            var level = academicRecordsRepository.GetAll().FirstOrDefault(x => x.StudentId == id).Level;
            var semester = academicRecordsRepository.GetAll().FirstOrDefault(x => x.StudentId == id).Semester;
            var Images = uniFileRepository.GetAllImages();

            ViewData["StudentId"] = id;
            ViewData["LogoTitle"] = Images[0].File;

            var StudentSubjects = departmentSubjectsRepository.StudentSubjects(level, semester, department.Id);
            return View(StudentSubjects);
        }
    }
}
