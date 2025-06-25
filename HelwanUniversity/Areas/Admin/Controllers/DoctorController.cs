using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Enums;
using System.Security.Claims;
using ViewModels;

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DoctorController : Controller
    {
        private readonly IDoctorRepository doctorRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly ISubjectRepository subjectRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IActivityLogger _logger;


        public DoctorController(IDoctorRepository doctorRepository, ICloudinaryService cloudinaryService,
            ISubjectRepository subjectRepository, IDepartmentRepository departmentRepository,
            IDepartmentSubjectsRepository departmentSubjectsRepository, IActivityLogger logger, IHighBoardRepository highBoardRepository)
        {
            this.doctorRepository = doctorRepository;
            this.cloudinaryService = cloudinaryService;
            this.subjectRepository = subjectRepository;
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            _logger = logger;
            this.highBoardRepository = highBoardRepository;
        }
        public IActionResult Index()
        {
            var Doctors = doctorRepository.GetAll();

            ViewBag.Subjects = doctorRepository.GetSubjects(Doctors);
            ViewBag.DoctorDepartments = doctorRepository.GetDepartments(Doctors);
            ViewBag.DoctorColleges = doctorRepository.GetColleges(Doctors);

            return View(Doctors);
        }
        public IActionResult Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var DoctorDatails = doctorRepository.GetOne(id);

            _logger.Log(
             actionType: "Show Doctor Details",
             tableName: "Doctor",
             recordId: DoctorDatails.Id,
             description: $"{admin.JobTitle} viewed the profile of Doctor '{DoctorDatails.Name}'.",
             userId: admin.Id,
             userName: admin.Name,
             userRole: UserRole.Admin
            );

            return View(DoctorDatails);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var doctor = doctorRepository.GetOne(id);
            var doctorVM = new DoctorVM
            {
                Id = id,
                Name = doctor.Name,
                Address = doctor.Address,
                JobTitle = doctor.JobTitle,
                Picture = doctor.Picture,
                Gender = doctor.Gender,
                Religion = doctor.Religion
            };

            return View(doctorVM);
        }
        [HttpPost]
        public async Task<IActionResult> SaveEdit(DoctorVM doctorVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var doctor = doctorRepository.GetOne(doctorVM.Id);
            if (doctor == null)
                return NotFound();

            var sensitiveChanges = new List<string>();

            if (doctor.Name != doctorVM.Name)
                sensitiveChanges.Add($"Name changed from '{doctor.Name}' to '{doctorVM.Name}'");

            if (doctor.JobTitle != doctorVM.JobTitle)
                sensitiveChanges.Add($"Job Title changed from '{doctor.JobTitle}' to '{doctorVM.JobTitle}'");

            string newPicture = doctor.Picture;
            try
            {
                newPicture = await cloudinaryService.UploadFile(doctorVM.FormFile, doctor.Picture, "An error occurred while uploading the photo. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                _logger.Log(
                    actionType: "Update Doctor Picture",
                    tableName: "Doctor",
                    recordId: doctor.Id,
                    description: $"{admin.JobTitle} failed to update Doctor '{doctor.Name}' photo due to error: {ex.Message}",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View(doctorVM);
            }

            if (doctor.Picture != newPicture)
                sensitiveChanges.Add("Profile picture has been updated");

            // Update doctor details
            doctor.Name = doctorVM.Name;
            doctor.JobTitle = doctorVM.JobTitle;
            doctor.Gender = doctorVM.Gender;
            doctor.Religion = doctorVM.Religion;
            doctor.Address = doctorVM.Address;
            doctor.Picture = newPicture;

            doctorRepository.Update(doctor);
            doctorRepository.Save();

            if (sensitiveChanges.Any())
            {
                _logger.Log(
                    actionType: "Update Doctor Details",
                    tableName: "Doctor",
                    recordId: doctor.Id,
                    description: $"{admin.JobTitle} updated Doctor '{doctor.Name}' sensitive details: {string.Join(", ", sensitiveChanges)}.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );
            }
            else
            {
                _logger.Log(
                    actionType: "Update Doctor Details",
                    tableName: "Doctor",
                    recordId: doctor.Id,
                    description: $"{admin.JobTitle} updated Doctor '{doctor.Name}' details",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );
            }

            TempData["SuccessMessageDoctor"] = "Doctor details updated successfully.";
            return RedirectToAction("Details", new { id = doctor.Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var doctor = doctorRepository.GetOne(id);
            if (doctor == null)
                return NotFound();

            var userIdToDelete = doctor.ApplicationUserId;
            var subjects = subjectRepository.SubjectsByDoctor(id).ToList();

            if (subjects.Any())
            {
                ViewBag.Error = "You cannot delete this doctor because they are linked to subjects.";

                var doctors = doctorRepository.GetAll();
                ViewBag.Subjects = doctorRepository.GetSubjects(doctors);
                ViewBag.DoctorDepartments = doctorRepository.GetDepartments(doctors);
                ViewBag.DoctorColleges = doctorRepository.GetColleges(doctors);

                _logger.Log(
                    actionType: "Delete Doctor",
                    tableName: "Doctor",
                    recordId: doctor.Id,
                    description: $"{admin.JobTitle} attempted to delete Doctor '{doctor.Name}' but they are linked to subjects.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("Index", doctors);
            }

            doctorRepository.Delete(id);
            doctorRepository.Save();

            if (!string.IsNullOrEmpty(userIdToDelete))
            {
                doctorRepository.DeleteUser(userIdToDelete);
                doctorRepository.Save();
            }

            TempData["SuccessMessage"] = "The Doctor has been successfully deleted.";

            _logger.Log(
                actionType: "Delete Doctor",
                tableName: "Doctor",
                recordId: doctor.Id,
                description: $"{admin.JobTitle} permanently deleted Doctor '{doctor.Name}' and their associated user account.",
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.Admin
            );

            return RedirectToAction("Index");
        }
        public IActionResult DisplaySubject(int id)
        {
            var subjects = subjectRepository.SubjectsByDoctor(id).ToList();

            if (subjects == null || !subjects.Any())
            {
                ViewBag.Message = "There are No Subjects For this Doctor";
                return View();
            }

            var subjectIds = subjectRepository.GetIds(subjects);
            var departmentSubjects = departmentSubjectsRepository.GetDepartmentSubjects(subjectIds);
            var departmentDictionary = departmentRepository.Dict();
            ViewBag.SubjectDepartments = departmentSubjectsRepository.GetDepartmentsSubject(subjects,departmentSubjects);
            ViewBag.DepartmentDictionary = departmentDictionary;

            return View(subjects);
        }
    }
}
