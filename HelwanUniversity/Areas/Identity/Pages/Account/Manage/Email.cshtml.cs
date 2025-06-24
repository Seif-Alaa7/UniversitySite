// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Data.Repository.IRepository;
using HelwanUniversity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Models;
using Models.Enums;

namespace HelwanUniversity.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IActivityLogger logger;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IDoctorRepository doctorRepository;

        public EmailModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender,IActivityLogger logger,
            IHighBoardRepository highBoardRepository,IDoctorRepository doctorRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            this.logger = logger;
            this.highBoardRepository = highBoardRepository; 
            this.doctorRepository = doctorRepository;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

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
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
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
        public async Task<IActionResult> OnPostChangeEmailAsync()
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

            var currentEmail = await _userManager.GetEmailAsync(user);

            if (Input.NewEmail != currentEmail)
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
                try
                {
                    var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmailChange",
                        pageHandler: null,
                        values: new { area = "Identity", userId = applicationUserId, email = Input.NewEmail, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(
                        Input.NewEmail,
                        "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    logger.Log(
                        actionType: "Request Email Change",
                        tableName: "AspNetUsers",
                        recordId: recordId,
                        description: $"{userRole} '{userName}' requested to change email from '{currentEmail}' to '{Input.NewEmail}'. Confirmation email sent successfully.",
                        userId: recordId,
                        userName: userName,
                        userRole: userRole
                    );

                    StatusMessage = "Confirmation link to change email sent. Please check your email.";
                    return RedirectToPage();
                }
                catch (Exception ex)
                {
                    logger.Log(
                        actionType: "Request Email Change",
                        tableName: "AspNetUsers",
                        recordId: recordId,
                        description: $"{userRole} '{userName}' attempted to change email from '{currentEmail}' to '{Input.NewEmail}' but failed. Error: {ex.Message}",
                        userId: recordId,
                        userName: userName,
                        userRole: userRole
                    );

                    ModelState.AddModelError(string.Empty, "An error occurred while sending the confirmation email. Please try again.");
                    await LoadAsync(user);
                    return Page();
                }
            }

            StatusMessage = "Your email is unchanged.";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
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

            var applicationUserId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);

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

            try
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = applicationUserId, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    email,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                logger.Log(
                    actionType: "Send Verification Email",
                    tableName: "AspNetUsers",
                    recordId: recordId,
                    description: $"{userRole} '{userName}' requested to resend verification email to '{email}'.",
                    userId: recordId,
                    userName: userName,
                    userRole: userRole
                );

                StatusMessage = "Verification email sent. Please check your email.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while sending the verification email. Please try again.");

                logger.Log(
                    actionType: "Send Verification Email",
                    tableName: "AspNetUsers",
                    recordId: recordId,
                    description: $"{userRole} '{userName}' attempted to resend verification email to '{email}' but failed. Error: {ex.Message}",
                    userId: recordId,
                    userName: userName,
                    userRole: userRole
                );

                await LoadAsync(user);
                return Page();
            }
        }
    }
}
