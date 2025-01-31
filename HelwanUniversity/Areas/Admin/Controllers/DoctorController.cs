using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public DoctorController(IDoctorRepository doctorRepository, ICloudinaryService cloudinaryService,
            ISubjectRepository subjectRepository, IDepartmentRepository departmentRepository,
            IDepartmentSubjectsRepository departmentSubjectsRepository)
        {
            this.doctorRepository = doctorRepository;
            this.cloudinaryService = cloudinaryService;
            this.subjectRepository = subjectRepository;
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
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
            var DoctorDatails = doctorRepository.GetOne(id);
            return View(DoctorDatails);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
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
            var doctor = doctorRepository.GetOne(doctorVM.Id);

            if(doctor.Name != doctorVM.Name)
            {
                var exist = doctorRepository.ExistName(doctorVM.Name);
                if(exist)
                {
                    ModelState.AddModelError("Name", "This Name is Already Exist");
                    return View("Edit",doctorVM);  
                }
            }

            try
            {
                doctorVM.Picture = await cloudinaryService.UploadFile(doctorVM.FormFile, doctor.Picture, "An error occurred while uploading the photo. Please try again.");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Edit", doctorVM);
            }
            doctor.Id = doctorVM.Id;
            doctor.Name = doctorVM.Name;
            doctor.Address = doctorVM.Address;
            doctor.JobTitle = doctorVM.JobTitle;
            doctor.Picture = doctorVM.Picture;
            doctor.Gender = doctorVM.Gender;
            doctor.Religion = doctorVM.Religion;

            doctorRepository.Update(doctor);
            doctorRepository.Save();

            return RedirectToAction("Details", new { id = doctor.Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var doctor = doctorRepository.GetOne(id);
            var UserId = doctor.ApplicationUserId;

            var subjects = subjectRepository.SubjectsByDoctor(id).ToList();

            if (subjects.Any())
            {
                ViewBag.Error = "You cannot delete this doctor because they are linked to subjects.";

                var Doctors = doctorRepository.GetAll();

                ViewBag.Subjects = doctorRepository.GetSubjects(Doctors);
                ViewBag.DoctorDepartments = doctorRepository.GetDepartments(Doctors);
                ViewBag.DoctorColleges = doctorRepository.GetColleges(Doctors);

                return View("Index",Doctors);
            }
            doctorRepository.Delete(id);
            doctorRepository.Save();

            doctorRepository.DeleteUser(UserId);
            doctorRepository.Save();

            TempData["SuccessMessage"] = "The Doctor has been successfully deleted.";

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
