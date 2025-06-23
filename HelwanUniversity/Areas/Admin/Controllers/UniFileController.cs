using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.UniFileVMs;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Data.Repository;
using ViewModels;
using Models.Enums;
using System.Drawing;

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UniFileController : Controller
    {
        private readonly IUniFileRepository uniFileRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IActivityLogger _logger;
        private readonly IHighBoardRepository highBoardRepository;

        public UniFileController(IUniFileRepository uniFileRepository, ICloudinaryService cloudinaryService, IActivityLogger logger, IHighBoardRepository highBoardRepository)
        {
            this.uniFileRepository = uniFileRepository;
            this.cloudinaryService = cloudinaryService;
            _logger = logger;
            this.highBoardRepository = highBoardRepository;
        }

        //Display image & Video
        public IActionResult News()
        {
            var videos = uniFileRepository.GetAllVideos();
            return View(videos);
        }
        public IActionResult EmbededLink()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AddVideo()
        {
            var Videovm = new UniFileVM()
            {
                File = "",
                ContentType = Models.Enums.Filetype.Video
            };
            return View(Videovm);
        }
        [HttpPost]
        public IActionResult SaveVideo(UniFileVM uniFileVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(uniFileVM.File) || !uniFileVM.File.StartsWith("https://www.facebook.com/"))
            {
                ModelState.AddModelError("File", "The video link must be a valid Facebook link starting with 'https://www.facebook.com/'");
            }

            if (ModelState.IsValid)
            {
                var file = new UniFile
                {
                    File = uniFileVM.File,
                    ContentType = uniFileVM.ContentType,
                };
                uniFileRepository.Add(file);
                uniFileRepository.Save();

                _logger.Log(
                    actionType: "Add Video",
                    tableName: "UniFiles",
                    recordId: file.Id,
                    description: $"{admin.JobTitle} has successfully added a new Video to the News.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return RedirectToAction("News");
            }
            var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

            _logger.Log(
                actionType: "Add Video",
                tableName: "UniFiles",
                recordId: 0,
                description: $"{admin.JobTitle} failed to add a new video to the News due to invalid form data. Errors: {string.Join(" | ", errors)}",
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.Admin
            );

            return View("AddVideo", uniFileVM);
        }
        [HttpGet]
        public IActionResult AddImage()
        {
            var imgvm = new UniFileVM()
            {
                File = Empty.ToString(),
                ContentType = Filetype.IMG
            };

            return View(imgvm);
        }
        [HttpPost]
        public async Task<IActionResult> SaveImg(UniFileVM uniFileVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            try
            {
                uniFileVM.File = await cloudinaryService.UploadFile(uniFileVM.ImgPath, string.Empty, "An error occurred while uploading the photo. Please try again.");

                var file = new UniFile
                {
                    File = uniFileVM.File,
                    ContentType = uniFileVM.ContentType,
                };

                uniFileRepository.Add(file);
                uniFileRepository.Save();

                _logger.Log(
                    actionType: "Add Image",
                    tableName: "UniFiles",
                    recordId: file.Id,
                    description: $"{admin.JobTitle} has successfully added a new image to the slide images.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return RedirectToAction("DisplayImages");
            }
            catch (Exception ex)
            {
                _logger.Log(
                    actionType: "Add Image",
                    tableName: "UniFiles",
                    recordId: 0,
                    description: $"{admin.JobTitle} failed to add a new image to the slide images. Error: {ex.Message}",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                ModelState.AddModelError(string.Empty, ex.Message);
                return View("AddImage", uniFileVM);
            }
        }

        [HttpGet]
        public IActionResult UpdateVideo(int id)
        {
            var Video = uniFileRepository.GetFile(id);

            if (Video == null)
            {
                return NotFound(); 
            }

            // Mapping
            var VideoVM = new UniFileVM2
            {
                Id = Video.Id,
                File = Video.File,
                ContentType = Video.ContentType
            };

            return View(VideoVM);
        }

        [HttpPost]
        public IActionResult SaveUpdateVideo(UniFileVM2 newVideoVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var video = uniFileRepository.GetFile(newVideoVM.Id);

            if (string.IsNullOrWhiteSpace(newVideoVM.File) || !newVideoVM.File.StartsWith("https://www.facebook.com/"))
            {
                ModelState.AddModelError("File", "The video link must be a valid Facebook link starting with 'https://www.facebook.com/'");

                _logger.Log(
                    actionType: "Update Video",
                    tableName: "UniFiles",
                    recordId: video.Id,
                    description: $"{admin.JobTitle} failed to update the video file due to invalid Facebook link.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("UpdateVideo", newVideoVM);
            }

            if (newVideoVM.File != video.File)
            {
                var existVideo = uniFileRepository.ExistVideo(newVideoVM.File);
                if (existVideo)
                {
                    ModelState.AddModelError("File", "This file is already Registered");

                    _logger.Log(
                          actionType: "Update Video",
                          tableName: "UniFiles",
                          recordId: video.Id,
                          description: $"{admin.JobTitle} failed to update the video file. The same video file is already registered.",
                          userId: admin.Id,
                          userName: admin.Name,
                          userRole: UserRole.Admin
                    );

                    return View("UpdateVideo", newVideoVM);

                }
            }
            //Update Changes
            video.File = newVideoVM.File;
            uniFileRepository.Update(video);
            uniFileRepository.Save();

            _logger.Log(
                     actionType: "Update Video",
                     tableName: "UniFiles",
                     recordId: video.Id,
                     description: $"{admin.JobTitle} successfully updated the Facebook video link.",
                     userId: admin.Id,
                     userName: admin.Name,
                     userRole: UserRole.Admin
            );

            return RedirectToAction("News");
        }
        [HttpGet]
        public IActionResult UpdateImage(int id)
        {
            var Img = uniFileRepository.GetFile(id);
            if (Img == null)
            {
                return NotFound();
            }
            // Mapping
            var ImgVM = new UniFileVM2
            {
                Id = Img.Id,
                File = Img.File,
                ContentType = Img.ContentType
            };

            return View(ImgVM);
        }
        [HttpPost]
        public async Task<IActionResult> SaveUpdate(UniFileVM2 newImgVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var IMG = uniFileRepository.GetFile(newImgVM.Id);
            if (IMG == null)
                return NotFound();

            var imgs = uniFileRepository.GetAllImages().ToList();

            var index = imgs.FindIndex(img => img.Id == IMG.Id);
            if (index == -1)
                return NotFound();

            string imgRole = index switch
            {
                0 => " of Title Logo",
                1 => " of Internal Map",
                2 => " of Update Form",
                3 => " of Signin & Slide",
                4 => " of Forgot Password & Slide",
                _ => " of Slide"
            };

            try
            {
                    newImgVM.File = await cloudinaryService.UploadFile(newImgVM.ImgPath, IMG.File, "An error occurred while uploading the photo. Please try again.");

                    //Update Change
                    IMG.File = newImgVM.File;
                    uniFileRepository.Update(IMG);
                    uniFileRepository.Save();

                _logger.Log(
                      actionType: $"Update Image",
                      tableName: "UniFiles",
                      recordId: IMG.Id,
                      description: $"{admin.JobTitle} has updated the Image file{imgRole} successfully.",
                      userId: admin.Id,
                      userName: admin.Name,
                      userRole: UserRole.Admin
                );

                return RedirectToAction("DisplayImages");
            }
            catch (Exception ex)
            {
                    ModelState.AddModelError(string.Empty, ex.Message);

                _logger.Log(
                   actionType: "Update Image",
                   tableName: "UniFiles",
                   recordId: IMG.Id,
                   description: $"{admin.JobTitle} Failed to Update Image{imgRole}. Error: {ex.Message}",
                   userId: admin.Id,
                   userName: admin.Name,
                   userRole: UserRole.Admin
                );

                return View("UpdateImage", newImgVM);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var file = uniFileRepository.GetFile(id);
            if (file == null)
                return NotFound();

            string fileTypeText = file.ContentType switch
            {
                Filetype.IMG => " Image",
                Filetype.Video => " Video",
                _ => ""
            };
            string article = fileTypeText.Trim().StartsWith(" I") ? "an" : "a";
            string imgRole = string.Empty;

            if (file.ContentType == Filetype.IMG)
            {
                var imgs = uniFileRepository.GetAllImages().ToList();
                var index = imgs.FindIndex(img => img.Id == file.Id);

                if (index != -1)
                {
                    imgRole = index switch
                    {
                        0 => " of Title Logo",
                        1 => " of Internal Map",
                        2 => " of Update Form",
                        3 => " of Signin & Slide",
                        4 => " of Forgot Password & Slide",
                        _ => " of Slide"
                    };
                }
            }

            uniFileRepository.Delete(file);
            uniFileRepository.Save();

            _logger.Log(
                actionType: $"Delete{fileTypeText}",
                tableName: "UniFiles",
                recordId: file.Id,
                description: $"{admin.JobTitle} has deleted {article}{fileTypeText} file{imgRole} successfully.",
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.Admin
            );

            return file.ContentType == Filetype.IMG
                ? RedirectToAction("DisplayImages")
                : RedirectToAction("News");
        }

        public IActionResult DisplayImages()
        {
            var Imgs = uniFileRepository.GetAllImages();
            return View(Imgs);
        }
    }
}
