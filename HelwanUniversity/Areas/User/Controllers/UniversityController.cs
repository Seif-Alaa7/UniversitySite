using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace HelwanUniversity.Areas.User.Controllers
{
    [Area("User")]
    public class UniversityController : Controller
    {
        private readonly IUniversityRepository universityRepository;
        private readonly IUniFileRepository uniFileRepository;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IFacultyRepository facultyRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IStudentRepository studentRepository;
        public UniversityController(IUniversityRepository universityRepository, IUniFileRepository uniFileRepository, IHighBoardRepository highBoardRepository,IFacultyRepository facultyRepository,IDoctorRepository doctorRepository,IStudentRepository studentRepository)
        {
            this.universityRepository = universityRepository;
            this.uniFileRepository = uniFileRepository;
            this.highBoardRepository = highBoardRepository;
            this.facultyRepository = facultyRepository;
            this.doctorRepository = doctorRepository;
            this.studentRepository = studentRepository;
        }
        public IActionResult Index()
        {
            var UNI = universityRepository.Get();
            var Images = uniFileRepository.GetAllImages();
            var Hboards = highBoardRepository.GetAll();

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
        public IActionResult DisplayMap()
        {
            var Imgs = uniFileRepository.GetAllImages();
            ViewData["MapImage"] = Imgs[1].File;

            return View();
        }
        public IActionResult Details()
        {
            var Images = uniFileRepository.GetAllImages();
            ViewData["LogoTitle"] = Images[0].File;

            var university = universityRepository.Get();
            return View(university);
        }
    }
}
