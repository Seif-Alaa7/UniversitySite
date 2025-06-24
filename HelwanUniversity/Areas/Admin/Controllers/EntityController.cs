using Data.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Models.Enums;

namespace HelwanUniversity.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EntityController : Controller
    {
        private readonly ISubjectRepository subjectRepository;
        private readonly IFacultyRepository facultyRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDoctorRepository doctorRepository;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IActivityLogger logger;


        public EntityController(ISubjectRepository subjectRepository ,
            IFacultyRepository facultyRepository, 
            ICloudinaryService cloudinaryService,
            IDepartmentRepository departmentRepository,
            IDoctorRepository doctorRepository,
            IHighBoardRepository highBoardRepository,
            IActivityLogger logger
            )
        {
            this.subjectRepository = subjectRepository;
            this.facultyRepository = facultyRepository;
            this.cloudinaryService = cloudinaryService;
            this.departmentRepository = departmentRepository;
            this.doctorRepository = doctorRepository;
            this.highBoardRepository = highBoardRepository;
            this.logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Add()
        {
            LoadPageData();
            return View(new AddEntity());
        }
        [HttpPost]
        public async Task<IActionResult> Add(AddEntity entity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = highBoardRepository.GetByUserId(userId);
            if (admin == null)
                return Forbid();

            switch (entity.EntityType)
            {

                case "Department":
                    var department = new Department
                    {
                        Name = entity.Name,
                        HeadId = entity.HeadId ?? 0,
                        FacultyId = entity.FacultyId ?? 0,
                        Allowed = entity.Allowed ?? 0
                    };
                    
                    var head = highBoardRepository.GetOne(department.HeadId);
                    var Faculty = facultyRepository.GetOne(department.FacultyId);

                    if (departmentRepository.ExistHeadInDepartment(entity.HeadId??0))
                    {
                        ModelState.AddModelError("HeadId", "This person is already a head of a registered department.");
                        LoadPageData();
                      
                        logger.Log(
                                actionType: "Add Department",
                                tableName: "Department",
                                recordId: 0,
                                description: $"{admin.JobTitle} attempted to add department '{department.Name}' under faculty '{Faculty.Name}' with head '{head.Name}', but head is already assigned to another department.",
                                userId: admin.Id,
                                userName: admin.Name,
                                userRole: UserRole.Admin
                        );
                        return View("Add", entity);
                    }

                    departmentRepository.Add(department);
                    departmentRepository.Save();

                    logger.Log(
                          actionType: "Add Department",
                          tableName: "Department",
                          recordId: department.Id,
                          description: $"{admin.JobTitle} added department '{department.Name}' under faculty '{Faculty.Name}' with head '{head.Name}'.",
                          userId: admin.Id,
                          userName: admin.Name,
                          userRole: UserRole.Admin
                    );
                    break;

                case "FacultyVm":

                    try
                    {
                        entity.LogoPath = await cloudinaryService.UploadFile(entity.Logo, string.Empty, "There was an error uploading the cinema logo. Please try again.");
                        entity.PicturePath = await cloudinaryService.UploadFile(entity.Picture, string.Empty, "There was an error uploading the cinema Picture. Please try again.");

                        var faculty = new Faculty
                        {
                            Name = entity.Name,
                            DeanId = entity.DeanId ?? 0,
                            Logo = entity.LogoPath,
                            Picture = entity.PicturePath,
                            Description = entity.Description,
                            ViewCount = entity.ViewCount ?? 0
                        };

                        var Dean = highBoardRepository.GetOne(faculty.DeanId);  
                        if (facultyRepository.ExistDeanInFaculty(entity.DeanId ?? 0))
                        {
                            ModelState.AddModelError("DeanId", "This person is already a Dean of a registered Faculty.");
                            LoadPageData();

                            logger.Log(
                             actionType: "Add Faculty",
                             tableName: "Faculty",
                             recordId: 0,
                             description: $"{admin.JobTitle} attempted to add faculty '{faculty.Name}' with dean '{Dean.Name}', but dean is already assigned to another faculty.",
                             userId: admin.Id,
                             userName: admin.Name,
                             userRole: UserRole.Admin
                            );

                            return View("Add", entity);
                        }

                        facultyRepository.Add(faculty);
                        facultyRepository.Save();

                        logger.Log(
                                   actionType: "Add Faculty",
                                   tableName: "Faculty",
                                   recordId: faculty.Id,
                                   description: $"{admin.JobTitle} added faculty '{faculty.Name}' with dean '{Dean.Name}'.",
                                   userId: admin.Id,
                                   userName: admin.Name,
                                   userRole: UserRole.Admin
                         );
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        LoadPageData();

                        logger.Log(
                                actionType: "Add Faculty",
                                tableName: "Faculty",
                                recordId: 0,
                                description: $"{admin.JobTitle} attempted to add faculty but failed during image upload. Error: {ex.Message}",
                                userId: admin.Id,
                                userName: admin.Name,
                                userRole: UserRole.Admin
                      );

                        return View("Add", entity);
                    }
                    break;

                case "Subject":
                    var subject = new Subject
                    {
                        Name = entity.Name,
                        DoctorId = entity.DoctorId ?? 0,
                        SubjectHours = entity.SubjectHours ?? 0,
                        StudentsAllowed = entity.StudentsAllowed ?? 0,
                        Level = entity.Level ?? 0,
                        Semester = entity.Semester ?? 0,
                        summerStatus = entity.SummerStatus ?? 0,
                        subjectType = entity.SubjectType ?? 0,
                        Salary = entity.Salary ?? 0
                    };

                    var doctor = doctorRepository.GetOne(subject.DoctorId);
                    subjectRepository.Add(subject);
                    subjectRepository.Save();

                    logger.Log(
                           actionType: "Add Subject",
                           tableName: "Subject",
                           recordId: subject.Id,
                           description: $"{admin.JobTitle} added subject '{subject.Name}' taught by doctor '{doctor.Name}' for level {subject.Level}, semester {subject.Semester}.",
                           userId: admin.Id,
                           userName: admin.Name,
                           userRole: UserRole.Admin
                    );
                    break;
                default:
                    ModelState.AddModelError("", "An Error");
                    ViewBag.EntityTypes = new List<string> { "Department", "FacultyVm", "Subject" };
                    LoadPageData();
                    return View(entity);
            }
            TempData["SuccessMessage"] = "Success !";
            LoadPageData();
            return View(new AddEntity());
        }
        private void LoadPageData()
        {
            ViewBag.EntityTypes = new List<string> { "Department", "FacultyVm", "Subject" };

            ViewData["Heads"] = highBoardRepository.selectHeads();
            ViewData["Faculties"] = facultyRepository.Select();
            ViewData["Deans"] = highBoardRepository.selectDeans();
            ViewData["Doctors"] = doctorRepository.Select();
            ViewData["Departments"] = departmentRepository.Select();
        }
    }
}
