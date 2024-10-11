using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Models;
using System.Collections.Generic;
using System.Net.WebSockets;
using ViewModels;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class HighBoardController : Controller
    {
        private readonly IHighBoardRepository highBoardRepository;
        private readonly ICloudinaryService cloudinaryService;
        

        public HighBoardController(IHighBoardRepository highBoardRepository, ICloudinaryService cloudinaryService)
        {
            this.highBoardRepository = highBoardRepository;
            this.cloudinaryService = cloudinaryService;
        }
        public IActionResult Details(int id)
        {
            var highboard = highBoardRepository.GetOne(id);
            return View(highboard);
        }
        [HttpGet]
        public IActionResult ChangePicture(int id)
        {
            var doctor = highBoardRepository.GetOne(id);
            var ModelVM = new Picture()
            {
                Id = id,
                MainPicture = doctor.Picture
            };

            return View(ModelVM);
        }
        [HttpPost]
        public async Task<IActionResult> SaveChange(Picture ModelVM)
        {
            var HB = highBoardRepository.GetOne(ModelVM.Id);
            try
            {
                ModelVM.MainPicture = await cloudinaryService.UploadFile(ModelVM.MainPictureFile, HB.Picture, "An error occurred while uploading the Picture. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("ChangePicture", ModelVM);
            }
            HB.Picture = ModelVM.MainPicture;
            
            highBoardRepository.Update(HB);
            highBoardRepository.Save();

            return RedirectToAction("Details", new { id = HB.Id });
        }
    }
}
