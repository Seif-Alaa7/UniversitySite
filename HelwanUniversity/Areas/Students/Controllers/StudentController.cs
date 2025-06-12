using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using ViewModels;

namespace HelwanUniversity.Areas.Students.Controllers
{
    [Area("Students")]
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IStudentRepository studentRepository;
        private readonly IFacultyRepository faculty;
        private readonly IUniversityRepository universityRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IUniFileRepository uniFileRepository;

        
        public StudentController(IStudentRepository studentRepository
            ,IFacultyRepository faculty, IUniversityRepository universityRepository 
            , ICloudinaryService cloudinaryService,IUniFileRepository uniFileRepository)
        {
            this.studentRepository = studentRepository;
            this.faculty = faculty;
            this.universityRepository = universityRepository;
            this.cloudinaryService = cloudinaryService;
            this.uniFileRepository = uniFileRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentStudent = studentRepository.GetByUserId(userId);

            if (currentStudent == null || currentStudent.Id != id)
                return Forbid();

            var studentDatails = studentRepository.GetOne(id);
            if (studentDatails == null)
                return NotFound();
            var Images = uniFileRepository.GetAllImages();
            ViewData["LogoTitle"] = Images[0].File;
            return View(studentDatails);
        }
        [HttpGet]
        public IActionResult ChangePicture(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentStudent = studentRepository.GetByUserId(userId);

            if (currentStudent == null || currentStudent.Id != id)
                return Forbid();
            var ModelVM = new Picture()
            {
                Id = id,
                MainPicture = currentStudent.Picture
            };

            return View(ModelVM);
        }
        [HttpPost]
        public async Task<IActionResult> SaveChange(Picture ModelVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentStudent = studentRepository.GetByUserId(userId);

            if (currentStudent == null || currentStudent.Id != ModelVM.Id)
                return Forbid();
            try
            {
                ModelVM.MainPicture = await cloudinaryService.UploadFile(ModelVM.MainPictureFile, currentStudent.Picture, "An error occurred while uploading the Picture. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("ChangePicture", ModelVM);
            }
            currentStudent.Picture = ModelVM.MainPicture;
            studentRepository.Update(currentStudent);
            studentRepository.Save();

            return RedirectToAction("Details", new { id = currentStudent.Id });
        }
    }
}
