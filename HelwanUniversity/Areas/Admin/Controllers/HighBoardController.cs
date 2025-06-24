using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Models;
using Models.Enums;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Claims;
using ViewModels;

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HighBoardController : Controller
    {
        private readonly IHighBoardRepository highBoardRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IFacultyRepository facultyRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly IActivityLogger _logger;
        private readonly UserManager<IdentityUser> _userManager;


        public HighBoardController(IHighBoardRepository highBoardRepository,
            ICloudinaryService cloudinaryService,IFacultyRepository facultyRepository,IDepartmentRepository departmentRepository
            ,IDepartmentSubjectsRepository departmentSubjectsRepository
            , UserManager<IdentityUser> userManager,IActivityLogger logger)
        {
            this.highBoardRepository = highBoardRepository;
            this.cloudinaryService = cloudinaryService;
            this.facultyRepository = facultyRepository;
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this._userManager = userManager;
            this._logger = logger;  
        }
        public IActionResult Index()
        {
            var Highboards = highBoardRepository.GetAll();
            ViewData["President"] = highBoardRepository.GetPresident();
            return View(Highboards);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var highboard = highBoardRepository.GetOne(id);
            var highboardVM = new HighBoardVM
            {
                Id = id,
                Name = highboard.Name,
                Description = highboard.Description,
                JobTitle = highboard.JobTitle,
                Picture = highboard.Picture
            };
            return View(highboardVM);
        }
        [HttpPost]
        public async Task<IActionResult> SaveEdit(HighBoardVM highBoardVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var highboard = highBoardRepository.GetOne(highBoardVM.Id);
            if (highboard == null)
                return NotFound();

            var sensitiveChanges = new List<string>();

            if (highboard.Name != highBoardVM.Name)
            {
                var exist = highBoardRepository.ExistName(highBoardVM.Name);
                if (exist)
                {
                    ModelState.AddModelError("Name", "This Name already exists");
                    highBoardVM.Picture = highboard.Picture;

                    _logger.Log(
                        actionType: "Update Highboard Name",
                        tableName: "Highboard",
                        recordId: highboard.Id,
                        description: $"{admin.JobTitle} attempted to change name of Highboard '{highboard.Name}' to '{highBoardVM.Name}', but this name already exists.",
                        userId: admin.Id,
                        userName: admin.Name,
                        userRole: UserRole.Admin
                    );

                    return View("Edit", highBoardVM);
                }
                sensitiveChanges.Add($"Name changed from '{highboard.Name}' to '{highBoardVM.Name}'");
            }

            if (highboard.JobTitle != highBoardVM.JobTitle)
            {
                if (highBoardVM.JobTitle == Models.Enums.JobTitle.VP_For_AcademicAffairs
                    || highBoardVM.JobTitle == Models.Enums.JobTitle.VP_For_Finance
                    || highBoardVM.JobTitle == Models.Enums.JobTitle.President)
                {
                    var exist = highBoardRepository.ExistJop(highBoardVM.JobTitle);
                    if (exist)
                    {
                        ModelState.AddModelError("JobTitle", "This Job Title already exists");
                        highBoardVM.Picture = highboard.Picture;

                        _logger.Log(
                            actionType: "Update Highboard Job Title",
                            tableName: "Highboard",
                            recordId: highboard.Id,
                            description: $"{admin.JobTitle} attempted to change Job Title of Highboard '{highboard.Name}' to '{highBoardVM.JobTitle}', but this Job Title already exists.",
                            userId: admin.Id,
                            userName: admin.Name,
                            userRole: UserRole.Admin
                        );

                        return View("Edit", highBoardVM);
                    }
                }
                else if (highBoardVM.JobTitle == Models.Enums.JobTitle.VicePrecident
                    && highboard.JobTitle == Models.Enums.JobTitle.DeanOfFaculty)
                {
                    highboard.JobTitle = Models.Enums.JobTitle.VicePrecident;

                    var user = await _userManager.FindByIdAsync(highboard.ApplicationUserId);
                    if (user != null)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Doctor"))
                        {
                            await _userManager.RemoveFromRoleAsync(user, "Doctor");
                        }

                        if (!await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            await _userManager.AddToRoleAsync(user, "Admin");
                        }
                        await _userManager.UpdateAsync(user);
                    }
                }
                sensitiveChanges.Add($"Job Title changed from '{highboard.JobTitle}' to '{highBoardVM.JobTitle}'");
            }

            string newPicture = highboard.Picture;
            try
            {
                newPicture = await cloudinaryService.UploadFile(highBoardVM.FormFile, highboard.Picture, "An error occurred while uploading the photo. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                highBoardVM.Picture = highboard.Picture;

                _logger.Log(
                    actionType: "Update Highboard Picture",
                    tableName: "Highboard",
                    recordId: highboard.Id,
                    description: $"{admin.JobTitle} failed to update picture of Highboard '{highboard.Name}' due to error: {ex.Message}.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("Edit", highBoardVM);
            }

            if (highboard.Picture != newPicture)
                sensitiveChanges.Add("Profile picture has been updated");

            highboard.Description = highBoardVM.Description;
            highboard.Name = highBoardVM.Name;
            highboard.JobTitle = highBoardVM.JobTitle;
            highboard.Picture = newPicture;

            highBoardRepository.Update(highboard);
            highBoardRepository.Save();

            if (sensitiveChanges.Any())
            {
                _logger.Log(
                    actionType: "Update Highboard Details",
                    tableName: "Highboard",
                    recordId: highboard.Id,
                    description: $"{admin.JobTitle} updated Highboard details: {string.Join(", ", sensitiveChanges)}.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );
            }
            else
            {
                _logger.Log(
                    actionType: "Update Highboard Details",
                    tableName: "Highboard",
                    recordId: highboard.Id,
                    description: $"{admin.JobTitle} updated Highboard '{highboard.Name} details'",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );
            }

            if (highboard.JobTitle == Models.Enums.JobTitle.DeanOfFaculty)
                return RedirectToAction("DisplayDean");
            else if (highboard.JobTitle == Models.Enums.JobTitle.HeadOfDepartment)
                return RedirectToAction("DisplayHead");
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(UserId);
            if (admin == null)
                return Forbid();

            var highBoard = highBoardRepository.GetOne(id);
            if (highBoard == null)
                return NotFound();

            var applicationUserId = highBoard.ApplicationUserId;
            var job = highBoard.JobTitle;

            if (job == Models.Enums.JobTitle.DeanOfFaculty)
            {
                var faculty = facultyRepository.GetFacultybyDean(id);
                if (faculty != null)
                {
                    var departments = facultyRepository.GetDepartments(faculty.Id);
                    if (departments.Count > 0)
                    {
                        ViewBag.Faculty = facultyRepository.GetFaculty(facultyRepository.GetAll());
                        ViewBag.Error = "Cannot delete the dean. The faculty has linked departments.";

                        _logger.Log(
                            actionType: "Delete Highboard",
                            tableName: "Highboard",
                            recordId: highBoard.Id,
                            description: $"{admin.JobTitle} attempted to delete Dean '{highBoard.Name}', but the faculty '{faculty.Name}' has linked departments.",
                            userId: admin.Id,
                            userName: admin.Name,
                            userRole: UserRole.Admin
                        );

                        return View("DisplayDean", highBoardRepository.GetDeans());
                    }

                    ViewBag.Faculty = facultyRepository.GetFaculty(facultyRepository.GetAll());
                    ViewBag.Error = "Cannot delete the dean. The faculty is linked.";

                    _logger.Log(
                        actionType: "Delete Highboard",
                        tableName: "Highboard",
                        recordId: highBoard.Id,
                        description: $"{admin.JobTitle} attempted to delete Dean '{highBoard.Name}', but the faculty '{faculty.Name}' is linked.",
                        userId: admin.Id,
                        userName: admin.Name,
                        userRole: UserRole.Admin
                    );

                    return View("DisplayDean", highBoardRepository.GetDeans());
                }
            }
            else if (job == Models.Enums.JobTitle.HeadOfDepartment)
            {
                var department = departmentRepository.GetDepartbyHead(id);
                if (department != null)
                {
                    ViewBag.Department = departmentRepository.GetDepartments(departmentRepository.GetAll());
                    ViewBag.Error = "Cannot delete the department head. The department is linked.";

                    _logger.Log(
                        actionType: "Delete Highboard",
                        tableName: "Highboard",
                        recordId: highBoard.Id,
                        description: $"{admin.JobTitle} attempted to delete Head of Department '{highBoard.Name}', but the department '{department.Name}' is linked.",
                        userId: admin.Id,
                        userName: admin.Name,
                        userRole: UserRole.Admin
                    );

                    return View("DisplayHead", highBoardRepository.GetHeads());
                }
            }

            highBoardRepository.Delete(id);
            highBoardRepository.Save();

            highBoardRepository.DeleteUser(applicationUserId);
            highBoardRepository.Save();

            TempData["SuccessMessage"] = "The member has been successfully deleted.";

            _logger.Log(
                actionType: "Delete Highboard",
                tableName: "Highboard",
                recordId: highBoard.Id,
                description: $"{admin.JobTitle} permanently deleted Highboard '{highBoard.Name}' with Job Title '{highBoard.JobTitle}'.",
                userId: admin.Id,
                userName: admin.Name,
                userRole: UserRole.Admin
            );

            return job == Models.Enums.JobTitle.DeanOfFaculty ? RedirectToAction("DisplayDean") :
                   job == Models.Enums.JobTitle.HeadOfDepartment ? RedirectToAction("DisplayHead") :
                   RedirectToAction("Index");
        }
        public IActionResult DisplayDean()
        {
            var deans = highBoardRepository.GetDeans();
            ViewBag.Faculty = facultyRepository.GetFaculty(facultyRepository.GetAll());
            return View(deans);
        }

        public IActionResult DisplayHead()
        {
            var heads = highBoardRepository.GetHeads();
            ViewBag.Department = departmentRepository.GetDepartments(departmentRepository.GetAll());
            return View(heads);
        }
    }
}
