﻿using Data;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using ViewModels;

namespace HelwanUniversity.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly IDoctorRepository doctorRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly ICloudinaryService cloudinaryService;

        public DoctorController(IDoctorRepository doctorRepository,
            ISubjectRepository subjectRepository, IDepartmentRepository departmentRepository,
            IDepartmentSubjectsRepository departmentSubjectsRepository, ICloudinaryService cloudinaryService)
        {
            this.doctorRepository = doctorRepository;
            this.subjectRepository = subjectRepository;
            this.departmentRepository = departmentRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.cloudinaryService = cloudinaryService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            var DoctorDatails = doctorRepository.GetOne(id);
            return View(DoctorDatails);
        }
        public IActionResult DisplaySubject(int id)
        {
            var subjects = subjectRepository.SubjectsByDoctor(id).ToList();

            if (subjects == null || !subjects.Any())
            {
                ViewBag.Message = "There are No Subjects For this Doctor";
                return NotFound();
            }

            var subjectIds = subjectRepository.GetIds(subjects);
            var departmentSubjects = departmentSubjectsRepository.GetDepartmentSubjects(subjectIds);
            var departmentDictionary = departmentRepository.Dict();
            ViewBag.SubjectDepartments = departmentSubjectsRepository.GetDepartmentsSubject(subjects,departmentSubjects);
            ViewBag.DepartmentDictionary = departmentDictionary;

            return View(subjects);
        }
        [HttpGet]
        public IActionResult ChangePicture(int id)
        {
            var doctor = doctorRepository.GetOne(id);
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
            var doctor = doctorRepository.GetOne(ModelVM.Id);
            try
            {
                ModelVM.MainPicture = await cloudinaryService.UploadFile(ModelVM.MainPictureFile , doctor.Picture, "An error occurred while uploading the Picture. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("ChangePicture", ModelVM);
            }
            doctor.Picture = ModelVM.MainPicture;
            doctorRepository.Update(doctor);
            doctorRepository.Save();

            return RedirectToAction("Details", new {id = doctor.Id});
        }
    }
}
