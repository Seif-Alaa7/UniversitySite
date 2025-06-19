using Data.Repository.IRepository;
using Data.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels;
using System.Security.Claims;
using Models.Enums;
using System.Security.Cryptography.Pkcs;

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
        private readonly IStudentRepository studentRepository;
        private readonly IHighBoardRepository highBoardRepository;

        public SubjectController(ISubjectRepository subjectRepository,IDoctorRepository doctorRepository,
            IDepartmentRepository departmentRepository, IDepartmentSubjectsRepository departmentSubjectsRepository,
            IFacultyRepository facultyRepository,IStudentRepository studentRepository, IHighBoardRepository highBoardRepository)
        {
            this.subjectRepository = subjectRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.facultyRepository = facultyRepository;
            this.studentRepository = studentRepository;
            this.highBoardRepository = highBoardRepository;
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var highBoard = highBoardRepository.GetByUserId(userId);
            if (highBoard == null) 
                return Forbid();

            var subject = subjectRepository.GetOne(id);
            if (subject == null) 
                return NotFound();

            var subjectDepartmentIds = departmentSubjectsRepository.SubjectDepartments(id)
                .Select(ds => ds.DepartmentId)
                .ToList();

            if (highBoard.JobTitle == JobTitle.HeadOfDepartment)
            {
                if (!subjectDepartmentIds.Contains(highBoard.Department.Id))
                    return Forbid();
            }
            else if (highBoard.JobTitle == JobTitle.DeanOfFaculty)
            {
                var allowedDepartments = departmentRepository.GetDepartmentsByCollegeId(highBoard.Faculty.Id)
                    .Select(d => d.Id)
                    .ToList();
                if (!subjectDepartmentIds.Any(did => allowedDepartments.Contains(did)))
                    return Forbid();
            }
            else
            {
                return Forbid();
            }
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var highBoard = highBoardRepository.GetByUserId(userId);
            if (highBoard == null)
                return Forbid();

            var subject = subjectRepository.GetOne(model.Id);
            if (subject == null) 
                return NotFound();

            var subjectDepartmentIds = departmentSubjectsRepository
                .SubjectDepartments(subject.Id)
                .Select(ds => ds.DepartmentId)
                .ToList();

            bool isAuthorized = false;

            if (highBoard.JobTitle == JobTitle.HeadOfDepartment)
            {
                isAuthorized = subjectDepartmentIds.Contains(highBoard.Department.Id);
            }
            else if (highBoard.JobTitle == JobTitle.DeanOfFaculty)
            {
                var allowedDepartments = departmentRepository
                    .GetDepartmentsByCollegeId(highBoard.Faculty.Id)
                    .Select(d => d.Id)
                    .ToList();
                isAuthorized = subjectDepartmentIds.Any(did => allowedDepartments.Contains(did));
            }

            if (!isAuthorized)
                return Forbid();

            if (model.Name != subject.Name)
            {
                var exist = subjectRepository.ExistSubject(model.Name);
                if (exist)
                {
                    ModelState.AddModelError("Name", "This Name is Already Exist");
                    return View("Edit", model);
                }
            }

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
                Grade=grade.Grade,
                StudentName = studentRepository.GetStudentName(grade.StudentId)
            }).ToList();

            return Json(gradesViewModel);
        }
        public async Task<IActionResult> ChartDataDoctor(int subjectId)
        {
            ViewBag.SubjectId = subjectId;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);
            if (entity == null)
            {
                return NotFound();
            }
            if (entity is Doctor doctor)
            {
                /*{
                    return Forbid();
                }*/
            }
            if (entity is HighBoard highboard)
            {
                /*int highboardId = highboard.Id;
                var course = await doctorRepository.GetDepartmentForHeadAsync(highboardId, id);
                if (course == null)
                {
                    return NotFound();
                }*/
            }
            return View();
        }
    }
}
