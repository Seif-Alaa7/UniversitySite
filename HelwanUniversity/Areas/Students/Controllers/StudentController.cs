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
            var studentDatails = studentRepository.GetOne(id);

            var Images = uniFileRepository.GetAllImages();
            ViewData["LogoTitle"] = Images[0].File;
            return View(studentDatails);
        }
        [HttpGet]
        public IActionResult ChangePicture(int id)
        {
            var student = studentRepository.GetOne(id);
            var ModelVM = new Picture()
            {
                Id = id,
                MainPicture = student.Picture
            };

            return View(ModelVM);
        }
        [HttpPost]
        public async Task<IActionResult> SaveChange(Picture ModelVM)
        {
            var student = studentRepository.GetOne(ModelVM.Id);
            try
            {
                ModelVM.MainPicture = await cloudinaryService.UploadFile(ModelVM.MainPictureFile, student.Picture, "An error occurred while uploading the Picture. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("ChangePicture", ModelVM);
            }
            student.Picture = ModelVM.MainPicture;
            studentRepository.Update(student);
            studentRepository.Save();

            return RedirectToAction("Details", new { id = student.Id });
        }
    }
}
