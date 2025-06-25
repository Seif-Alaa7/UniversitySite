using HelwanUniversity.Services;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Models.Enums;

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UniversityController : Controller
    {
        private readonly IUniversityRepository universityRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IUniFileRepository uniFileRepository;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IFacultyRepository facultyRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IActivityLogger _logger;

        public UniversityController(IUniversityRepository universityRepository, ICloudinaryService cloudinaryService, IUniFileRepository uniFileRepository, IHighBoardRepository highBoardRepository,IFacultyRepository facultyRepository,IDoctorRepository doctorRepository,IStudentRepository studentRepository
            ,IActivityLogger logger)
        {
            this.universityRepository = universityRepository;
            this.cloudinaryService = cloudinaryService;
            this.uniFileRepository = uniFileRepository;
            this.highBoardRepository = highBoardRepository;
            this.facultyRepository = facultyRepository;
            this.doctorRepository = doctorRepository;
            this.studentRepository = studentRepository;
            this._logger = logger;  
        }
        public IActionResult Index()
        {
            var UNI = universityRepository.Get();
            var Images = uniFileRepository.GetAllImages();
            var Hboards = highBoardRepository.GetAll();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var admin = highBoardRepository.GetAll().FirstOrDefault(h => h.ApplicationUserId == userId);
            if (admin != null)
            {
                ViewData["Admin"] = admin;
            };
            
            //ViewData
            ViewData["LogoTitle"] = Images[0].File;
            ViewData["Images"] = Images;
            ViewData["Mail"] = $"mailto:{UNI.ContactMail}";
            ViewData["President"]= Hboards.FirstOrDefault(a=>a.JobTitle == Models.Enums.JobTitle.President);
            ViewData["VicePresidents"]= Hboards.Where(a=>a.JobTitle == Models.Enums.JobTitle.VicePrecident).ToList();
            ViewData["VPAcademicAffairs"] = Hboards.FirstOrDefault(a => a.JobTitle == Models.Enums.JobTitle.VP_For_AcademicAffairs);

            //Counts
            ViewData["FacultyCounts"] = facultyRepository.GetAll().Count();
            ViewData["DoctorCounts"] = doctorRepository.GetAll().Count();
            ViewData["StudentCounts"] = studentRepository.GetAll().Count();

            UNI.ViewCount++;
            universityRepository.Save();    

            return View(UNI);       

        }
        [HttpGet]
        public IActionResult Update()
        {
            var university = universityRepository.Get();

            // Mapping
            var universityVM = new UniversityVM
            {
                Name = university.Name,
                Logo = university.Logo,
                MainPicture = university.MainPicture,
                Description = university.Description,
                FacebookPage = university.FacebookPage,
                LinkedInPage = university.LinkedInPage,
                MainPage = university.MainPage,
                ContactMail = university.ContactMail,
                HistoricalBackground = university.HistoricalBackground,
                GoogleForm = university.GoogleForm, 
                ViewCount = university.ViewCount,
            };

            var Imgs = uniFileRepository.GetAllImages();

            ViewData["ImgUpdate"] = Imgs[2].File;
            ViewData["LogoTitle"] = Imgs[0].File;

            return View(universityVM);

        }
        [HttpPost]
        public async Task<IActionResult> SaveUpdate(UniversityVM newUniVm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var uni = universityRepository.Get();
                try
                {
                    newUniVm.Logo = await cloudinaryService.UploadFile(newUniVm.LogoFile, uni.Logo, "An error occurred while uploading the logo. Please try again.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);


                _logger.Log(
                    actionType: "Update University Logo",
                    tableName: "University",
                    recordId: uni.Id,
                    description: $"{admin.JobTitle} failed to update the university logo. Error: {ex.Message}",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("Update", newUniVm);
                }
                try
                {
                    newUniVm.MainPicture = await cloudinaryService.UploadFile(newUniVm.MainPictureFile, uni.MainPicture, "An error occurred while uploading the photo. Please try again.");

                }
            catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);

                _logger.Log(
                     actionType: "Update University Main Picture",
                     tableName: "University",
                     recordId: uni.Id,
                     description: $"{admin.JobTitle} failed to update the university main picture. Error: {ex.Message}",
                     userId: admin.Id,
                     userName: admin.Name,
                     userRole: UserRole.Admin
                 );

                return View("Update", newUniVm);

                }

            List<string> changes = new();
            if (uni.Name != newUniVm.Name)
                changes.Add($"Name changed to '{newUniVm.Name}'");

            if (uni.Logo != newUniVm.Logo)
                changes.Add("Logo updated");

            if (uni.MainPicture != newUniVm.MainPicture)
                changes.Add("Main Picture updated");

            if (uni.MainPage != newUniVm.MainPage)
                changes.Add("Main Website updated");


            uni.Name = newUniVm.Name;
                uni.Logo = newUniVm.Logo;
                uni.MainPicture = newUniVm.MainPicture;
                uni.Description = newUniVm.Description;
                uni.FacebookPage = newUniVm.FacebookPage;
                uni.LinkedInPage = newUniVm.LinkedInPage;
                uni.MainPage = newUniVm.MainPage;
                uni.ContactMail = newUniVm.ContactMail;
                uni.HistoricalBackground = newUniVm.HistoricalBackground;
                uni.GoogleForm = newUniVm.GoogleForm;
                uni.ViewCount = newUniVm.ViewCount;

                universityRepository.Update(uni);
                universityRepository.Save();


            if (changes.Count > 0)
            {
                _logger.Log(
                    actionType: "Update University Details",
                    tableName: "University",
                    recordId: uni.Id,
                    description: $"{admin.JobTitle} updated university details: {string.Join(", ", changes)}.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );
            }

            return RedirectToAction("Index");
        }
        public IActionResult DisplayMap()
        {
            var Imgs = uniFileRepository.GetAllImages();

            ViewData["MapImage"] = Imgs[1].File;
            ViewData["LogoTitle"] = Imgs[0].File;

            return View();
        }
        public IActionResult Details()
        {
            var Images = uniFileRepository.GetAllImages();
            ViewData["LogoTitle"] = Images[0].File;
            ViewData["LogoTitle"] = Images[0].File;

            var university = universityRepository.Get();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var admin = highBoardRepository.GetAll().FirstOrDefault(h => h.ApplicationUserId == userId);
            if (admin != null)
            {
                ViewBag.Admin = admin;
            };

            return View(university);
        }
    }
}
