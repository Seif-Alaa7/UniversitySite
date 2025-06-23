using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Enums;
using Newtonsoft.Json;
using Stripe.Checkout;
using System.Security.Claims;
using ViewModels;

namespace HelwanUniversity.Areas.Students.Controllers
{
    [Area("Students")]
    [Authorize(Roles = "Student")]
    public class StudentSubjectsController : Controller
    {
        private readonly IStudentSubjectsRepository studentSubjectsRepository;
        private readonly IAcademicRecordsRepository academicRecordsRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IUniFileRepository uniFileRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IActivityLogger _logger;

        public StudentSubjectsController(IStudentSubjectsRepository studentSubjectsRepository, IAcademicRecordsRepository academicRecordsRepository,
            ISubjectRepository subjectRepository,
            IDoctorRepository doctorRepository,
            IDepartmentRepository departmentRepository,
            IUniFileRepository uniFileRepository,
            IStudentRepository studentRepository,
            IActivityLogger logger)
        {
            this.studentSubjectsRepository = studentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.subjectRepository = subjectRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
            this.uniFileRepository = uniFileRepository;
            this.studentRepository = studentRepository;
            this._logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddSubject(int studentId, int subjectId)
        {
            var subjectName = subjectRepository.GetName(subjectId);
            var departmentName = departmentRepository.DepartmentByStudent(studentId)?.Name;
            var exists = studentSubjectsRepository.Exist(studentId, subjectId);
            if (exists)
            {
                TempData["ErrorMessage"] = "This subject is already registered.";

                _logger.Log(
                   actionType: "Enroll Subject",
                   tableName: "StudentSubject",
                   recordId: subjectId,
                   description: $"Attempted to enroll in subject '{subjectName}' in Department of '{departmentName}', but it is already registered.",
                   userId: studentId,
                   userName: studentRepository.GetStudentName(studentId),
                   userRole: UserRole.Student
                );

                return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { id = studentId });
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

            _logger.Log(
               actionType: "Enroll Subject",
               tableName: "StudentSubject",
               recordId: subjectId,
               description: $"Enrolled in subject '{subjectName}' in Department of '{departmentName}' successfully.",
               userId: studentId,
               userName: studentRepository.GetStudentName(studentId),
               userRole: UserRole.Student
            );

            return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { id = studentId });
        }
        public IActionResult DeleteSubject(int studentId, int subjectId)
        {
            var subjectName = subjectRepository.GetName(subjectId);
            var departmentName = departmentRepository.DepartmentByStudent(studentId)?.Name;

            var links = studentSubjectsRepository.FindStudent(studentId);
            if (links.Count() == 1)
            {
                TempData["ErrorMessage"] = "You cannot delete this subject as it's the only one registered. Removing it will delete the student record.";

                _logger.Log(
                  actionType:"Cancel Enrolling in Subject",
                  tableName: "StudentSubject",
                  recordId: subjectId,
                  description: $"Attempted to delete subject '{subjectName}' in Department of '{departmentName}', but it is the only registered subject and cannot be removed.",
                  userId: studentId,
                  userName: studentRepository.GetStudentName(studentId),
                  userRole: UserRole.Student
               );

                return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { id = studentId });
            }
            else
            {
                var link = studentSubjectsRepository.GetOne(studentId, subjectId);
                if (link == null)
                {
                    TempData["ErrorMessage"] = "you Can't Delete Subject because you Did not Add";

                    _logger.Log(
                        actionType: "Cancel Enrolling in Subject",
                        tableName: "StudentSubject",
                        recordId: subjectId,
                        description: $"Attempted to delete subject '{subjectName}' in Department of '{departmentName}', but it was never registered.",
                        userId: studentId,
                        userName: studentRepository.GetStudentName(studentId),
                        userRole: UserRole.Student
                    );

                    return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { id = studentId });
                }
                else
                {
                    studentSubjectsRepository.Delete(link);
                    studentSubjectsRepository.Save();

                    // Update Academic Records
                    UpdateAcademicRecords(studentId);

                    TempData["Success"] = "Subject has been successfully Deleted.";

                    _logger.Log(
                        actionType: "Cancel Enrolling in Subject",
                        tableName: "StudentSubject",
                        recordId: subjectId,
                        description: $"Successfully deleted subject '{subjectName}' in Department of '{departmentName}'.",
                        userId: studentId,
                        userName: studentRepository.GetStudentName(studentId),
                        userRole: UserRole.Student
                    );
                }
            }
            return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { id = studentId });
        }
        public IActionResult SubjectRegsitered(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentStudent = studentRepository.GetByUserId(userId);

            if (currentStudent == null || currentStudent.Id != id)
                return Forbid();
            var Images = uniFileRepository.GetAllImages();
            var Subjects = subjectRepository.GetSubjects(id);
            ViewData["LogoTitle"] = Images[0].File;
            ViewBag.TotalSalary = Subjects.Select(x=>x.Salary).Sum();   

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            TempData["CartItems"] = JsonConvert.SerializeObject(Subjects,settings); 
            return View(Subjects);
        }
        public IActionResult Pay()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentStudent = studentRepository.GetByUserId(userId);

            var subjectsJson = TempData["CartItems"] as string;
            if (string.IsNullOrEmpty(subjectsJson))
            {
                return NotFound();
            }

            var subjects = JsonConvert.DeserializeObject<List<Subject>>(subjectsJson);
            if (subjects == null || !subjects.Any())
            {
                return NotFound("Subjects list is empty.");
            }
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Students/checkout/success",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Students/checkout/cancel",
            };

            long totalAmount = 0;

            foreach (var model in subjects)
            {
                var amount = (long)model.Salary * 100;
                totalAmount += amount;

                var line = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = model.Name,
                            Description = model.subjectType.ToString(),
                        },
                        UnitAmount = (long)model.Salary*100,
                    },
                    Quantity = 1,
                };
                options.LineItems.Add(line);
            }

            var service = new SessionService();
            var session = service.Create(options);

            _logger.Log(
                   actionType: "Start Payment",
                   tableName: "Student",
                   recordId: null, 
                   description: $"Initiated payment for {subjects.Count} subjects. Total: {totalAmount / 100.0:F2} USD.",
                   userId: currentStudent.Id,
                   userName: currentStudent.Name,
                   userRole: UserRole.Student
            );

            return Redirect(session.Url);   
        }
        public IActionResult DisplayDegrees(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentStudent = studentRepository.GetByUserId(userId);

            if (currentStudent == null || currentStudent.Id != id)
                return Forbid();

            var studentSubjects = studentSubjectsRepository.FindStudent(id);
            var Images = uniFileRepository.GetAllImages();
            var Subjects = subjectRepository.GetSubjects(id);
            ViewBag.SubjectNames = subjectRepository.GetName(Subjects);


            var AcademicRecords = academicRecordsRepository.GetStudent(id);
            ViewBag.StudentId = id;
            ViewData["AcademicRecords"] = AcademicRecords;
            ViewData["LogoTitle"] = Images[0].File;

            return View(studentSubjects);
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
