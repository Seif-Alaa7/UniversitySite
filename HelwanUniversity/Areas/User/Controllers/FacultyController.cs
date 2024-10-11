using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace HelwanUniversity.Areas.User.Controllers
{
    [Area("User")]
    public class FacultyController : Controller
    {
        private readonly IFacultyRepository facultyRepository;
        private readonly IHighBoardRepository highBoardRepository;

        public FacultyController(IFacultyRepository facultyRepository, IHighBoardRepository highBoardRepository)
        {
            this.facultyRepository = facultyRepository;
            this.highBoardRepository = highBoardRepository;
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
    }
}
