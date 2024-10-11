using Data;
using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Security.Claims;
using ViewModels.FacultyVMs;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class FacultyController : Controller
    {
        private readonly IFacultyRepository facultyRepository;
        private readonly IHighBoardRepository highBoardRepository;
        public FacultyController(IFacultyRepository facultyRepository,
            IHighBoardRepository highBoardRepository)
        {
            this.facultyRepository = facultyRepository;
            this.highBoardRepository = highBoardRepository;
        }
        public IActionResult Index()
        {
            var faculties = facultyRepository.GetAll().ToList();
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
