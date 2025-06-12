using Data.Repository;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Enums;
using Newtonsoft.Json;
using Stripe.Checkout;
using System.Security.Claims;

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
        public StudentSubjectsController(IStudentSubjectsRepository studentSubjectsRepository, IAcademicRecordsRepository academicRecordsRepository,
            ISubjectRepository subjectRepository,
            IDoctorRepository doctorRepository,
            IDepartmentRepository departmentRepository,
            IUniFileRepository uniFileRepository,
            IStudentRepository studentRepository)
        {
            this.studentSubjectsRepository = studentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.subjectRepository = subjectRepository;
            this.doctorRepository = doctorRepository;
            this.departmentRepository = departmentRepository;
            this.uniFileRepository = uniFileRepository;
            this.studentRepository = studentRepository;
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
            return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { id = studentId });
        }
        public IActionResult DeleteSubject(int studentId, int subjectId)
        {
            var links = studentSubjectsRepository.FindStudent(studentId);
            if (links.Count() == 1)
            {
                TempData["ErrorMessage"] = "You cannot delete this subject as it's the only one registered. Removing it will delete the student record.";
                return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { id = studentId });
            }
            else
            {
                var link = studentSubjectsRepository.GetOne(studentId, subjectId);
                if (link == null)
                {
                    TempData["ErrorMessage"] = "you Can't Delete Subject because you Did not Add";
                    return RedirectToAction("DisplaySubjects", "DepartmentSubjects", new { id = studentId });
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
            var subjectsJson = TempData["CartItems"] as string;
            if (string.IsNullOrEmpty(subjectsJson))
            {
                return NotFound();
            }

            var subjects = JsonConvert.DeserializeObject<List<Subject>>(subjectsJson);


            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Students/checkout/success",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Students/checkout/cancel",
            };

            foreach (var model in subjects)
            {
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
