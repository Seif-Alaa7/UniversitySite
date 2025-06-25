// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Encodings.Web;
using Data;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Models;
using Models.Enums;
using ViewModels.Vaildations.ApplicationUserValid;
using ViewModels.Vaildations.DoctorValid;
using ViewModels.Vaildations.HighBoardValid;
using ViewModels.Vaildations.StudentsValid;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Authorization;
using Data.Repository;
using System.Security.Claims;

namespace HelwanUniversity.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "Admin")]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IActivityLogger activityLogger;
        private readonly IHighBoardRepository highBoardRepository;
        public RegisterModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IOptions<IdentityOptions> identityOptions,
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService,
            IDepartmentRepository department
            ,IActivityLogger Logger
            ,IHighBoardRepository highBoardRepository
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            activityLogger = Logger;
            _emailSender = emailSender;
            _identityOptions = identityOptions;
            _context = context;
            this.cloudinaryService = cloudinaryService;
            departmentRepository = department;
            this.highBoardRepository = highBoardRepository; 
            ;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            ///
            [Required]
            public string Role { get; set; }
            [ValidateNever]
            public IEnumerable<SelectListItem> ListOfRoles { get; set; }

            [UniqueEmail]
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "User Type")]
            public UserType UserType { get; set; }
            [NotMapped]

            public IFormFile? Picture { get; set; }
            public string? PicturePath { get; set; }


            // Fields for Student
            [UniqueStudentName]
            public string? StudentName { get; set; }

            public DateOnly? StudentBirthDate { get; set; }

            [Required(ErrorMessage = "Nationality is required")]
            public string? StudentNationality { get; set; }
            public Gender StudentGender { get; set; }
            public Religion StudentReligion { get; set; }
            public string? StudentAddress { get; set; }

            [Phone]
            [UniqueSPhoneNumber]
            public string StudentPhoneNumber { get; set; }
            public int? StudentDepartmentId { get; set; }
            public bool? StudentPaymentFees { get; set; }
            public DateTime? StudentAdmissionDate { get; set; }
            public DateTime? StudentPaymentFeesDate { get; set; }

            // Fields for Doctor
            [UniqueDoctorName]
            public string? DoctorName { get; set; }
            public Gender DoctorGender { get; set; }
            public Religion DoctorReligion { get; set; }
            public string? DoctorAddress { get; set; }
            public JobTitle? DoctorJobTitle { get; set; }

            // Fields for HighBoard
            [UniqueHBName]
            public string? HighBoardName { get; set; }
            public string? HighBoardDescription { get; set; }
            public JobTitle? HighBoardJobTitle { get; set; } 
            public Faculty? HighBoardFaculty { get; set; }
            public Department? HighBoardDepartment { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("Student"));
                await _roleManager.CreateAsync(new IdentityRole("Doctor"));
            }

            Input = new InputModel
            {
                ListOfRoles = _roleManager.Roles.Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id
                }).ToList()
            };

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            LoadPageData();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                LoadPageData();
                return Page();
            }

            var user = CreateUser();

            await _userManager.SetUserNameAsync(user, Input.Email);
            await _userManager.SetEmailAsync(user, Input.Email);

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                var userId = user.Id;

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var admin = highBoardRepository.GetByUserId(currentUserId);
                var adminId = admin?.Id ?? 0;
                var adminName = admin?.Name ?? "Unknown";

                activityLogger.Log(
                         actionType: "Create Identity Account",
                         tableName: "AspNetUsers",
                         recordId: 0,
                         description: $"{admin.JobTitle} created new Identity account with email '{Input.Email}' and role '{Input.Role}'.",
                         userId: adminId,
                         userName: adminName,
                         userRole: UserRole.Admin
                );

                try
                {
                    Input.PicturePath = await cloudinaryService.UploadFile(Input.Picture, string.Empty, "There was an error uploading the file. Please try again.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);

                    activityLogger.Log(
                              actionType: "Photo Upload",
                              tableName: "User Register",
                              recordId: 0,
                              description: $"{admin.JobTitle} failed to upload photo for new user. Error: {ex.Message}",
                              userId: adminId,
                              userName: adminName,
                              userRole: UserRole.Admin
                     );
                }

                if (!string.IsNullOrEmpty(Input.Role))
                {
                    await _userManager.AddToRoleAsync(user, Input.Role);
                }

                // Handle user types and create corresponding records
                switch (Input.UserType)
                {
                    case UserType.Student:

                        var student = new Models.Student
                        {
                            Name = Input.StudentName!,
                            Nationality = Input.StudentNationality!,
                            BirthDate = Input.StudentBirthDate ?? DateOnly.MinValue,
                            Address = Input.StudentAddress,
                            PhoneNumber = Input.StudentPhoneNumber,
                            DepartmentId = Input.StudentDepartmentId ?? 0,
                            PaymentFees = Input.StudentPaymentFees ?? false,
                            Picture = Input.PicturePath,
                            AdmissionDate = Input.StudentAdmissionDate ?? DateTime.Now,
                            PaymentFeesDate = Input.StudentPaymentFeesDate,
                            ApplicationUserId = userId , 
                            Gender = Input.StudentGender,
                            Religion = Input.StudentReligion
                        };
                        var department = departmentRepository.GetOne(Input.StudentDepartmentId ?? 0);

                        if (department.Allowed == departmentRepository.GetStudentCount(Input.StudentDepartmentId))
                        {
                            ModelState.AddModelError(string.Empty, "The department has reached its maximum allowed number of students.");
                            LoadPageData();
                            return Page();
                        }
                        _context.Students.Add(student);
                        await _context.SaveChangesAsync(cancellationToken);

                        var academicRecords = new AcademicRecords
                        {
                            StudentId = student.Id,
                            GPASemester = 0,
                            GPATotal = 0,
                            SemesterPoints = 0,
                            TotalPoints = 0
                        };
                        _context.academicRecords.Add(academicRecords);
                        await _context.SaveChangesAsync(cancellationToken);

                        TempData["SuccessMessage"] = $"Student '{student.Name}' registered successfully.";

                        activityLogger.Log(
                            actionType: "Register New Student",
                            tableName: "Student",
                            recordId: student.Id,
                            description: $"{admin.JobTitle} registered new student '{student.Name}' in department '{department?.Name ?? "Unknown"}'.",
                            userId: adminId,
                            userName: adminName,
                            userRole: UserRole.Admin
                        );

                        break;

                    case UserType.Doctor:
                        var doctor = new Models.Doctor
                        {
                            Name = Input.DoctorName!,
                            Gender = Input.DoctorGender,
                            Religion = Input.DoctorReligion,
                            Address = Input.DoctorAddress,
                            JobTitle = Input.DoctorJobTitle.GetValueOrDefault(),
                            Picture = Input.PicturePath,
                            ApplicationUserId = userId
                        };

                        if (Input.DoctorName == null)
                        {
                            ModelState.AddModelError("Input.DoctorName", "Name is Required");
                            LoadPageData();
                            return Page();
                        }
                        _context.Doctors.Add(doctor);
                        await _context.SaveChangesAsync(cancellationToken);

                        TempData["SuccessMessage"] = $"Doctor '{doctor.Name}' with Job Title '{doctor.JobTitle}' registered successfully.";
                        activityLogger.Log(
                                  actionType: "Register New Doctor",
                                  tableName: "Doctor",
                                  recordId: doctor.Id,
                                  description: $"{admin.JobTitle} registered new doctor '{doctor.Name}' with Job Title '{doctor.JobTitle}'.",
                                  userId: adminId,
                                  userName: adminName,
                                  userRole: UserRole.Admin
                         );

                        break;

                    case UserType.HighBoard:
                        var highBoard = new HighBoard
                        {
                            Name = Input.HighBoardName!,
                            Description = Input.HighBoardDescription,
                            JobTitle = Input.HighBoardJobTitle.GetValueOrDefault(),
                            Picture = Input.PicturePath,
                            Faculty = Input.HighBoardFaculty,
                            Department = Input.HighBoardDepartment,
                            ApplicationUserId = userId
                        };
                        if (Input.HighBoardName == null)
                        {
                            ModelState.AddModelError("Input.HighBoardName", "Name is Required");
                            LoadPageData();
                            return Page();
                        }
                        _context.HighBoards.Add(highBoard);
                        await _context.SaveChangesAsync(cancellationToken);

                        TempData["SuccessMessage"] = $"HighBoard member '{highBoard.Name}' with Job Title '{highBoard.JobTitle}' registered successfully.";
                        activityLogger.Log(
                            actionType: "Register New HighBoard",
                            tableName: "HighBoard",
                            recordId: highBoard.Id,
                            description: $"{admin.JobTitle} registered new highboard member '{highBoard.Name}' with Job Title '{highBoard.JobTitle}'.",
                            userId: adminId,
                            userName: adminName,
                            userRole: UserRole.Admin
                        );
                        break;
                }
                _logger.LogInformation("User created a new account with password.");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                }
                else
                {
                    /*await _signInManager.SignInAsync(user, isPersistent: false);*/
                    return RedirectToPage(/*"Login"*/);
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            LoadPageData();
            return Page();
        }

        private void LoadPageData()
        {
            ViewData["Departments"] = departmentRepository.Select();

            Input = new InputModel
            {
                ListOfRoles = _roleManager.Roles.Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Name
                })
            };
        }



        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. Ensure that '{nameof(ApplicationUser)}' has a parameterless constructor.");

            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
