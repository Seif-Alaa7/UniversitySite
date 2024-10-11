using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class UniFileController : Controller
    {
        private readonly IUniFileRepository uniFileRepository;

        public UniFileController(IUniFileRepository uniFileRepository)
        {
            this.uniFileRepository = uniFileRepository;

        }
        public IActionResult News()
        {
            var videos = uniFileRepository.GetAllVideos();
            return View(videos);
        }
    }
}
