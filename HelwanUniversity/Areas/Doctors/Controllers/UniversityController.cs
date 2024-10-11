using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class UniversityController : Controller
    {
        private readonly IUniversityRepository universityRepository;
        private readonly IUniFileRepository uniFileRepository;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IFacultyRepository facultyRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IDepartmentRepository departmentRepository;
        public UniversityController(IUniversityRepository universityRepository, IUniFileRepository uniFileRepository, 
            IHighBoardRepository highBoardRepository,IFacultyRepository facultyRepository,IDoctorRepository doctorRepository,
            IStudentRepository studentRepository,IDepartmentRepository departmentRepository)
        {
            this.universityRepository = universityRepository;
            this.uniFileRepository = uniFileRepository;
            this.highBoardRepository = highBoardRepository;
            this.facultyRepository = facultyRepository;
            this.doctorRepository = doctorRepository;
            this.studentRepository = studentRepository;
            this.departmentRepository = departmentRepository;
        }
        public IActionResult Index()
        {
            var UNI = universityRepository.Get();
            var Images = uniFileRepository.GetAllImages();
            var Hboards = highBoardRepository.GetAll();


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var Highboard = highBoardRepository.GetAll().FirstOrDefault(h => h.ApplicationUserId == userId);
            var Doctor = doctorRepository.GetAll().FirstOrDefault(h=>h.ApplicationUserId == userId);
            if (Doctor != null)
            {
                ViewData["Doctor"] = Doctor;
            }
            else
            {
                ViewData["Doctor"] = Highboard;
                if(Highboard != null && Highboard.JobTitle == Models.Enums.JobTitle.HeadOfDepartment)
                {
                    ViewData["Department"] = departmentRepository.GetDepartbyHead(Highboard.Id);
                }
            }
           //ViewData
            ViewData["LogoTitle"] = Images[0].File;
            ViewData["Images"] = Images;
            ViewData["Mail"] = $"mailto:{UNI.ContactMail}";

            ViewData["President"] = Hboards.FirstOrDefault(a => a.JobTitle == Models.Enums.JobTitle.President);
            ViewData["VicePresidents"] = Hboards.Where(a => a.JobTitle == Models.Enums.JobTitle.VicePrecident).ToList();
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
