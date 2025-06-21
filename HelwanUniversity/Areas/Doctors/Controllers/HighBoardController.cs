using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Models;
using Models.Enums;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Numerics;
using System.Security.Claims;
using ViewModels;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class HighBoardController : Controller
    {
        private readonly IHighBoardRepository highBoardRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IActivityLogger _logger;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IFacultyRepository facultyRepository;  

        public HighBoardController(IHighBoardRepository highBoardRepository, ICloudinaryService cloudinaryService, IActivityLogger logger
            ,IDepartmentRepository departmentRepository,IFacultyRepository facultyRepository)
        {
            this.highBoardRepository = highBoardRepository;
            this.cloudinaryService = cloudinaryService;
            this._logger = logger;
            this.departmentRepository = departmentRepository;
            this.facultyRepository = facultyRepository;
        }
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await highBoardRepository.GetEntityByUserIdAsync(userId);

            if (entity is not HighBoard highBoard || highBoard.Id != id)
            {
                return Forbid();
            }
            var highboard = highBoardRepository.GetOne(id);
            return View(highboard);
        }
        [HttpGet]
        public async Task<IActionResult> ChangePicture(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await highBoardRepository.GetEntityByUserIdAsync(userId);

            if (entity is not HighBoard highBoard || highBoard.Id != id)
            {
                return Forbid();
            }
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = await highBoardRepository.GetEntityByUserIdAsync(userId);

            if (entity is not HighBoard highBoard || highBoard.Id != ModelVM.Id)
            {
                return Forbid();
            }
            var HB = highBoardRepository.GetOne(ModelVM.Id);

            string positionDetails = HB.JobTitle switch
            {
                JobTitle.HeadOfDepartment => $" of {departmentRepository.GetDepartbyHead(HB.Id)?.Name}",
                JobTitle.DeanOfFaculty => $" of {facultyRepository.GetFacultybyDean(HB.Id)?.Name}",
                _ => ""
            };

            try
            {
                ModelVM.MainPicture = await cloudinaryService.UploadFile(
                    ModelVM.MainPictureFile,
                    HB.Picture,
                    "An error occurred while uploading the Picture. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                _logger.Log(
                    actionType: "Update",
                    tableName: "HighBoards",
                    recordId: HB.Id,
                    description: $"{HB.JobTitle}{positionDetails} failed to update profile picture due to Cloudinary error.",
                    userId: HB.Id,
                    userName: HB.Name,
                    userRole: UserRole.HighBoard
                );

                return View("ChangePicture", ModelVM);
            }
            if (!string.IsNullOrEmpty(ModelVM.MainPicture))
            {
                HB.Picture = ModelVM.MainPicture;
                highBoardRepository.Update(HB);
                highBoardRepository.Save();

                _logger.Log(
                      actionType: "Update",
                      tableName:  "HighBoards",
                      recordId: HB.Id,
                      description: $"{HB.JobTitle}{positionDetails} updated their profile picture successfully.",
                      userId: HB.Id,
                      userName: HB.Name,
                      userRole: UserRole.HighBoard
                );

                return RedirectToAction("Details", new { id = HB.Id });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Picture upload failed unexpectedly.");

                _logger.Log(
                   actionType: "Update",
                   tableName: "HighBoards",
                   recordId: HB.Id,
                   description: $"{HB.JobTitle}{positionDetails} upload returned empty picture URL despite no exception. Possibly a silent failure.",
                   userId: HB.Id,
                   userName: HB.Name,
                   userRole: UserRole.HighBoard
                );

                return View("ChangePicture", ModelVM);
            }
        }
    }
}
