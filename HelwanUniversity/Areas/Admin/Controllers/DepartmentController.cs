using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
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

        public DepartmentController(IDepartmentRepository departmentRepository,IHighBoardRepository highBoardRepository,
            IDepartmentSubjectsRepository departmentSubjectsRepository,IDoctorRepository doctorRepository,IFacultyRepository facultyRepository)
        {
            this.departmentRepository = departmentRepository;
            this.highBoardRepository = highBoardRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.doctorRepository = doctorRepository;
            this.facultyRepository = facultyRepository;
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
            var department = departmentRepository.GetOne(departmentVM.Id);
            if(departmentVM.Name != department.Name)
            {
                var exist = departmentRepository.Exist(departmentVM.Name);
                if(exist)
                {
                    ModelState.AddModelError("Name", "Error, you try to change department name to an existing name. Try another name.");

                    ViewData["Heads"] = highBoardRepository.selectHeads();
                    ViewData["Faculities"] = facultyRepository.Select();

                    return View("Edit");
                }
            };

            if(department.HeadId != departmentVM.HeadId) 
            {
                var Exist = departmentRepository.ExistHeadInDepartment(departmentVM.HeadId);
                if (Exist)
                {
                    ModelState.AddModelError("HeadId", "This person is already a head of a registered department.");

                    ViewData["Heads"] = highBoardRepository.selectHeads();
                    ViewData["Faculities"] = facultyRepository.Select();

                    return View("Edit");
                }
            };

            department.HeadId = departmentVM.HeadId;
            department.Name = departmentVM.Name;
            department.FacultyId = departmentVM.FacultyId;
            department.Allowed = departmentVM.Allowed;

            departmentRepository.Update(department);
            departmentRepository.Save();

            if (departmentVM.FacultyId == department.FacultyId)
            {
               return RedirectToAction("Details","Faculty", new { id = department.FacultyId });
            };
            return RedirectToAction("Details", new {id = department.Id});
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var department = departmentRepository.GetOne(id);

            var departmentSubjects = departmentSubjectsRepository.GetAll().Where(ds => ds.DepartmentId == id).ToList();
            foreach(var Department in departmentSubjects)
            {
               departmentSubjectsRepository.Delete(Department);
            }
            departmentRepository.Delete(department);
            departmentRepository.Save();

            return RedirectToAction("Details", "Faculty", new { id = department.FacultyId });
        }
    }
}
