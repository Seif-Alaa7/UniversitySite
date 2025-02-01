using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.FacultyVMs;

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FacultyController : Controller
    {
        private readonly IFacultyRepository facultyRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IUniFileRepository uniFileRepository;
        private readonly IDepartmentRepository departmentRepository;

        public FacultyController(IFacultyRepository facultyRepository,ICloudinaryService cloudinaryService,
            IHighBoardRepository highBoardRepository, IUniFileRepository uniFileRepository,IDepartmentRepository departmentRepository)
        {
            this.facultyRepository = facultyRepository;
            this.cloudinaryService = cloudinaryService;
            this.highBoardRepository = highBoardRepository;
            this.departmentRepository = departmentRepository;
            this.uniFileRepository = uniFileRepository;
        }
        public IActionResult Index()
        {
            var faculties =facultyRepository.GetAll().ToList();
            return View(faculties);
        }
        public IActionResult Details(int id)
        {
            var faculty = facultyRepository.GetOne(id);
            if (faculty == null)
            {
                return NotFound();
            }
            faculty.ViewCount++;
            facultyRepository.Save();

            ViewData["Dean"] = highBoardRepository.GetOne(faculty.DeanId)?.Name;

            return View(faculty);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var Faculty = facultyRepository.GetOne(id);
            var FacultyVM = new FacultyVm
            {
                DeanId = Faculty.DeanId,
                Id = Faculty.Id,
                Description = Faculty.Description,
                Logo = Faculty.Logo,
                Name = Faculty.Name,
                Picture = Faculty.Picture,
                ViewCount = Faculty.ViewCount
            };
            var imgs = uniFileRepository.GetAllImages();
            ViewData["iMGUpdate"] = imgs[2].File;
            ViewData["Deans"] = highBoardRepository.selectDeans();

            return View(FacultyVM);
        }
        [HttpPost]
        public async Task<IActionResult> SaveEdit(FacultyVm facultyvm)
        {
            var Faculty = facultyRepository.GetOne(facultyvm.Id);

            if(facultyvm.Name != Faculty.Name)
            {
                var Faculities = facultyRepository.GetAll();
                var NameExists = Faculities.Any(a=>a.Name == facultyvm.Name);
                if(NameExists)
                {
                    ModelState.AddModelError("Name", "The faculty name already exists.");
                    return View("Edit", facultyvm);

                }
            }
            if(facultyvm.DeanId != Faculty.DeanId)
            {
                if (facultyRepository.ExistDeanInFaculty(facultyvm.DeanId))
                {
                    ModelState.AddModelError("DeanId", "This person is already a Dean of a registered Faculty.");
                    return View("Edit", facultyvm);
                }
            }
            try
            {
                facultyvm.Logo = await cloudinaryService.UploadFile(facultyvm.LogoFile,Faculty.Logo, "An error occurred while uploading the logo. Please try again.");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Edit", facultyvm);
            }
            try
            {
                facultyvm.Picture = await cloudinaryService.UploadFile(facultyvm.PictureFile, Faculty.Picture, "An error occurred while uploading the logo. Please try again.");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Edit", facultyvm);
            }
            Faculty.Description = facultyvm.Description;
            Faculty.DeanId = facultyvm.DeanId;
            Faculty.Logo = facultyvm.Logo;
            Faculty.Name = facultyvm.Name;
            Faculty.Picture = facultyvm.Picture;
            Faculty.ViewCount = facultyvm.ViewCount;

            facultyRepository.Update(Faculty);
            facultyRepository.Save();
            return RedirectToAction("Details", "Faculty", new {id = Faculty.Id});
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Faculty faculty)
        {
            facultyRepository.Delete(faculty);
            facultyRepository.Save();

            TempData["SuccessMessage"] = "The Faculty has been successfully deleted.";

            return RedirectToAction("Index");
        }
        public IActionResult AllFaculities()
        {
            var faculties = facultyRepository.GetAll().ToList();
            ViewBag.DepartmentsByFaculty = departmentRepository.GetDepartmentsByFaculty(faculties);
            return View(faculties);
        }
    }
}
