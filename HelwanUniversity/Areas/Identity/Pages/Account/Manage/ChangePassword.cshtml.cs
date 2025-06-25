// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Data.Repository;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Models;
using Models.Enums;

namespace HelwanUniversity.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;
        private readonly IActivityLogger activityLogger;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IDoctorRepository doctorRepository;

        public ChangePasswordModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<ChangePasswordModel> logger, IActivityLogger Logger,IHighBoardRepository highBoardRepository,
            IStudentRepository studentRepository,IDoctorRepository doctorRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            this.activityLogger = Logger; 
            this.highBoardRepository = highBoardRepository; 
            this.doctorRepository = doctorRepository;
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
        [TempData]
        public string StatusMessage { get; set; }

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
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);

            var applicationUserId = await _userManager.GetUserIdAsync(user);

            UserRole userRole = UserRole.Admin;
            string userName = "Unknown";
            int recordId = 0;
            string jobTitle = "";

            if (studentRepository.IsStudent(applicationUserId))
            {
                var student = studentRepository.GetByUserId(applicationUserId);
                userRole = UserRole.Student;
                userName = student.Name;
                recordId = student.Id;
            }
            else if (doctorRepository.IsDoctor(applicationUserId))
            {
                var doctor = await doctorRepository.GetEntityByUserIdAsync(applicationUserId) as Doctor;
                userRole = UserRole.Doctor;
                userName = doctor.Name;
                recordId = doctor.Id;
                jobTitle = doctor.JobTitle.ToString();
            }
            else if (highBoardRepository.IsHighboard(applicationUserId))
            {
                var highBoard = highBoardRepository.GetByUserId(applicationUserId);
                userName = highBoard.Name;
                recordId = highBoard.Id;
                jobTitle = highBoard.JobTitle.ToString();

                if (highBoard.JobTitle == JobTitle.President || highBoard.JobTitle == JobTitle.VicePrecident || highBoard.JobTitle == JobTitle.VP_For_AcademicAffairs)
                {
                    userRole = UserRole.Admin;
                }
                else
                {
                    userRole = UserRole.HighBoard;
                }
            }

            activityLogger.Log(
                actionType: "Change Password",
                tableName: "AspNetUsers",
                recordId: recordId,
                description: $"{userRole} '{userName}' changed their account password.",
                userId: recordId,
                userName: userName,
                userRole: userRole
            );

            StatusMessage = "Your password has been changed.";
            return RedirectToPage();
        }
    }
}
