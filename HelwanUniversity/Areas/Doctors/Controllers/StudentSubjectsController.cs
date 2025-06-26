using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models.Enums;
using Models;
using ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Data.Repository;
using HelwanUniversity.Services;
using System.Numerics;

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
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IActivityLogger _logger;
        private readonly IFacultyRepository facultyRepository;
        private readonly IAttendanceRecordRepository attendanceRecordRepository;


        public StudentSubjectsController(IStudentSubjectsRepository studentSubjectsRepository,
            IAcademicRecordsRepository academicRecordsRepository, ISubjectRepository subjectRepository, IStudentRepository studentRepository,
            IDoctorRepository doctorRepository, IDepartmentRepository departmentRepository, IHighBoardRepository highBoardRepository,
            IActivityLogger logger,IFacultyRepository facultyRepository,IAttendanceRecordRepository attendanceRecordRepository)
        {
            this.studentSubjectsRepository = studentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.subjectRepository = subjectRepository;
            this.studentRepository = studentRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
            this.highBoardRepository = highBoardRepository;
            this._logger = logger;  
            this.facultyRepository = facultyRepository; 
            this.attendanceRecordRepository = attendanceRecordRepository;   
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddSubject(int studentId, int subjectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var HB = highBoardRepository.GetByUserId(userId);
            var studentName = studentRepository.GetStudentName(studentId);
            var subjectName = subjectRepository.GetName(subjectId); 
            var departmentName = departmentRepository.DepartmentByStudent(studentId)?.Name;

            if (studentName == null || subjectName == null || departmentName == null)
                return NotFound();

            if (HB == null) return Forbid();


            string positionDetails = HB.JobTitle switch
            {
                JobTitle.HeadOfDepartment => $" of {departmentRepository.GetDepartbyHead(HB.Id)?.Name}",
                JobTitle.DeanOfFaculty => $" of {facultyRepository.GetFacultybyDean(HB.Id)?.Name}",
                _ => ""
            };

            var exists = studentSubjectsRepository.Exist(studentId, subjectId);
            if (exists)
            {
                TempData["ErrorMessage"] = "This subject is already registered.";

                _logger.Log(
                   actionType: "Enroll Subject",
                   tableName: "StudentSubject",
                   recordId: subjectId,
                   description: $"{HB.JobTitle}{positionDetails} Attempted to enroll for Student '{studentName}' in subject '{subjectName}' in Department of '{departmentName}', but it is already registered.",
                   userId: HB.Id,
                   userName: HB.Name,
                   userRole: UserRole.HighBoard
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
                 description: $"{HB.JobTitle}{positionDetails} Enrolled for student '{studentName}' in subject '{subjectName}' in Department of '{departmentName}' successfully.",
                 userId: HB.Id,
                 userName: HB.Name,
                 userRole: UserRole.HighBoard
            );


            return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
        }
        public IActionResult DeleteSubject(int studentId, int subjectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var HB = highBoardRepository.GetByUserId(userId);
            var subjectName = subjectRepository.GetName(subjectId);
            var department = departmentRepository.DepartmentByStudent(studentId);
            var departmentName = department?.Name ?? "Unknown";
            var studentName = studentRepository.GetStudentName(studentId);

            if (HB == null) return Forbid();

            string positionDetails = HB?.JobTitle switch
            {
                JobTitle.HeadOfDepartment => $" of {departmentRepository.GetDepartbyHead(HB.Id)?.Name}",
                JobTitle.DeanOfFaculty => $" of {facultyRepository.GetFacultybyDean(HB.Id)?.Name}",
                _ => ""
            };

            var linksCount = studentSubjectsRepository.FindStudent(studentId).Count();
            if (linksCount == 1)
            {
                TempData["ErrorMessage"] = "You cannot delete this subject as it's the only one registered. Removing it will delete the student record.";

                _logger.Log(
                    actionType: "Cancel Enrolling in Subject",
                    tableName: "StudentSubject",
                    recordId: subjectId,
                    description: $"{HB.JobTitle}{positionDetails} attempted to delete subject '{subjectName}' for student '{studentName}' in department '{departmentName}', but it is the only registered subject and cannot be removed.",
                    userId: HB.Id,
                    userName: HB.Name,
                    userRole: UserRole.HighBoard
                );

                return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
            }

            var link = studentSubjectsRepository.GetOne(studentId, subjectId);
            if (link == null)
            {
                TempData["ErrorMessage"] = "You cannot delete this subject because it is not registered.";

                _logger.Log(
                    actionType: "Cancel Enrolling in Subject",
                    tableName: "StudentSubject",
                    recordId: subjectId,
                    description: $"{HB.JobTitle}{positionDetails} attempted to delete subject '{subjectName}' for student '{studentName}' in department '{departmentName}', but it was never registered.",
                    userId: HB.Id,
                    userName: HB.Name,
                    userRole: UserRole.HighBoard
                );

                return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { Studentid = studentId });
            }

            studentSubjectsRepository.Delete(link);
            studentSubjectsRepository.Save();

            UpdateAcademicRecords(studentId);

            TempData["SuccessMessage"] = "Subject has been successfully deleted.";

            _logger.Log(
                actionType: "Cancel Enrolling in Subject",
                tableName: "StudentSubject",
                recordId: subjectId,
                description: $"{HB.JobTitle}{positionDetails} successfully deleted subject '{subjectName}' for student '{studentName}' in department '{departmentName}'.",
                userId: HB.Id,
                userName: HB.Name,
                userRole: UserRole.HighBoard
            );

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
        public async Task<IActionResult> DisplayDegrees(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await highBoardRepository.GetEntityByUserIdAsync(userId);
            if (entity is not HighBoard highBoard)
            {
                return Forbid();
            }
            var student = studentRepository.GetOne(id);

            if (student == null)
            {
                return NotFound();
            }

            if (highBoard.JobTitle == JobTitle.HeadOfDepartment)
            {
                if (student.DepartmentId != highBoard.Department.Id)
                {
                    return Forbid();
                }
            }
            else if (highBoard.JobTitle == JobTitle.DeanOfFaculty)
            {
                if (student.Department.Faculty.Id != highBoard.Faculty.Id)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }
            var studentSubjects = studentSubjectsRepository.FindStudent(id);

            var Subjects = subjectRepository.GetSubjects(id);
            ViewBag.SubjectNames = subjectRepository.GetName(Subjects);

            var AcademicRecords = academicRecordsRepository.GetStudent(id);

            ViewData["AcademicRecords"] = AcademicRecords;

            string positionDetails = highBoard.JobTitle switch
            {
                JobTitle.HeadOfDepartment => $" of {departmentRepository.GetDepartbyHead(highBoard.Id)?.Name}",
                JobTitle.DeanOfFaculty => $" of {facultyRepository.GetFacultybyDean(highBoard.Id)?.Name}",
                _ => ""
            };

            _logger.Log(
                     actionType: "Show Student Degrees",
                     tableName: "Student",
                     recordId: student.Id,
                     description: $"{highBoard.JobTitle}{positionDetails} viewed the degrees of student '{student.Name}'.",
                     userId: highBoard.Id,
                     userName: highBoard.Name,
                     userRole: UserRole.HighBoard
            );

            return View(studentSubjects);
        }

        [HttpPost]
        public IActionResult SaveAllDegrees(int subjectId, Dictionary<int, int> Degrees)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var Doctor = doctorRepository.GetAll().FirstOrDefault(h => h.ApplicationUserId == userId);
            var subjectName = subjectRepository.GetName(subjectId);

            foreach (var studentDegree in Degrees)
            {
                var studentId = studentDegree.Key;
                var degree = studentDegree.Value;

                var studentSubject = studentSubjectsRepository.GetOne(studentId, subjectId);

                // ✅ Check if degree didn't change
                if (studentSubject.Degree == degree)
                    continue;

                // Update degree and grade
                studentSubject.Degree = degree;
                studentSubject.Grade = studentSubjectsRepository.CalculateGrade(degree);

                studentSubjectsRepository.Update(studentSubject);
                studentSubjectsRepository.Save();

                // Update Academic Records
                var academicRecords = academicRecordsRepository.GetStudent(studentId);
                var currentLevel = academicRecords?.Level;

                UpdateAcademicRecords(studentId);

                var creditHours = studentSubjectsRepository.CalculateCreditHours(studentId);
                var semester = studentSubjectsRepository.Calculatesemester(creditHours);

                var gpaSemester = academicRecordsRepository.CalculateGpaSemester(studentId, semester);
                var gpaTotal = academicRecordsRepository.CalculateGPATotal(studentId);

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

                _logger.Log(
                    actionType: "Update Degree",
                    tableName: "StudentSubject",
                    recordId: studentSubject.StudentId,
                    description: $"Doctor Updated degree for student '{studentRepository.GetStudentName(studentId)}' in subject '{subjectName}' to {degree}.",
                    userId: Doctor.Id,
                    userName: Doctor.Name,
                    userRole: UserRole.Doctor
                );
            }

            return RedirectToAction("DisplaySubject", "Doctor", new { id = Doctor.Id });
        }
        public async Task<IActionResult> StudentSubjectRegistered(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctor = await doctorRepository.GetEntityByUserIdAsync(userId) as Doctor;

            if (doctor == null)
            {
                return Forbid();
            }

            var subject = subjectRepository.GetOne(id);
            if (subject == null || subject.DoctorId != doctor.Id)
            {
                return Forbid();
            }

            ViewData["SubjectName"] = subjectRepository.GetName(id);
            ViewData["Level"] = subjectRepository.GetLevel(id);
            ViewData["Semester"] = subjectRepository.GetSemester(id);
            ViewData["id"] = id;

            var students = studentRepository.StudentsBySubject(id);
            var studentDegree = studentRepository.ReturnDegrees(students, id);
            var studentGrade = studentRepository.ReturnGrades(students, id);

            ViewData["StudentDegree"] = studentDegree;
            ViewData["StudentGrade"] = studentGrade;

            ViewBag.AttendanceCount = attendanceRecordRepository.GetAttendanceCount(id);

            var (recommendedAttendance, correlation) = attendanceRecordRepository.RecommendAttendanceBasedOnAnalysis(id);

            if (recommendedAttendance > 0)
            {
                ViewBag.Recommendation = $"Based on data analysis (correlation = {correlation}), students are recommended to attend at least {recommendedAttendance} lectures to increase their chances of success.";
            }
            else
            {
                ViewBag.Recommendation = "No strong correlation found between attendance and grades, No strict attendance recommendation can be made based on current data.";
            }
            return View(students);
        }
        public async Task<IActionResult> AttendanceBySubject(int subjectId)
        {
            var attendence = await attendanceRecordRepository.GetAttendanceBySubjectIdAsync(subjectId);
            var lectureMap = attendanceRecordRepository.GetLectureNumbersBySubject(subjectId);

            ViewBag.DateToLectureMap = lectureMap;
            ViewBag.SubjectName = subjectRepository.GetName(subjectId);
            return View(attendence.OrderBy(a => a.AttendanceDate).ToList());
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
