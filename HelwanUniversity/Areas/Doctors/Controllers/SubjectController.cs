using Data.Repository.IRepository;
using Data.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class SubjectController : Controller
    {
        private readonly ISubjectRepository subjectRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly IFacultyRepository facultyRepository;

        public SubjectController(ISubjectRepository subjectRepository,IDoctorRepository doctorRepository,
            IDepartmentRepository departmentRepository, IDepartmentSubjectsRepository departmentSubjectsRepository,
            IFacultyRepository facultyRepository)
        {
            this.subjectRepository = subjectRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.facultyRepository = facultyRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ResultsRegisteration(int id)
        {
            var Subjects = subjectRepository.GetSubjects(id);
            ViewBag.DoctorNames = doctorRepository.GetName(Subjects);
            var department = departmentRepository.DepartmentByStudent(id);
            ViewData["StudentId"] = id;
            ViewData["departmentName"] = department.Name;

            return View(Subjects);
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
            var faculty = facultyRepository.FacultyByDepartment(departmentId);
            var departments = departmentRepository.SelectDepartsByFaculty(faculty.Id);
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
            var subject = subjectRepository.GetOne(model.Id);
            if (model.Name != subject.Name)
            {
                var exist = subjectRepository.ExistSubject(model.Name);
                if (exist)
                {
                    ModelState.AddModelError("Name", "This Name is Already Exist");
                    return View("Edit", model);
                }
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
            var departmentSubjectOld = new DepartmentSubjects()
            {
                SubjectId = subject.Id,
                DepartmentId = departmentOld.Id,
            };
            var departmentSubject = new DepartmentSubjects()
            {
                DepartmentId = department.Id,
                SubjectId = subject.Id,
            };
            var exists = departmentSubjectsRepository.Exist(departmentSubject);
            if (exists)
            {
                return RedirectToAction("Details", "Department", new { id = departmentSubject.DepartmentId });
            }
            departmentSubjectsRepository.Delete(departmentSubjectOld);
            departmentSubjectsRepository.Add(departmentSubject);
            departmentRepository.Save();
            return RedirectToAction("Details", "Department", new { id = departmentSubject.DepartmentId });
        }
        public IActionResult DeleteForever(int id, int Departmentid)
        {
            var Departments = departmentSubjectsRepository.SubjectDepartments(id);
            foreach (var department in Departments)
            {
                departmentSubjectsRepository.Delete(department);
            }
            var subject = subjectRepository.GetOne(id);
            subjectRepository.Delete(subject);
            departmentSubjectsRepository.Save();
            return RedirectToAction("Details", "Department", new { id = Departmentid });
        }
        
        [HttpGet]
        public IActionResult GetGrades(int subjectId)
        {
            var grades = subjectRepository.GetStudentGrades(subjectId);

            var gradesViewModel = grades.Select(grade => new StudentSubjectsVM
            {
                StudentId = grade.StudentId,
                SubjectId = subjectId,
                Degree = grade.Degree,
                Grade=grade.Grade
            }).ToList();
            return Json(gradesViewModel);
        }
        public IActionResult ChartData()
        {

            return View();
        }

        
    }
}
