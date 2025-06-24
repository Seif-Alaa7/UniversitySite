using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Enums;
using System.Security.Claims;
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
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IFacultyRepository facultyRepository;  
        private readonly IActivityLogger _logger;

        public StudentSubjectsController(IStudentSubjectsRepository studentSubjectsRepository,
            IAcademicRecordsRepository academicRecordsRepository, ISubjectRepository subjectRepository, IStudentRepository studentRepository,
            IDoctorRepository doctorRepository, IDepartmentRepository departmentRepository,IActivityLogger logger, IHighBoardRepository highBoardRepository
            ,IFacultyRepository facultyRepository)
        {
            this.studentSubjectsRepository = studentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.subjectRepository = subjectRepository;
            this.studentRepository = studentRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
            this._logger = logger;
            this.facultyRepository = facultyRepository;
            this.highBoardRepository = highBoardRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddSubject(int studentId, int subjectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var student = studentRepository.GetOne(studentId);  
            var subject =  subjectRepository.GetOne(subjectId); 
            var department = departmentRepository.DepartmentByStudent(studentId);

            if (student == null || subject == null || department == null)
                return NotFound();

            var exists = studentSubjectsRepository.Exist(studentId, subjectId);
            if (exists)
            {
                TempData["ErrorMessage"] = "This subject is already registered.";


                _logger.Log(
                   actionType: "Enroll Subject",
                   tableName: "StudentSubject",
                   recordId: subjectId,
                   description: $"{admin.JobTitle} attempted to enroll student '{student.Name}' in subject '{subject.Name}' in department '{department.Name}', but it is already registered.",
                   userId: admin.Id,
                   userName: admin.Name,
                   userRole: UserRole.Admin
                );

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

            TempData["SuccessMessage"] = "Subject has been successfully added.";


            _logger.Log(
                 actionType: "Enroll Subject",
                 tableName: "StudentSubject",
                 recordId: subjectId,
                 description: $"{admin.JobTitle} Enrolled for student '{student.Name}' in subject '{subject.Name}' in Department of '{department.Name}' successfully.",
                 userId: admin.Id,
                 userName: admin.Name,
                 userRole: UserRole.Admin
            );

            return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
        }
        public IActionResult DeleteSubject(int studentId, int subjectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var subjectName = subjectRepository.GetName(subjectId);
            var departmentName = departmentRepository.DepartmentByStudent(studentId)?.Name;
            var studentName = studentRepository.GetStudentName(studentId);

            var link = studentSubjectsRepository.GetOne(studentId, subjectId);
            if (link == null)
            {
                TempData["ErrorMessage"] = "You can't delete this subject because it is not registered.";

                _logger.Log(
                    actionType: "Cancel Enrolling in Subject",
                    tableName: "StudentSubject",
                    recordId: subjectId,
                    description: $"{admin.JobTitle} attempted to delete subject '{subjectName}' for student '{studentName}' in Department '{departmentName}', but it was never registered.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
            }

            var linksCount = studentSubjectsRepository.FindStudent(studentId).Count();
            if (linksCount == 1)
            {
                TempData["ErrorMessage"] = "You cannot delete this subject as it's the only one registered. Removing it will delete the student record.";

                _logger.Log(
                    actionType: "Cancel Enrolling in Subject",
                    tableName: "StudentSubject",
                    recordId: subjectId,
                    description: $"{admin.JobTitle} attempted to delete subject '{subjectName}' for student '{studentName}' in Department '{departmentName}', but it is the only registered subject and cannot be removed.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
            }

            studentSubjectsRepository.Delete(link);
            studentSubjectsRepository.Save();

            // Update Academic Records
            UpdateAcademicRecords(studentId);

            TempData["SuccessMessage"] = "Subject has been successfully deleted.";

            _logger.Log(
                actionType: "Cancel Enrolling in Subject",
                tableName: "StudentSubject",
                recordId: subjectId,
                description: $"{admin.JobTitle} successfully deleted subject '{subjectName}' for student '{studentName}' in Department '{departmentName}'.",
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.Admin
            );

            return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
        }
        public IActionResult DisplayDegrees(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var student = studentRepository.GetOne(id);
            var studentSubjects = studentSubjectsRepository.FindStudent(id);
            var Subjects = subjectRepository.GetSubjects(id);
            var AcademicRecords = academicRecordsRepository.GetStudent(id);

            ViewBag.SubjectNames = subjectRepository.GetName(Subjects);
            ViewData["AcademicRecords"] = AcademicRecords;
            ViewBag.StudentId = id;

            _logger.Log(
              actionType: "Show Student Degrees",
              tableName: "Student",
              recordId: student.Id,
              description: $"{admin.JobTitle} viewed the degrees of student '{student.Name}'.",
              userId: admin.Id,
              userName: admin.Name,
              userRole: UserRole.Admin
            );

            return View(studentSubjects);
        }
        [HttpPost]
        public IActionResult SaveAllDegrees(int studentId, Dictionary<int, int> Degrees)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var academicRecords = academicRecordsRepository.GetStudent(studentId);
            var oldLevel = academicRecords?.Level;

            foreach (var subjectDegree in Degrees)
            {
                var subjectId = subjectDegree.Key;
                var degree = subjectDegree.Value;

                var studentSubject = studentSubjectsRepository.GetOne(studentId, subjectId);
                if (studentSubject != null)
                {
                    studentSubject.Degree = degree;
                    studentSubject.Grade = studentSubjectsRepository.CalculateGrade(degree);
                    studentSubjectsRepository.Update(studentSubject);
                }
            }

            studentSubjectsRepository.Save();

            // Update Academic Records
            UpdateAcademicRecords(studentId);

            var creditHours = studentSubjectsRepository.CalculateCreditHours(studentId);
            var semester = studentSubjectsRepository.Calculatesemester(creditHours);
            var gpaSemester = academicRecordsRepository.CalculateGpaSemester(studentId, semester);
            var gpaTotal = academicRecordsRepository.CalculateGPATotal(studentId);

            var student = studentRepository.GetOne(studentId);
            var department = departmentRepository.DepartmentByStudent(studentId);
            var faculty = facultyRepository.FacultyByDepartment(department.Id);

            string description = $"{admin.JobTitle} updated all degrees for student '{student.Name}' in department '{department.Name}' , faculty '{faculty.Name}'.";

            if (academicRecords != null)
            {
                academicRecords.GPASemester = gpaSemester;
                academicRecords.GPATotal = gpaTotal;

                academicRecordsRepository.Update(academicRecords);
                academicRecordsRepository.Save();

                if (academicRecords.Level != oldLevel)
                {
                    if (student != null)
                    {
                        student.PaymentFees = !student.PaymentFees;
                        studentRepository.Update(student);
                        studentRepository.Save();

                        description += $" Student level updated from '{oldLevel}' to '{academicRecords.Level}', Payment Fees set to '{(student.PaymentFees ? "Paid" : "Not Paid")}'.";
                    }
                }
                else
                {
                    description += " GPA updated, no level change.";
                }
            }

            _logger.Log(
                actionType: "Update Degrees",
                tableName: "StudentSubject",
                recordId: student.Id,
                description: description,
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.Admin
            );

            return RedirectToAction("Index", "Student");
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
