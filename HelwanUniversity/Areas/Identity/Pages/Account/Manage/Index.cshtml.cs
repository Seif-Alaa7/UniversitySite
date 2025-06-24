// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.Enums;

namespace HelwanUniversity.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IActivityLogger activityLogger;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IDoctorRepository doctorRepository;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IActivityLogger activityLogger,IHighBoardRepository highBoardRepository,IDoctorRepository doctorRepository,
            IStudentRepository studentRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.activityLogger = activityLogger;
            this.highBoardRepository = highBoardRepository; 
            this.studentRepository = studentRepository;
            this.doctorRepository = doctorRepository;   
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

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
        [BindProperty]
        public InputModel Input { get; set; }

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
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            if (Input.PhoneNumber != phoneNumber)
            {
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

                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }

                activityLogger.Log(
                    actionType: "Change Phone Number",
                    tableName: "AspNetUsers",
                    recordId: recordId,
                    description: $"{userRole} '{userName}' changed their phone number from '{phoneNumber}' to '{Input.PhoneNumber}'.",
                    userId: recordId,
                    userName: userName,
                    userRole: userRole
                );
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
