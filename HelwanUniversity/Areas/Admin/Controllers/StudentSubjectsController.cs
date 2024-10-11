using Data.Repository;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Enums;
using ViewModels;

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentSubjectsController : Controller
    {
        private readonly IStudentSubjectsRepository studentSubjectsRepository;
        private readonly IAcademicRecordsRepository academicRecordsRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IDepartmentRepository departmentRepository;
        public StudentSubjectsController(IStudentSubjectsRepository studentSubjectsRepository,
            IAcademicRecordsRepository academicRecordsRepository, ISubjectRepository subjectRepository, IStudentRepository studentRepository,
            IDoctorRepository doctorRepository, IDepartmentRepository departmentRepository)
        {
            this.studentSubjectsRepository = studentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.subjectRepository = subjectRepository;
            this.studentRepository = studentRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddSubject(int studentId, int subjectId)
        {
            var exists = studentSubjectsRepository.Exist(studentId, subjectId);
            if (exists)
            {
                ModelState.AddModelError("", "This subject is already registered.");
                return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
            }

            var studentSubject = new StudentSubjects
            {
                StudentId = studentId,
                SubjectId = subjectId,
                Degree = 0,
                Grade = Grade.F
            };

            // Add Student Subject
            studentSubjectsRepository.Add(studentSubject);
            studentSubjectsRepository.Save();

            // Calculate Academic Records
            UpdateAcademicRecords(studentId);

            return RedirectToAction("SubjectRegsitered", new { id = studentId });
        }
        public IActionResult DeleteSubject(int studentId, int subjectId)
        {
            var links = studentSubjectsRepository.FindStudent(studentId);
            if (links.Count() == 1)
            {
                academicRecordsRepository.DeleteByStudent(studentId);
                studentRepository.Delete(studentId);

                studentRepository.Save();

                foreach (var model in links)
                {
                    studentSubjectsRepository.Delete(model);
                    studentSubjectsRepository.Save();
                }

                // Update Academic Records
                UpdateAcademicRecords(studentId);

                return RedirectToAction("SubjectRegsitered", new { id = studentId });
            }
            else
            {
                var link = studentSubjectsRepository.GetOne(studentId, subjectId);
                if (link == null)
                {
                    ModelState.AddModelError("", "you Can't Delete Subject because you Did not Add");
                    return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
                }
                else
                {
                    studentSubjectsRepository.Delete(link);
                    studentSubjectsRepository.Save();

                    // Update Academic Records
                    UpdateAcademicRecords(studentId);
                }
            }
            return RedirectToAction("SubjectRegsitered", new { id = studentId });
        }
        public IActionResult SubjectRegsitered(int id)
        {

            var Subjects = subjectRepository.GetSubjects(id);
            var department = departmentRepository.DepartmentByStudent(id);

            ViewData["departmentName"] = department.Name;
            ViewBag.DoctorNames = doctorRepository.GetName(Subjects);

            return View(Subjects);
        }
        public IActionResult DisplayDegrees(int id)
        {
            var studentSubjects = studentSubjectsRepository.FindStudent(id);
            var Subjects = subjectRepository.GetSubjects(id);
            var AcademicRecords = academicRecordsRepository.GetStudent(id);

            ViewBag.SubjectNames = subjectRepository.GetName(Subjects);
            ViewData["AcademicRecords"] = AcademicRecords;

            return View(studentSubjects);
        }
        [HttpGet]
        public IActionResult AddDegree(int Studentid, int Subjectid)
        {
            var ModelVM = new StudentSubjectsVM()
            {
                StudentId = Studentid,
                SubjectId = Subjectid
            };
            return View(ModelVM);
        }
        [HttpPost]
        public IActionResult SaveAdd(StudentSubjectsVM modelVM)
        {

            var studentId = modelVM.StudentId;
            var academicRecords = academicRecordsRepository.GetStudent(studentId);
            var CurrentLevel = academicRecordsRepository.GetStudent(studentId)?.Level;

            var StudentSubject = studentSubjectsRepository.GetOne(modelVM.StudentId, modelVM.SubjectId);
            StudentSubject.Degree = modelVM.Degree;
            StudentSubject.Grade = studentSubjectsRepository.CalculateGrade(modelVM.Degree);

            studentSubjectsRepository.Update(StudentSubject);
            studentSubjectsRepository.Save();

            UpdateAcademicRecords(modelVM.StudentId);

            var credithours = studentSubjectsRepository.CalculateCreditHours(modelVM.StudentId);
            var semester = studentSubjectsRepository.Calculatesemester(credithours);

            var gpaSemester = academicRecordsRepository.CalculateGpaSemester(modelVM.StudentId , semester);
            var gpaTotal = academicRecordsRepository.CalculateGPATotal(modelVM.StudentId);

            if (academicRecords != null)
            {
                academicRecords.GPASemester = gpaSemester;
                academicRecords.GPATotal = gpaTotal;
                academicRecordsRepository.Update(academicRecords);
                academicRecordsRepository.Save();

                if (academicRecords.Level != CurrentLevel)
                {
                    var student = studentRepository.GetOne(studentId);
                    if (student != null)
                    {
                        if (student.PaymentFees == true)
                        {
                            student.PaymentFees = false;
                        }
                        else
                        {
                            student.PaymentFees = true;
                        }
                        studentRepository.Update(student);
                        studentRepository.Save();
                    }
                }
            }
            return RedirectToAction("DisplayDegrees", new { id = modelVM.StudentId });
        }
        public IActionResult StudentSubjectRegistered(int id)
        {

            ViewData["SubjectName"] = subjectRepository.GetName(id);
            ViewData["Level"] = subjectRepository.GetLevel(id);
            ViewData["Semester"] = subjectRepository.GetSemester(id);
            ViewData["id"] = id;

            var students = studentRepository.StudentsBySubject(id); 
            return View(students);
        }

        private void UpdateAcademicRecords(int studentId)
        {
            var creditHours = studentSubjectsRepository.CalculateCreditHours(studentId);
            var semester = studentSubjectsRepository.Calculatesemester(creditHours);
            var level = studentSubjectsRepository.CalculateLevel(creditHours);
            var totalPoints = studentSubjectsRepository.CalculateTotalPoints(studentId);
            var semesterPoints = studentSubjectsRepository.CalculateSemesterPoints(studentId, semester);
            var recordedHours = studentSubjectsRepository.CalculateRecordedHours(studentId, semester);
            var totalHours = studentSubjectsRepository.CalculateTotalHours(studentId);
            var gpaSemester = academicRecordsRepository.CalculateGpaSemester(studentId, semester);
            var gpaTotal = academicRecordsRepository.CalculateGPATotal(studentId);

            var academicRecords = academicRecordsRepository.GetStudent(studentId);

            if (academicRecords != null)
            {
                academicRecords.CreditHours = creditHours;
                academicRecords.Semester = semester;
                academicRecords.Level = level;
                academicRecords.TotalPoints = totalPoints;
                academicRecords.SemesterPoints = semesterPoints;
                academicRecords.RecordedHours = recordedHours;
                academicRecords.TotalHours = totalHours;
                academicRecords.GPASemester = gpaSemester;
                academicRecords.GPATotal = gpaTotal;

                academicRecordsRepository.Update(academicRecords);
                academicRecordsRepository.Save();
            }
        }
    }
}
