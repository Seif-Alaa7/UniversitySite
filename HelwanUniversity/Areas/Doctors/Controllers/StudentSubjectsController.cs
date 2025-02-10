using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models.Enums;
using Models;
using ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
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
                TempData["ErrorMessage"] = "This subject is already registered.";
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

            TempData["Success"] = "Subject has been successfully added.";
            return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
        }
        public IActionResult DeleteSubject(int studentId, int subjectId)
        {
            var links = studentSubjectsRepository.FindStudent(studentId);
            if (links.Count() == 1)
            {
                TempData["ErrorMessage"] = "You cannot delete this subject as it's the only one registered. Removing it will delete the student record.";
                return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
            }
            else
            {
                var link = studentSubjectsRepository.GetOne(studentId, subjectId);
                if (link == null)
                {
                    TempData["ErrorMessage"] = "you Can't Delete Subject because you Did not Add";
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
            TempData["Success"] = "Subject has been successfully Deleted.";
            return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
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
            ViewBag.SubjectNames = subjectRepository.GetName(Subjects);

            var AcademicRecords = academicRecordsRepository.GetStudent(id);

            ViewData["AcademicRecords"] = AcademicRecords;

            return View(studentSubjects);
        }

        [HttpPost]
        public IActionResult SaveAllDegrees(int subjectId, Dictionary<int, int> Degrees)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var Doctor = doctorRepository.GetAll().FirstOrDefault(h => h.ApplicationUserId == userId);

            foreach (var studentDegree in Degrees)
            {
                var studentId = studentDegree.Key;
                var degree = studentDegree.Value;


                // Fetch academic records for the student
                var academicRecords = academicRecordsRepository.GetStudent(studentId);
                var currentLevel = academicRecordsRepository.GetStudent(studentId)?.Level;

                // Fetch student subject entry and update it
                var studentSubject = studentSubjectsRepository.GetOne(studentId, subjectId);
                studentSubject.Degree = degree;
                studentSubject.Grade = studentSubjectsRepository.CalculateGrade(degree);

                studentSubjectsRepository.Update(studentSubject);
                studentSubjectsRepository.Save();

                // Update Academic Records
                UpdateAcademicRecords(studentId);

                var creditHours = studentSubjectsRepository.CalculateCreditHours(studentId);
                var semester = studentSubjectsRepository.Calculatesemester(creditHours);

                var gpaSemester = academicRecordsRepository.CalculateGpaSemester(studentId, semester);
                var gpaTotal = academicRecordsRepository.CalculateGPATotal(studentId);
                // Update GPA information
                if (academicRecords != null)
                {
                    academicRecords.GPASemester = gpaSemester;
                    academicRecords.GPATotal = gpaTotal;
                    academicRecordsRepository.Update(academicRecords);
                    academicRecordsRepository.Save();
                    if (academicRecords.Level != currentLevel)
                    {
                        var student = studentRepository.GetOne(studentId);
                        if (student != null)
                        {
                            student.PaymentFees = !student.PaymentFees;
                            studentRepository.Update(student);
                            studentRepository.Save();
                        }
                    }
                }
            }
            return RedirectToAction("DisplaySubject", "Doctor", new { id = @Doctor.Id });
        }
        public IActionResult StudentSubjectRegistered(int id)
        {
            ViewData["SubjectName"] = subjectRepository.GetName(id);
            ViewData["Level"] = subjectRepository.GetLevel(id);
            ViewData["Semester"] = subjectRepository.GetSemester(id);
            ViewData["id"] = id;

            var students = studentRepository.StudentsBySubject(id);
            var studentDegree = studentRepository.ReturnDegrees(students, id);
            var studentGrade = studentRepository.ReturnGrades(students, id);

            ViewData["StudentDegree"] = studentDegree;
            ViewData["StudentGrade"] = studentGrade;
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
