using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace HelwanUniversity.Areas.User.Controllers
{
    [Area("User")]
    public class UniFileController : Controller
    {
        private readonly IUniFileRepository uniFileRepository;

        public UniFileController(IUniFileRepository uniFileRepository)
        {
            this.uniFileRepository = uniFileRepository;
        }

        //Display Video News
        public IActionResult News()
        {
            var videos = uniFileRepository.GetAllVideos();
            return View(videos);
        }
    }
}
