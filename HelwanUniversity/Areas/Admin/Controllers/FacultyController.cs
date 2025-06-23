using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Models;
using Models.Enums;
using System;
using System.Security.Claims;
using ViewModels.FacultyVMs;

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FacultyController : Controller
    {
        private readonly IFacultyRepository facultyRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IUniFileRepository uniFileRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IActivityLogger _logger;

        public FacultyController(IFacultyRepository facultyRepository,ICloudinaryService cloudinaryService,
            IHighBoardRepository highBoardRepository, IUniFileRepository uniFileRepository,IDepartmentRepository departmentRepository,IActivityLogger logger)
        {
            this.facultyRepository = facultyRepository;
            this.cloudinaryService = cloudinaryService;
            this.highBoardRepository = highBoardRepository;
            this.departmentRepository = departmentRepository;
            this.uniFileRepository = uniFileRepository;
            this._logger = logger;
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
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var Faculty = facultyRepository.GetOne(id);
            var FacultyVM = new FacultyVm
            {
                DeanId = Faculty.DeanId,
                Id = Faculty.Id,
                Description = Faculty.Description,
                Logo = Faculty.Logo,
                Name = Faculty.Name,
                Picture = Faculty.Picture,
                ViewCount = Faculty.ViewCount
            };
            var imgs = uniFileRepository.GetAllImages();
            ViewData["iMGUpdate"] = imgs[2].File;
            ViewData["Deans"] = highBoardRepository.selectDeans();

            return View(FacultyVM);
        }
        [HttpPost]
        public async Task<IActionResult> SaveEdit(FacultyVm facultyvm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var faculty = facultyRepository.GetOne(facultyvm.Id);

            if (facultyvm.Name != faculty.Name && facultyRepository.GetAll().Any(f => f.Name == facultyvm.Name))
            {
                ModelState.AddModelError("Name", "The faculty name already exists.");

                _logger.Log(
                    actionType: "Update Faculty Name",
                    tableName: "Faculty",
                    recordId: faculty.Id,
                    description: $"{admin.JobTitle} failed to update the name of faculty '{faculty.Name}' to '{facultyvm.Name}' as the name already exists.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("Edit", facultyvm);
            }

            if (facultyvm.DeanId != faculty.DeanId && facultyRepository.ExistDeanInFaculty(facultyvm.DeanId))
            {
                ModelState.AddModelError("DeanId", "This person is already assigned as Dean of another faculty.");

                _logger.Log(
                    actionType: "Update Faculty Dean",
                    tableName: "Faculty",
                    recordId: faculty.Id,
                    description: $"{admin.JobTitle} failed to change the Dean of faculty '{faculty.Name}' as the selected person is already Dean elsewhere.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("Edit", facultyvm);
            }

            try
            {
                facultyvm.Logo = await cloudinaryService.UploadFile(facultyvm.LogoFile, faculty.Logo, "An error occurred while uploading the logo. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                _logger.Log(
                    actionType: "Update Faculty Logo",
                    tableName: "Faculty",
                    recordId: faculty.Id,
                    description: $"{admin.JobTitle} failed to update the logo for faculty '{faculty.Name}'. Error: {ex.Message}",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("Edit", facultyvm);
            }

            try
            {
                facultyvm.Picture = await cloudinaryService.UploadFile(facultyvm.PictureFile, faculty.Picture, "An error occurred while uploading the picture. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                _logger.Log(
                    actionType: "Update Faculty Picture",
                    tableName: "Faculty",
                    recordId: faculty.Id,
                    description: $"{admin.JobTitle} failed to update the picture for faculty '{faculty.Name}'. Error: {ex.Message}",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );

                return View("Edit", facultyvm);
            }

            List<string> changes = new();

            if (faculty.Name != facultyvm.Name)
                changes.Add($"Name changed to '{facultyvm.Name}'");

            if (faculty.DeanId != facultyvm.DeanId)
            {
                var oldDeanName = highBoardRepository.GetDeanByFaculty(faculty.Id)?.Name ?? "N/A";
                var newDeanName = highBoardRepository.GetDeanByFaculty(facultyvm.DeanId)?.Name ?? "N/A";
                changes.Add($"Dean changed from '{oldDeanName}' to '{newDeanName}'");
            }

            if (faculty.Logo != facultyvm.Logo)
                changes.Add("Logo updated");

            if (faculty.Picture != facultyvm.Picture)
                changes.Add("Picture updated");

            faculty.Name = facultyvm.Name;
            faculty.DeanId = facultyvm.DeanId;
            faculty.Description = facultyvm.Description;
            faculty.Logo = facultyvm.Logo;
            faculty.Picture = facultyvm.Picture;
            faculty.ViewCount = facultyvm.ViewCount;

            facultyRepository.Update(faculty);
            facultyRepository.Save();

            if (changes.Count > 0)
            {
                _logger.Log(
                    actionType: "Update Faculty Details",
                    tableName: "Faculty",
                    recordId: faculty.Id,
                    description: $"{admin.JobTitle} updated faculty '{faculty.Name}' details: {string.Join(", ", changes)}.",
                    userId: admin.Id,
                    userName: admin.Name,
                    userRole: UserRole.Admin
                );
            }

            return RedirectToAction("Details", "Faculty", new { id = faculty.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Faculty faculty)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            var departments = departmentRepository.GetDepartmentsByCollegeId(faculty.Id);
            if (!departments.Any())
            {
                facultyRepository.Delete(faculty);
                facultyRepository.Save();

                TempData["SuccessMessage"] = "The Faculty has been successfully deleted.";

                _logger.Log(
                         actionType: "Delete Faculty",
                         tableName: "Faculty",
                         recordId: faculty.Id,
                         description: $"{admin.JobTitle} successfully deleted faculty of '{faculty.Name}'.",
                         userId: admin.Id,
                         userName: admin.Name,
                         userRole: UserRole.Admin
                );
            }
            else
            {
                TempData["ErrorMessage"] = "Deletion is not allowed as the faculty is still associated with departments.";

                var Departments = departmentRepository.GetDepartmentsByCollegeId(faculty.Id);
                _logger.Log(
                       actionType: "Delete Faculty",
                       tableName: "Faculty",
                       recordId: faculty.Id,
                       description: $"{admin.JobTitle} failed to delete faculty of '{faculty.Name}' as it is still associated with the following departments: {string.Join(", ", departments.Select(d => d.Name))}",
                       userId: admin.Id,
                       userName: admin.Name,
                       userRole: UserRole.Admin
                );
            }
            return RedirectToAction("Index");
        }
        public IActionResult AllFaculities()
        {
            var faculties = facultyRepository.GetAll().ToList();
            ViewBag.DepartmentsByFaculty = departmentRepository.GetDepartmentsByFaculty(faculties);
            return View(faculties);
        }
    }
}
