using Data;
using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Enums;
using System;
using System.Security.Claims;
using ViewModels;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly IDoctorRepository doctorRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IActivityLogger _logger;

        public DoctorController(IDoctorRepository doctorRepository,
            ISubjectRepository subjectRepository, IDepartmentRepository departmentRepository, IHttpContextAccessor httpContextAccessor,
            IDepartmentSubjectsRepository departmentSubjectsRepository, ICloudinaryService cloudinaryService, IActivityLogger logger)
        {
            this.doctorRepository = doctorRepository;
            this.subjectRepository = subjectRepository;
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.cloudinaryService = cloudinaryService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);

            if (entity is not Doctor doctor || doctor.Id != id)
            {
                return Forbid();
            }

            return View(doctor);
        }
        public async Task<IActionResult> DisplaySubject(int id)
        {
            var subjects = subjectRepository.SubjectsByDoctor(id).ToList();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);
            if (entity == null)
            {
                return Forbid();
            }
            if (entity is Doctor doctor)
            {
                int doctorId = doctor.Id;
                var course = await doctorRepository.GetCourseForDoctorAsync(doctorId, id);
                if (course == null)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }
            var subjectIds = subjectRepository.GetIds(subjects);
            var departmentSubjects = departmentSubjectsRepository.GetDepartmentSubjects(subjectIds);
            var departmentDictionary = departmentRepository.Dict();
            ViewBag.SubjectDepartments = departmentSubjectsRepository.GetDepartmentsSubject(subjects,departmentSubjects);
            ViewBag.DepartmentDictionary = departmentDictionary;

            return View(subjects);
        }
        [HttpGet]
        public async Task<IActionResult> ChangePicture(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await doctorRepository.GetEntityByUserIdAsync(userId);

            if (entity is not Doctor doctor || doctor.Id != id)
            {
                return Forbid();
            }
            var ModelVM = new Picture()
            {
                Id = id,
                MainPicture = doctor.Picture
            };

            return View(ModelVM);
        }
        [HttpPost]
        public async Task<IActionResult> SaveChange(Picture ModelVM)
        {
            var doctor = doctorRepository.GetOne(ModelVM.Id);

            try
            {
                ModelVM.MainPicture = await cloudinaryService.UploadFile(
                    ModelVM.MainPictureFile,
                    doctor.Picture,
                    "An error occurred while uploading the Picture. Please try again."
                );
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                _logger.Log(
                    actionType: "Update",
                    tableName: "Doctors",
                    recordId: doctor.Id,
                    description: "Failed to update profile picture due to Cloudinary error.",
                    userId: doctor.Id,
                    userName: doctor.Name,
                    userRole: UserRole.Doctor
                );

                return View("ChangePicture", ModelVM);
            }

            if (!string.IsNullOrEmpty(ModelVM.MainPicture))
            {
                doctor.Picture = ModelVM.MainPicture;
                doctorRepository.Update(doctor);
                doctorRepository.Save();

                _logger.Log(
                    actionType: "Update",
                    tableName: "Doctors",
                    recordId: doctor.Id,
                    description: "Doctor updated their profile picture successfully.",
                    userId: doctor.Id,
                    userName: doctor.Name,
                    userRole: UserRole.Doctor
                );

                return RedirectToAction("Details", new { id = doctor.Id });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Picture upload failed unexpectedly.");

                _logger.Log(
                    actionType: "Update",
                    tableName: "Doctors",
                    recordId: doctor.Id,
                    description: "Upload returned empty picture URL despite no exception. Possibly a silent failure.",
                    userId: doctor.Id,
                    userName: doctor.Name,
                    userRole: UserRole.Doctor
                );

                return View("ChangePicture", ModelVM);
            }
        }
    }
}
