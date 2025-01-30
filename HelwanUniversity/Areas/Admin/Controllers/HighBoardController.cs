using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Models;
using System.Collections.Generic;
using System.Net.WebSockets;
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

        public HighBoardController(IHighBoardRepository highBoardRepository,
            ICloudinaryService cloudinaryService,IFacultyRepository facultyRepository,IDepartmentRepository departmentRepository
            ,IDepartmentSubjectsRepository departmentSubjectsRepository)
        {
            this.highBoardRepository = highBoardRepository;
            this.cloudinaryService = cloudinaryService;
            this.facultyRepository = facultyRepository;
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
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
            var highboard = highBoardRepository.GetOne(highBoardVM.Id);

            if (highboard.Name != highBoardVM.Name)
            {
                var exist = highBoardRepository.ExistName(highBoardVM.Name);
                if (exist)
                {
                    ModelState.AddModelError("Name", "This Name is already exists");
                    highBoardVM.Picture = highboard.Picture;
                    return View("Edit", highBoardVM);
                }
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
                        ModelState.AddModelError("JobTitle", "This job is already exists");
                        highBoardVM.Picture = highboard.Picture;
                        return View("Edit", highBoardVM);
                    }
                }
            }

            try
            {
                highBoardVM.Picture = await cloudinaryService.UploadFile(highBoardVM.FormFile, highboard.Picture, "An error occurred while uploading the photo. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                highBoardVM.Picture = highboard.Picture;
                return View("Edit", highBoardVM);
            }


            highboard.Id = highBoardVM.Id;
            highboard.Description = highBoardVM.Description;
            highboard.Name = highBoardVM.Name;
            highboard.JobTitle = highBoardVM.JobTitle;
            highboard.Picture = highBoardVM.Picture;

            highBoardRepository.Update(highboard);
            highBoardRepository.Save();

            if(highboard.JobTitle == Models.Enums.JobTitle.DeanOfFaculty)
            {
                return RedirectToAction("DisplayDean");
            }
            else if(highboard.JobTitle == Models.Enums.JobTitle.HeadOfDepartment)
            {
                return RedirectToAction("DisplayHead");
            }
            else{
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var highBoard = highBoardRepository.GetOne(id);
            if (highBoard == null)
            {
                return NotFound();
            }

            var userId = highBoard.ApplicationUserId;
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
                        return View("DisplayDean", highBoardRepository.GetDeans());
                    }
                    ViewBag.Faculty = facultyRepository.GetFaculty(facultyRepository.GetAll());
                    ViewBag.Error = "Cannot delete the dean. The faculty is linked.";
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
                    return View("DisplayHead", highBoardRepository.GetHeads());
                }
            }

            highBoardRepository.Delete(id);
            highBoardRepository.Save();

            highBoardRepository.DeleteUser(userId);
            highBoardRepository.Save();

            TempData["SuccessMessage"] = "The member has been successfully deleted.";

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
