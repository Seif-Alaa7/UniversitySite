using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelwanUniversity.Areas.Students.Controllers
{
    [Area("Students")]
    [Authorize(Roles = "Student")]
    public class UniFileController : Controller
    {
        private readonly IUniFileRepository uniFileRepository;

        public UniFileController(IUniFileRepository uniFileRepository)
        {
            this.uniFileRepository = uniFileRepository;

        }
        //Display News
        public IActionResult News()
        {
            var videos = uniFileRepository.GetAllVideos();
            return View(videos);
        }
    }
}
