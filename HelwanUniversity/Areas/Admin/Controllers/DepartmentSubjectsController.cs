﻿using Data.Repository;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels;

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DepartmentSubjectsController : Controller
    {
        private readonly IDepartmentRepository departmentRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly IDepartmentSubjectsRepository departmentSubjectsRepository;
        private readonly IAcademicRecordsRepository academicRecordsRepository;
        private readonly IDoctorRepository doctorRepository;

        public DepartmentSubjectsController(IDepartmentRepository departmentRepository,ISubjectRepository subjectRepository,
            IDepartmentSubjectsRepository departmentSubjectsRepository
            ,IAcademicRecordsRepository academicRecordsRepository,IDoctorRepository doctorRepository)
        {
            this.departmentRepository = departmentRepository;
            this.subjectRepository = subjectRepository;
            this.departmentSubjectsRepository = departmentSubjectsRepository;
            this.academicRecordsRepository = academicRecordsRepository;
            this.doctorRepository = doctorRepository;   
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Add(int id) 
        {
            ViewData["DepartId"] = id;
            ViewData["Subjects"] = subjectRepository.Select();
            return View();
        }
        [HttpPost]
        public IActionResult SaveAdd(DepartmentSubjects model) 
        {
            var ExistDepartmentSubject = departmentSubjectsRepository.Exist(model);

            if (ExistDepartmentSubject)
            {
                ViewData["DepartId"] = model.DepartmentId;
                ModelState.AddModelError("SubjectId", "This Subject is Already Exist in this Department..");
                ViewData["Subjects"] = subjectRepository.Select();
                return View("Add");
            }
            else
            {
                var DepartmentSubject = new DepartmentSubjects()
                {
                    DepartmentId = model.DepartmentId,
                    SubjectId = model.SubjectId,
                };
                departmentSubjectsRepository.Add(DepartmentSubject);
                departmentSubjectsRepository.Save();

            }
            return RedirectToAction("Details", "Department", new { area = "Admin", id = model.DepartmentId });
        }
        public IActionResult Delete(int subjectId, int departmentId)
        {
            var link = departmentSubjectsRepository.DeleteRelation(subjectId, departmentId);

            if (link == null)
            {
                TempData["ErrorMessage"] = "The relationship between the subject and department could not be found.";
                return RedirectToAction("Details", "Department", new { area = "Admin", id = departmentId });

            }

            departmentSubjectsRepository.Delete(link);
            departmentSubjectsRepository.Save();

            TempData["SuccessMessage"] = "The Subject has been successfully deleted From this department.";
            return RedirectToAction("Details", "Department", new { area = "Admin", id = departmentId });
        }
        public IActionResult DisplaySubjects(int Studentid)
        {
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            else
            {
                ViewBag.SuccessMessage = TempData["Success"];
            }

            var department = departmentRepository.DepartmentByStudent(Studentid);
            var academicRecord = academicRecordsRepository.GetAll().FirstOrDefault(x => x.StudentId == Studentid);
            if (academicRecord == null || academicRecord.Level == null || academicRecord.Semester == null)
            {
                return NotFound();
            }
            else
            {
                var level = academicRecord.Level;
                var semester = academicRecord.Semester;
                var StudentSubjects = departmentSubjectsRepository.StudentSubjects(level, semester, department.Id);
                ViewData["StudentId"] = Studentid;
                ViewData["departmentName"] = department.Name;
                var Subjects = subjectRepository.GetSubjects(Studentid);
                var Department = departmentRepository.DepartmentByStudent(Studentid);
                ViewData["departmentName"] = department.Name;
                ViewBag.DoctorNames = doctorRepository.GetName(Subjects);
                ViewBag.Subjects = Subjects;
                return View(StudentSubjects);
            }
        }
    }
}
